using Microsoft.Extensions.DependencyInjection;

namespace FluentHttpClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFluentHttp(this IServiceCollection services)
    {
        var hasFactory = services.Any(d => d.ServiceType == typeof(IFluentHttpClientFactory)
            && d.ImplementationType == typeof(FluentHttpClientFactory));

        if (!hasFactory)
            services.AddTransient<IFluentHttpClientFactory, FluentHttpClientFactory>()
                    .AddTransient<IFluentHttpClientBuilder, FluentHttpClientFactory>()
                    .AddTransient<FluentHttpClient>()
                    .AddTransient<TimerHttpClientFilter>()
                    .AddSingleton<AccessTokenStorage>()
                    .AddHttpClient();

        return services;
    }

    public static IServiceCollection AddFluentHttp<TType>(this IServiceCollection services, Action<IFluentClientBuilderAction> action)
        => services.AddFluentHttp(typeof(TType).FullName!, action);

    public static IServiceCollection AddFluentHttp(this IServiceCollection services, string name, Action<IFluentClientBuilderAction> action)
    {
        services.AddFluentHttp();

        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<IFluentHttpClientBuilder>();

        var client = builder.CreateClient(name);

        action(client);

        var fc = (FluentHttpClientFactory)client;
        if (!fc.IsRegistered) fc.Register();

        return services;
    }
}
