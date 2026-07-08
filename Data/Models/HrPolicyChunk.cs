using Pgvector;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESSChatbot.Models;

public class HrPolicyChunk
{
    [Column("id")]
    public long Id { get; set; }
    
    [Column("content")]
    public string Content { get; set; } = string.Empty;
    
    [Column("source")]
    public string? Source { get; set; }
    
    [Column("chunk_index")]
    public int ChunkIndex { get; set; }
    
    [Column("embedding")]
    public Vector? Embedding { get; set; }
}