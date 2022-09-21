using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints;

namespace FluentHttpClient.Demo.Api.Features.Todo;

[ProducesResponseType(typeof(TodoItem), StatusCodes.Status201Created, "application/json", "application/xml")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
[Endpoint(TagName = "Todo", OperationId = nameof(GetTodoById))]
[Authorize]
public class GetTodoById : IEndpoint
{
    private readonly ITodoRepository _repository;

    public GetTodoById(ITodoRepository repository)
    {
        _repository = repository;
    }

    public string Pattern => "/todos/{id}";

    public HttpMethod Method => HttpMethod.Get;

    public Delegate Handler => SendAsync;

    /// <summary>
    /// Gets single todo item based on its' Id
    /// </summary>
    /// <returns>Returns item by its identifier</returns>
    /// <response code="200">Returns single todo item</response>
    /// <response code="500">Internal server error occured</response>
    public async Task<IResult> SendAsync(string id)
    {
        var item = await _repository.GetById(id);
        if (item == null) return Results.NotFound("No todo item exists for specified identifier");

        return Results.Ok(item);
    }
}
