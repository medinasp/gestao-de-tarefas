using System.Text.Json.Serialization;
using GestaoDeTarefas.Api.Domain;

namespace GestaoDeTarefas.Api.Dtos;

public sealed class TaskResponse
{
    public required string Code { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateOnly? DueDate { get; init; }
    public required TodoTaskStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("_links")]
    public Dictionary<string, LinkDto>? Links { get; set; }
}
