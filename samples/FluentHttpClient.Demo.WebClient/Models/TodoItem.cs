using System.Runtime.Serialization;

namespace FluentHttpClient.Demo.WebClient.Models;

public class TodoItem
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? Completed { get; set; }

    public override string ToString()
    {
        return $"{Id}-{Title}-{Completed}";
    }
}

[DataContract]
public class TodoItemDto
{
    [DataMember]
    public string Title { get; set; } = "";
    [DataMember]
    public string Description { get; set; } = "";
}
