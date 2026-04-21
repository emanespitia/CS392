using Yummiez.Models;

namespace Yummiez.Helpers;

public static class RestaurantMenuCatalog
{
    public record MenuItemOption(string Name, decimal Price);

    public static IReadOnlyList<MenuItemOption> GetMenuItems(Restaurant restaurant)
    {
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

    public static bool IsValidMenuLine(Restaurant restaurant, string itemName, decimal unitPrice) =>
        GetMenuItems(restaurant).Any(m =>
            string.Equals(m.Name, itemName, StringComparison.Ordinal)
            && m.Price == unitPrice);
}
