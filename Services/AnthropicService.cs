using System.Text;
using System.Text.Json;

namespace ESSChatbot.Services;

public class AnthropicService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AnthropicService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration["Anthropic:ApiKey"] ?? "";
    }

  public async Task<string> GetChatResponseAsync(string question, string context)
{
    var prompt = $@"You are an HR Policy Assistant. Use ONLY the following HR policy information to answer the question. If the answer is not in the provided context, say so politely.

HR Policy Context:
{context}

Question: {question}

Please provide a clear and helpful answer based on the HR policy above.";

    var requestBody = new
    {
        model = "claude-sonnet-4-6",
        max_tokens = 1024,
        messages = new[]
        {
            new { role = "user", content = prompt }
        }
    };

    var json = JsonSerializer.Serialize(requestBody);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    _httpClient.DefaultRequestHeaders.Clear();
    _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
    _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

    var response = await _httpClient.PostAsync(
        "https://api.anthropic.com/v1/messages", content);

    var responseJson = await response.Content.ReadAsStringAsync();
    
    // Log the response for debugging
    Console.WriteLine($"Anthropic response: {responseJson}");
    
    try
    {
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
        
        if (result.TryGetProperty("content", out var contentArray))
        {
            if (contentArray.GetArrayLength() > 0)
            {
                var firstItem = contentArray[0];
                if (firstItem.TryGetProperty("text", out var textProp))
                {
                    return textProp.GetString() ?? "Sorry, I could not process that.";
                }
            }
        }
        
        // If we get here, return the raw response for debugging
        return $"API Response: {responseJson}";
    }
    catch (Exception ex)
    {
        return $"Error parsing response: {ex.Message}. Raw: {responseJson}";
    }
}

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        // Using a simple mock embedding for now
        // Real implementation would call an embedding API
        var random = new Random(text.GetHashCode());
        var embedding = new float[1536];
        for (int i = 0; i < 1536; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2 - 1);
        }
        return await Task.FromResult(embedding);
    }
}