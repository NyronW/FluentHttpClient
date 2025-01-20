using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints;
using MinimalEndpoints.Extensions.Validation;

namespace FluentHttpClient.Demo.Api.Features.Todo;

[Accept(typeof(TodoItemDto), "application/json", AdditionalContentTypes = new[] { "application/xml" })]
[ProducesResponseType(typeof(TodoItem), StatusCodes.Status201Created, "application/json", "application/xml")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
[Endpoint(TagName = "Todo", OperationId = nameof(CreateTodoItem))]
[Authorize]
public class CreateTodoItem(ILoggerFactory loggerFactory, ITodoRepository repository) : EndpointBase<TodoItemDto, IResult>(loggerFactory)
{
    private readonly ITodoRepository _repository = repository;

    public override string Pattern => "/todos";

    public override HttpMethod Method => HttpMethod.Post;

    /// <summary>
    /// Creates new todo item
    /// </summary>
    /// <param name="request">Todo item</param>
    /// <param name="httpRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>New created item</returns>
    /// <remarks>
    /// Sample request:
    ///     POST /todos
    ///     {        
    ///       "title": "New Task",
    ///       "description": "This is a detail description"
    ///     }
    /// </remarks>
    /// <response code="201">Returns the newly create item</response>
    /// <response code="400">Invalid data passed from client</response>
    /// <response code="500">Internal server error occured</response>

    [HandlerMethod]
    public override async Task<IResult> HandleRequestAsync(TodoItemDto request, HttpRequest httpRequest, CancellationToken cancellationToken = default)
    {
        if (request.Title.StartsWith("error", StringComparison.OrdinalIgnoreCase))
        {
            return Results.StatusCode(StatusCodes.Status502BadGateway);
        }

        string id = await _repository.CreateAsync(request);

        //use content negotiation
        return Results.Created($"/todos/{id}", new TodoItem { Id = id, Title = request.Title, Description = request.Description, Completed = false });
    }

    //This check can be moved to an external validator library such as FluentValidation
    public override Task<IEnumerable<ValidationError>> ValidateAsync(TodoItemDto request)
    {
        var errors = new List<ValidationError>();

        if (request == null)
        {
            errors.Add(new ValidationError("", "Missing or invalid data"));
        }

        if (string.IsNullOrEmpty(request?.Title))
        {
            errors.Add(new ValidationError(nameof(request.Title), "Title is required"));
        }

        if (string.IsNullOrEmpty(request?.Description))
        {
            errors.Add(new ValidationError(nameof(request.Description), "Description is required"));
        }

        return Task.FromResult(errors.AsEnumerable());
    }
}
