using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TodoApi.Models;

public class Todo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = null!;

    [BsonElement("completed")]
    public bool Completed { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("__v")]
    [BsonIgnoreIfDefault]
    public int Version { get; set; }
}

public class CreateTodoDto
{
    public string Title { get; set; } = null!;
}

public class UpdateTodoDto
{
    public string? Title { get; set; }
    public bool? Completed { get; set; }
} 