using System.Text.Json;
using Yummiez.Models;

namespace Yummiez.Helpers
{
    public static class CartSessionHelper
    {
        public const string CartSessionKey = "ShoppingCart";

        public static List<CartItem> GetCart(ISession session)
        {
            var json = session.GetString(CartSessionKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<CartItem>();
            }

            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        public static void SaveCart(ISession session, List<CartItem> items)
        {
            var json = JsonSerializer.Serialize(items);
            session.SetString(CartSessionKey, json);
        }
    }
}
