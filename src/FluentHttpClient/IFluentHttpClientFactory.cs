namespace FluentHttpClient;

public interface IFluentHttpClientFactory
{
    IFluentHttpClient Get(string identifier, bool createIfNotFound = true);
    IFluentHttpClient Get<TType>() where TType : class;
    IFluentHttpClient Get(Type type);
}

public interface IFluentHttpClientBuilder
{
    IFluentClientBuilderAction CreateClient(string identifier);
    IFluentClientBuilderAction CreateClient<TType>() where TType : class;
    IFluentClientBuilderAction CreateClient(Type type);
}

public interface IFluentClientBuilderAction : ISetDefaultHeader, IRegisterHttpClient
{
    ISetDefaultHeader WithBaseUrl(string url);
}

public interface ISetDefaultHeader : ISetTimeOut, IRegisterHttpClient
{
    ISetDefaultHeader WithHeader(string key, object value);
    ISetDefaultHeader AddFilter<TFilter>();
}

public interface ISetTimeOut
{
    /// <summary>
    /// Sets default request timeout
    /// </summary>
    /// <param name="timeout">Request duration in seconds</param>
    /// <returns></returns>
    IRegisterHttpClient WithTimeout(int timeout);
    IRegisterHttpClient WithTimeout(TimeSpan timeout);
}

public interface IRegisterHttpClient
{
    void Register();
}