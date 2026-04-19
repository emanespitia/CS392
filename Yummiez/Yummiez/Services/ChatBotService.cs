using OpenAI.Chat;
using Yummiez.Services;

namespace Yummiez.Services;

public class ChatBotService
{
    private readonly string _apiKey;
    private readonly FaqService _faqService;
    private const string SUPPORT_EMAIL = "yummiezsupport@gmail.com";

    private const string SYSTEM_PROMPT = @"
You are Yummiez Bot, a friendly and helpful customer support assistant for Yummiez — a food delivery platform.

Your job is to answer user questions about the Yummiez platform. Here is what you know about Yummiez:
- Yummiez is a food delivery web app built with ASP.NET Core, connecting customers with local restaurants.
- Users can browse restaurants, add items to their cart, and place orders.
- Users can track their orders in real-time on a map with live driver location.
- There are three roles: User (order food), Driver (deliver orders), and Admin (manage the platform).
- Users can apply to become a Driver from the navigation bar.
- Authentication is required to place orders, but browsing restaurants is available to everyone.
- The platform currently serves the Newark, NJ area.
- Payment is currently cash on delivery; online payments are coming soon.
- Orders can be cancelled within 5 minutes if the restaurant hasn't started preparing.
- The app is a CS392 Web Application Development final project.

Guidelines:
1. Be concise, friendly, and helpful.
2. Answer questions based on your knowledge of the Yummiez platform.
3. If you don't know the answer or the question is outside your scope, politely tell the user to email " + SUPPORT_EMAIL + @" for further help.
4. Do NOT make up features that don't exist.
5. Keep responses short (2-3 sentences max unless more detail is needed).
6. Use a warm, casual tone with occasional emojis.
";

    public ChatBotService(IConfiguration configuration, FaqService faqService)
    {
        _apiKey = configuration["OpenAI:ApiKey"] ?? "";
        _faqService = faqService;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<string> GetResponseAsync(string userMessage, List<ChatMessage>? conversationHistory = null)
    {
        if (!IsConfigured)
        {
            return "🤖 I'm currently offline. Please email us at " + SUPPORT_EMAIL + " for assistance!";
        }

        try
        {
            var client = new ChatClient("gpt-4-turbo", _apiKey);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(SYSTEM_PROMPT)
            };

            // Include conversation history for context
            if (conversationHistory != null)
            {
                messages.AddRange(conversationHistory);
            }

            messages.Add(new UserChatMessage(userMessage));

            var completion = await client.CompleteChatAsync(messages);

            return completion.Value.Content[0].Text;
        }
        catch (Exception)
        {
            return "😅 I'm having trouble connecting right now. Please try again later or email us at " + SUPPORT_EMAIL + " for help!";
        }
    }
}
