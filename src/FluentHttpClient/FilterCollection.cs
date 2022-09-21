using System.Collections.ObjectModel;

namespace FluentHttpClient;

public class HttpRequestOption
{
    public FilterCollection Filters { get; } = new FilterCollection();
}

public class FilterCollection : Collection<Type>
{
    public void Add<TGeneratorType>() where TGeneratorType : IHttpClientFilter
       => Add(typeof(TGeneratorType), order: 0);

    public void Add<TGeneratorType>(int order) where TGeneratorType : IHttpClientFilter
        => Add(typeof(TGeneratorType), order);

    public void Add(Type filterType, int order)
    {
        if (filterType == null)
        {
            throw new ArgumentNullException(nameof(filterType));
        }

        if (!typeof(IHttpClientFilter).IsAssignableFrom(filterType))
        {
            throw new ArgumentException("Type must inherit from HtmlViewGeneratorBase", nameof(filterType));
        }

        Insert(order, filterType);
    }
}
