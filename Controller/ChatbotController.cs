using Microsoft.AspNetCore.Mvc;
using ESSChatbot.Services;
using ESSChatbot.Data;

namespace ESSChatbot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly AnthropicService _anthropicService;
    private readonly DocumentService _documentService;
    private readonly AppDbContext _context;

    public ChatbotController(
        AnthropicService anthropicService,
        DocumentService documentService,
        AppDbContext context)
    {
        _anthropicService = anthropicService;
        _documentService = documentService;
        _context = context;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request)
    {
        if (string.IsNullOrEmpty(request.Question))
            return BadRequest("Question cannot be empty");

        // Get embedding for the question
        var queryEmbedding = await _anthropicService.GetEmbeddingAsync(request.Question);

        // Search for relevant chunks
        var relevantChunks = await _documentService
            .SearchSimilarChunksAsync(queryEmbedding, 3);

        string context;
        if (relevantChunks.Any())
        {
            context = string.Join("\n\n", relevantChunks);
        }
        else
        {
            // Fallback to default HR policy if no chunks found
            context = @"
                - Sick Leave: 12 days per year
                - Casual Leave: 8 days per year
                - Annual Leave: 15 days per year
                - Working Hours: 9:30 AM to 6:30 PM, Monday to Friday
                - Remote Work: Up to 2 days per week with manager approval
                - Notice Period: 2 months
                - Probation: 6 months for new employees
                - Maternity Leave: 26 weeks
                - Paternity Leave: 2 weeks
            ";
        }

        // Get AI response
        var answer = await _anthropicService.GetChatResponseAsync(
            request.Question, context);

        return Ok(new { answer, chunksFound = relevantChunks.Count });
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadPolicy([FromBody] PolicyUploadRequest request)
    {
        if (string.IsNullOrEmpty(request.Content))
            return BadRequest("Content cannot be empty");

        var count = await _documentService
            .ProcessTextAsync(request.Content, request.Source ?? "HR Policy");

        return Ok(new { message = $"Successfully processed {count} chunks", count });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}

public record ChatRequest(string Question);
public record PolicyUploadRequest(string Content, string? Source);