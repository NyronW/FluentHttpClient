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

public interface IFluentClientBuilderAction : ISetDefaultHeader, IHandlerRegistration
{
    ISetDefaultHeader WithBaseUrl(string url);
    ISetDefaultHeader WithProperty<TValue>(string name, TValue value);
    ISetDefaultHeader WithProperties(IEnumerable<KeyValuePair<string, object?>> arguments);
}

public interface ISetDefaultHeader : ISetTimeOut, IHandlerRegistration
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
    IHandlerRegistration WithTimeout(int timeout);
    IHandlerRegistration WithTimeout(TimeSpan timeout);
}

public interface IHandlerRegistration: IRegisterHttpClient
{
    IRegisterHttpClient WithHandler(Func<HttpMessageHandler> configureHandler);
    IRegisterHttpClient WithHandler<THandler>() where THandler: HttpMessageHandler;
}

public interface IRegisterHttpClient
{
    void Register();
}