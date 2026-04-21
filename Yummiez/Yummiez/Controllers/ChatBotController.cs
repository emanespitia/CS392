using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using Yummiez.Helpers;
using Yummiez.Services;

namespace Yummiez.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class ChatBotController : ControllerBase
{
    private readonly ChatBotService _chatBotService;

    public ChatBotController(ChatBotService chatBotService)
    {
        _chatBotService = chatBotService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] ChatRequest? request)
    {
        if (request == null)
        {
            return BadRequest(new { reply = "Invalid request." });
        }

        var message = InputValidation.NormalizeSearchQuery(request.Message, maxLength: 4000);
        if (string.IsNullOrWhiteSpace(message))
        {
            return BadRequest(new { reply = "Please type a message!" });
        }

        // Build conversation history from client-sent messages
        var history = new List<ChatMessage>();
        if (request.History != null)
        {
            const int maxHistoryMessages = 30;
            var cappedHistory = request.History.TakeLast(maxHistoryMessages);
            foreach (var msg in cappedHistory)
            {
                if (msg is not { Role: "user" or "assistant" })
                {
                    continue;
                }

                var content = InputValidation.NormalizeSearchQuery(msg.Content, maxLength: 8000);
                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                if (msg.Role == "user")
                    history.Add(new UserChatMessage(content));
                else
                    history.Add(new AssistantChatMessage(content));
            }
        }

        var reply = await _chatBotService.GetResponseAsync(message, history);
        return Ok(new { reply });
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        return Ok(new { online = _chatBotService.IsConfigured });
    }
}

public class ChatRequest
{
    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    public List<ChatHistoryItem>? History { get; set; }
}

public class ChatHistoryItem
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
