using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Yummiez.Models;

public class FaqItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("question")]
    public string Question { get; set; } = string.Empty;

    [BsonElement("answer")]
    public string Answer { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = "General";

    [BsonElement("sortOrder")]
    public int SortOrder { get; set; }

    [BsonElement("isPublished")]
    public bool IsPublished { get; set; } = true;

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
