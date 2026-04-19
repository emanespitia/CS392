using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
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
    public async Task<IActionResult> Send([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { reply = "Please type a message!" });
        }

        // Build conversation history from client-sent messages
        var history = new List<ChatMessage>();
        if (request.History != null)
        {
            foreach (var msg in request.History)
            {
                if (msg.Role == "user")
                    history.Add(new UserChatMessage(msg.Content));
                else if (msg.Role == "assistant")
                    history.Add(new AssistantChatMessage(msg.Content));
            }
        }

        var reply = await _chatBotService.GetResponseAsync(request.Message, history);
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
    public string Message { get; set; } = string.Empty;
    public List<ChatHistoryItem>? History { get; set; }
}

public class ChatHistoryItem
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
