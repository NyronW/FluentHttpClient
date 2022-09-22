using Bogus;

namespace FluentHttpClient.Demo.Api.Features.Todo
{
    public interface ITodoRepository
    {
        Task<string> CreateAsync(TodoItemDto model);
        Task<IEnumerable<TodoItem>> GetAllAsync();
        Task<TodoItem> GetById(string id);
    }

    public class TodoRepository : ITodoRepository
    {
        private readonly Dictionary<string, TodoItem> items = new();
        private readonly Faker _faker = new();

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

        public Task<string> CreateAsync(TodoItemDto model)
        {
            var id = _faker.Random.Replace("###-??#");

            items.Add(id, new TodoItem { Id = id, Title = model.Title, Description = model.Description, Completed = false });

            return Task.FromResult(id);
        }

        public Task<IEnumerable<TodoItem>> GetAllAsync()
        {
            var values = items.Select(i => i.Value);
            return Task.FromResult(values);
        }

        public Task<TodoItem> GetById(string id)
        {
            if(!items.ContainsKey(id)) return Task.FromResult<TodoItem>(null);

            var item = items[id];

            return Task.FromResult(item);
        }
    }
}