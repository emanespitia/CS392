using Yummiez.Models;
using System.Text.Json;

namespace Yummiez.Helpers;

public static class RestaurantMenuCatalog
{
    public record MenuItemOption(string Name, decimal Price);
    private const int MaxMenuItems = 40;
    private const int MaxMenuItemNameLength = 120;

    public static IReadOnlyList<MenuItemOption> GetMenuItems(Restaurant restaurant)
    {
        var customMenu = GetCustomMenuItems(restaurant);
        if (customMenu.Count > 0)
        {
            return customMenu;
        }

        var category = restaurant.Category?.ToLowerInvariant() ?? string.Empty;
        return category switch
        {
            "pizza" =>
            [
                new MenuItemOption("Margherita Pizza", 13.99m),
                new MenuItemOption("Pepperoni Pizza", 15.49m),
                new MenuItemOption("Garlic Knots", 5.99m)
            ],
            "sushi" =>
            [
                new MenuItemOption("California Roll", 11.99m),
                new MenuItemOption("Salmon Nigiri (6pc)", 14.99m),
                new MenuItemOption("Miso Soup", 4.49m)
            ],
            "mexican" =>
            [
                new MenuItemOption("Chicken Tacos", 10.99m),
                new MenuItemOption("Burrito Bowl", 12.49m),
                new MenuItemOption("Chips & Guac", 6.99m)
            ],
            "healthy" =>
            [
                new MenuItemOption("Quinoa Bowl", 11.49m),
                new MenuItemOption("Avocado Salad", 9.99m),
                new MenuItemOption("Protein Smoothie", 7.99m)
            ],
            _ =>
            [
                new MenuItemOption("Classic Burger", 12.99m),
                new MenuItemOption("Crispy Fries", 4.99m),
                new MenuItemOption("Soft Drink", 2.99m)
            ]
        };
    }

    public static string SerializeMenuItems(IEnumerable<MenuItemOption> menuItems) =>
        JsonSerializer.Serialize(menuItems);

    public static IReadOnlyList<MenuItemOption> ParseMenuInput(string? menuInput)
    {
        if (string.IsNullOrWhiteSpace(menuInput))
        {
            return [];
        }

        var parsed = new List<MenuItemOption>();
        var lines = menuInput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            var parts = line.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                continue;
            }

            var name = parts[0].Trim();
            if (name.Length is < 1 or > MaxMenuItemNameLength)
            {
                continue;
            }

            if (!decimal.TryParse(parts[1], out var price) || price is < 0.01m or > 10_000m)
            {
                continue;
            }

            parsed.Add(new MenuItemOption(name, decimal.Round(price, 2)));
            if (parsed.Count >= MaxMenuItems)
            {
                break;
            }
        }

        return parsed;
    }

    public static string FormatMenuForEditor(Restaurant restaurant) =>
        string.Join(Environment.NewLine, GetMenuItems(restaurant).Select(i => $"{i.Name}|{i.Price:0.00}"));

    public static bool IsValidMenuLine(Restaurant restaurant, string itemName, decimal unitPrice) =>
        GetMenuItems(restaurant).Any(m =>
            string.Equals(m.Name, itemName, StringComparison.Ordinal)
            && m.Price == unitPrice);

    private static IReadOnlyList<MenuItemOption> GetCustomMenuItems(Restaurant restaurant)
    {
        if (string.IsNullOrWhiteSpace(restaurant.MenuItemsJson))
        {
            return [];
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<MenuItemOption>>(restaurant.MenuItemsJson);
            if (items == null)
            {
                return [];
            }

            return items
                .Where(i => !string.IsNullOrWhiteSpace(i.Name)
                            && i.Name.Length <= MaxMenuItemNameLength
                            && i.Price is >= 0.01m and <= 10_000m)
                .Take(MaxMenuItems)
                .ToList();
        }
        catch
        {
            return [];
        }
    }
}
