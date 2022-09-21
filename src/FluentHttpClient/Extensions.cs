using System.Net;
using System.Reflection;

namespace FluentHttpClient;

internal static class Extensions
{
    /// <summary>Add raw arguments to the URI's query string.</summary>
    /// <param name="uri">The URI to extend.</param>
    /// <param name="arguments">The raw arguments to add.</param>
    public static Uri WithArguments(this Uri uri, params KeyValuePair<string, object?>[] arguments)
    {
        // concat new arguments
        string newQueryString = string.Join("&",
            from argument in arguments
            where argument.Value != null
            let key = WebUtility.UrlEncode(argument.Key)
            let value = argument.Value != null ? WebUtility.UrlEncode(argument.Value.ToString()) : string.Empty
            select key + "=" + value
        );
        if (string.IsNullOrWhiteSpace(newQueryString))
            return uri;

        // adjust URL
        UriBuilder builder = new UriBuilder(uri);
        builder.Query = !string.IsNullOrWhiteSpace(builder.Query)
            ? builder.Query.TrimStart('?') + "&" + newQueryString
            : newQueryString;

        return builder.Uri;
    }

    /// <summary>Get the key/value arguments from an object representation.</summary>
    /// <param name="arguments">The arguments to parse.</param>
    public static IEnumerable<KeyValuePair<string, object?>> GetKeyValueArguments(this object? arguments)
    {
        if (arguments == null)
            return Enumerable.Empty<KeyValuePair<string, object?>>();

        return (
            from property in arguments.GetType().GetRuntimeProperties()
            where property.CanRead && property.GetIndexParameters().Any() != true
            select new KeyValuePair<string, object?>(property.Name, property.GetValue(arguments))
        ).ToArray();
    }

    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> destination, KeyValuePair<TKey, TValue>[] source)
    {
        foreach (var item in source)
        {
            destination.Add(item.Key, item.Value);
        }
    }

    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> destination, IDictionary<TKey, TValue> source)
    {
        foreach (var item in source)
        {
            destination.Add(item.Key, item.Value);
        }
    }

    public static bool ImplementsOrDerives(this Type @this, Type from)
    {
        if (from is null)
        {
            return false;
        }
        else if (!from.IsGenericType)
        {
            return from.IsAssignableFrom(@this);
        }
        else if (!from.IsGenericTypeDefinition)
        {
            return from.IsAssignableFrom(@this);
        }
        else if (from.IsInterface)
        {
            foreach (Type @interface in @this.GetInterfaces())
            {
                if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == from)
                {
                    return true;
                }
            }
        }

        if (@this.IsGenericType && @this.GetGenericTypeDefinition() == from)
        {
            return true;
        }

        return @this.BaseType?.ImplementsOrDerives(from) ?? false;
    }
}
