using System.Text;
using ESSChatbot.Data;
using ESSChatbot.Models;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace ESSChatbot.Services;

public class DocumentService
{
    private readonly AppDbContext _context;
    private readonly AnthropicService _anthropicService;

    public DocumentService(AppDbContext context, AnthropicService anthropicService)
    {
        _context = context;
        _anthropicService = anthropicService;
    }

    public async Task<int> ProcessTextAsync(string text, string source)
    {
        var chunks = SplitIntoChunks(text, 500);
        int savedCount = 0;

        for (int i = 0; i < chunks.Count; i++)
        {
            var embedding = await _anthropicService.GetEmbeddingAsync(chunks[i]);

            var chunk = new HrPolicyChunk
            {
                Content = chunks[i],
                Source = source,
                ChunkIndex = i,
                Embedding = new Vector(embedding)
            };

            _context.HrPolicyChunks.Add(chunk);
            savedCount++;
        }

        await _context.SaveChangesAsync();
        return savedCount;
    }

   public async Task<List<string>> SearchSimilarChunksAsync(
    float[] queryEmbedding, int topK = 3)
{
    var vector = new Vector(queryEmbedding);

    var chunks = _context.HrPolicyChunks
        .OrderBy(c => c.Embedding!.CosineDistance(vector))
        .Take(topK)
        .Select(c => c.Content)
        .ToList();

    return await Task.FromResult(chunks);
}

    private List<string> SplitIntoChunks(string text, int chunkSize)
    {
        var chunks = new List<string>();
        var paragraphs = text.Split(
            new[] { "\n\n", "\r\n\r\n" },
            StringSplitOptions.RemoveEmptyEntries);

        var currentChunk = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (currentChunk.Length + paragraph.Length > chunkSize
                && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
            }
            currentChunk.AppendLine(paragraph);
        }

        if (currentChunk.Length > 0)
            chunks.Add(currentChunk.ToString().Trim());

        return chunks;
    }
}