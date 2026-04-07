using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Yummiez.Data;
using Yummiez.Models;

namespace Yummiez.Services;

public class FaqService
{
    private readonly IMongoCollection<FaqItem> _faqCollection;

    public FaqService(IOptions<MongoDbSettings> settings)
    {
        var mongoSettings = MongoClientSettings.FromConnectionString(settings.Value.ConnectionString);
        mongoSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
        mongoSettings.ConnectTimeout = TimeSpan.FromSeconds(5);
        var mongoClient = new MongoClient(mongoSettings);
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _faqCollection = database.GetCollection<FaqItem>(settings.Value.FaqCollectionName);
    }

    /// <summary>Get all published FAQs, ordered by category then sort order.</summary>
    public async Task<List<FaqItem>> GetAllPublishedAsync()
    {
        return await _faqCollection
            .Find(f => f.IsPublished)
            .SortBy(f => f.Category)
            .ThenBy(f => f.SortOrder)
            .ToListAsync();
    }

    /// <summary>Get published FAQs for a specific category.</summary>
    public async Task<List<FaqItem>> GetByCategoryAsync(string category)
    {
        return await _faqCollection
            .Find(f => f.IsPublished && f.Category == category)
            .SortBy(f => f.SortOrder)
            .ToListAsync();
    }

    /// <summary>Search FAQs by text in question or answer (case-insensitive).</summary>
    public async Task<List<FaqItem>> SearchAsync(string query)
    {
        var filter = Builders<FaqItem>.Filter.And(
            Builders<FaqItem>.Filter.Eq(f => f.IsPublished, true),
            Builders<FaqItem>.Filter.Or(
                Builders<FaqItem>.Filter.Regex(f => f.Question, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                Builders<FaqItem>.Filter.Regex(f => f.Answer, new MongoDB.Bson.BsonRegularExpression(query, "i"))
            )
        );

        return await _faqCollection
            .Find(filter)
            .SortBy(f => f.Category)
            .ThenBy(f => f.SortOrder)
            .ToListAsync();
    }

    /// <summary>Get all distinct categories.</summary>
    public async Task<List<string>> GetCategoriesAsync()
    {
        return await _faqCollection
            .Distinct(f => f.Category, f => f.IsPublished)
            .ToListAsync();
    }

    /// <summary>Create a new FAQ item.</summary>
    public async Task CreateAsync(FaqItem faq)
    {
        faq.CreatedAt = DateTime.UtcNow;
        faq.UpdatedAt = DateTime.UtcNow;
        await _faqCollection.InsertOneAsync(faq);
    }

    /// <summary>Update an existing FAQ item.</summary>
    public async Task UpdateAsync(string id, FaqItem faq)
    {
        faq.UpdatedAt = DateTime.UtcNow;
        await _faqCollection.ReplaceOneAsync(f => f.Id == id, faq);
    }

    /// <summary>Delete a FAQ item.</summary>
    public async Task DeleteAsync(string id)
    {
        await _faqCollection.DeleteOneAsync(f => f.Id == id);
    }

    /// <summary>Seed default FAQs if the collection is empty.</summary>
    public async Task SeedDefaultFaqsAsync()
    {
        var count = await _faqCollection.CountDocumentsAsync(_ => true);
        if (count > 0) return;

        var defaultFaqs = new List<FaqItem>
        {
            // Orders
            new FaqItem
            {
                Question = "How do I place an order?",
                Answer = "Browse our restaurant listings, select items you'd like to order, add them to your cart, and proceed to checkout. You'll need to be logged in to complete your order.",
                Category = "Orders",
                SortOrder = 1,
                Tags = new List<string> { "order", "checkout", "cart" }
            },
            new FaqItem
            {
                Question = "Can I cancel my order after placing it?",
                Answer = "Orders can be cancelled within 5 minutes of placing them, as long as the restaurant hasn't started preparing your food. Go to 'My Orders' to check the status and cancel if available.",
                Category = "Orders",
                SortOrder = 2,
                Tags = new List<string> { "cancel", "refund", "order" }
            },
            new FaqItem
            {
                Question = "How can I track my order?",
                Answer = "Once your order is confirmed, you can track it in real-time from the 'My Orders' page. Click on any active order to see live driver tracking on a map with estimated arrival time.",
                Category = "Orders",
                SortOrder = 3,
                Tags = new List<string> { "tracking", "driver", "delivery", "map" }
            },

            // Account
            new FaqItem
            {
                Question = "How do I create an account?",
                Answer = "Click 'Sign Up' in the top navigation bar. You'll need to provide an email address and create a password that meets our security requirements (at least 6 characters, uppercase, lowercase, and a digit).",
                Category = "Account",
                SortOrder = 1,
                Tags = new List<string> { "register", "signup", "account" }
            },
            new FaqItem
            {
                Question = "I forgot my password. What should I do?",
                Answer = "On the login page, click 'Forgot your password?' and enter your email address. You'll receive a password reset link to create a new password.",
                Category = "Account",
                SortOrder = 2,
                Tags = new List<string> { "password", "reset", "forgot" }
            },
            new FaqItem
            {
                Question = "What are the different user roles?",
                Answer = "Yummiez has three roles: Users can browse restaurants and place orders. Drivers can deliver orders and track routes. Admins can manage restaurants, users, and the entire platform.",
                Category = "Account",
                SortOrder = 3,
                Tags = new List<string> { "roles", "admin", "driver", "user" }
            },

            // Delivery
            new FaqItem
            {
                Question = "How long does delivery take?",
                Answer = "Delivery times vary depending on the restaurant's preparation time and distance. Most orders arrive within 30–45 minutes. You can track your order in real-time once it's been picked up.",
                Category = "Delivery",
                SortOrder = 1,
                Tags = new List<string> { "delivery", "time", "eta" }
            },
            new FaqItem
            {
                Question = "How do I become a driver?",
                Answer = "If you're logged in as a regular user, click 'Become a Driver' in the navigation bar to submit your application. An admin will review and approve your request.",
                Category = "Delivery",
                SortOrder = 2,
                Tags = new List<string> { "driver", "apply", "job" }
            },
            new FaqItem
            {
                Question = "What areas do you deliver to?",
                Answer = "We currently deliver throughout the Newark, NJ area. Each restaurant has its own delivery radius shown on their details page. We're expanding to more areas soon!",
                Category = "Delivery",
                SortOrder = 3,
                Tags = new List<string> { "area", "coverage", "location" }
            },

            // Payments
            new FaqItem
            {
                Question = "What payment methods do you accept?",
                Answer = "We currently support cash on delivery. Online payment integration with credit/debit cards and digital wallets is coming soon!",
                Category = "Payments",
                SortOrder = 1,
                Tags = new List<string> { "payment", "credit card", "cash" }
            },
            new FaqItem
            {
                Question = "How do refunds work?",
                Answer = "If your order is cancelled or there's an issue with your delivery, refunds are processed automatically. For cash-on-delivery orders, no charge is applied. Contact us for any billing disputes.",
                Category = "Payments",
                SortOrder = 2,
                Tags = new List<string> { "refund", "money back", "dispute" }
            },

            // General
            new FaqItem
            {
                Question = "Is Yummiez a real food delivery service?",
                Answer = "Yummiez is a web application built as a final project for CS392 — Web Application Development. It demonstrates full-stack development including authentication, databases, real-time tracking, and more!",
                Category = "General",
                SortOrder = 1,
                Tags = new List<string> { "about", "project", "cs392" }
            },
        };

        await _faqCollection.InsertManyAsync(defaultFaqs);
    }
}
