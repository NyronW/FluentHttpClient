using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalEndpoints;
using System.Linq;

namespace FluentHttpClient.Demo.Api.Features.Todo
{
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TodoItem>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    [Endpoint(TagName = "Todo", OperationId = nameof(GetAllTodoItems))]
    [Authorize]
    public class GetAllTodoItems : IEndpoint
    {
        private readonly ITodoRepository _repository;

        public GetAllTodoItems(ITodoRepository repository)
        {
            _repository = repository;
        }

        public string Pattern => "/todos";

        public HttpMethod Method => HttpMethod.Get;

        public Delegate Handler => SendAsync;

        /// <summary>
        /// Gets all available todo items
        /// </summary>
        /// <returns>Returns all available todo items</returns>
        /// <response code="200">Returns all available items</response>
        /// <response code="500">Internal server error occured</response>
        public async Task<IEnumerable<TodoItem>> SendAsync(HttpResponse response, int pageNo = 1, int pageSize = 10)
        {
            var items = await _repository.GetAllAsync();

            response.Headers.Add("x-total-items", items.Count().ToString());

            items = items.Skip((pageNo - 1) * pageSize).Take(pageSize);

            return items;
        }
    }
}