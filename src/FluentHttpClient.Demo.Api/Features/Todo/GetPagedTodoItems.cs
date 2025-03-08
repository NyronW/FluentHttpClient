using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints;
using Wrapture.Pagination;

namespace FluentHttpClient.Demo.Api.Features.Todo;

[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TodoItem>))]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
[Endpoint(TagName = "Todo", OperationId = nameof(GetPagedTodoItems))]
[Authorize]
public class GetPagedTodoItems(ITodoRepository repository) : IEndpoint
{
    private readonly ITodoRepository _repository = repository;

    public string Pattern => "/todos/pages";

    public HttpMethod Method => HttpMethod.Get;

    public Delegate Handler => SendAsync;

    /// <summary>
    /// Gets all available todo items
    /// </summary>
    /// <returns>Returns all available todo items</returns>
    /// <response code="200">Returns all available items</response>
    /// <response code="500">Internal server error occured</response>
    [HandlerMethod]
    public async Task<IResult> SendAsync(HttpRequest request, int pageNo = 1, int pageSize = 10)
    {
        var items = await _repository.GetAllAsync();
        var pagedRes = items.ToPagedResult(items.Count(),pageNo,pageSize);
        return Results.Ok(pagedRes);
    }
}
