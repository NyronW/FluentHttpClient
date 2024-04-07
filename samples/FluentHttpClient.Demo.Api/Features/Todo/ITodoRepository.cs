using Bogus;
using Newtonsoft.Json.Linq;

namespace FluentHttpClient.Demo.Api.Features.Todo
{
    public interface ITodoRepository
    {
        Task<string> CreateAsync(TodoItemDto model);
        Task<IEnumerable<TodoItem>> GetAllAsync();
        IAsyncEnumerable<TodoItem> GetAllAsyncStream(int pageNumber, int pageSize);
        int GetCount();
        Task<TodoItem> GetById(string id);
    }

    /// <summary>
    /// Todo repository
    /// </summary>
    public class TodoRepository : ITodoRepository
    {
        private readonly Dictionary<string, TodoItem> items = [];
        private readonly Faker _faker = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public TodoRepository()
        {
            if (!items.Any())
            {
                for (int i = 0; i < _faker.Random.Int(100, 500); i++)
                {
                    var item = new TodoItem
                    {
                        Id = _faker.Random.Replace("###-??#"),
                        Title = $"Todo item: {i + 1}",
                        Description = _faker.Lorem.Sentence(10),
                        Completed = _faker.PickRandomParam(new[] { true, false })
                    };

                    items.Add(item.Id, item);
                }
            }
        }
        /// <summary>
        /// Creates todo item
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Task<string> CreateAsync(TodoItemDto model)
        {
            var id = _faker.Random.Replace("###-??#");

            items.Add(id, new TodoItem { Id = id, Title = model.Title, Description = model.Description, Completed = false });

            return Task.FromResult(id);
        }

        /// <summary>
        /// Gets all todo items
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<TodoItem>> GetAllAsync()
        {
            var values = items.Select(i => i.Value);
            return Task.FromResult(values);
        }

        /// <summary>
        /// Streams all todo items
        /// </summary>
        /// <returns></returns>
        public async IAsyncEnumerable<TodoItem> GetAllAsyncStream(int pageNumber, int pageSize)
        {
            var values = items.Select(i => i.Value);
            int skipped = 0;
            int taken = 0;
            int skip = (pageNumber - 1) * pageSize;

            foreach (var item in values)
            {
                if (skipped < skip)
                {
                    skipped++;
                    continue;
                }

                if (taken < pageSize)
                {
                    yield return item;
                    await Task.Delay(500);//simulate slow IO operation
                    taken++;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Get single todo item for specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<TodoItem> GetById(string id)
        {
            if(!items.ContainsKey(id)) return Task.FromResult<TodoItem>(null);

            var item = items[id];

            return Task.FromResult(item);
        }

        /// <summary>
        /// get count of items
        /// </summary>
        /// <returns></returns>
        public int GetCount() => items.Count;
    }
}