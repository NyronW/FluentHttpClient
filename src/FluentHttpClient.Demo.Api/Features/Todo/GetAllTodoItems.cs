using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints;

namespace FluentHttpClient.Demo.Api.Features.Todo;

[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TodoItem>))]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
[Endpoint(TagName = "Todo", OperationId = nameof(GetAllTodoItems))]
[Authorize]
public class GetAllTodoItems(ITodoRepository repository) : IEndpoint
{
    private readonly ITodoRepository _repository = repository;

    public string Pattern => "/todos";

    public HttpMethod Method => HttpMethod.Get;

    public Delegate Handler => SendAsync;

    /// <summary>
    /// Gets all available todo items
    /// </summary>
    /// <returns>Returns all available todo items</returns>
    /// <response code="200">Returns all available items</response>
    /// <response code="500">Internal server error occured</response>
    [HandlerMethod]
    public async IAsyncEnumerable<TodoItem> SendAsync(HttpRequest request, int pageNo = 1, int pageSize = 10)
    {
        var count = _repository.GetCount();
        var items = _repository.GetAllAsyncStream(pageNo, pageSize);
        request.HttpContext.Response.Headers["x-total-items"] = count.ToString();

        await foreach (var item in items)
        {
            yield return item;
        }
    }
}
