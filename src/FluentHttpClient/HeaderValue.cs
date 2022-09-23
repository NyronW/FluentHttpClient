//https://github.com/joseftw/jos.result

namespace FluentHttpClient;

public class HeaderValue<T>
{
    public HeaderValue()
    {
        HasValue = false;
    }

    public HeaderValue(T value)
    {
        Value = value;
        HasValue = true;
    }

    public T Value { get; }
    public bool HasValue { get; }
}
