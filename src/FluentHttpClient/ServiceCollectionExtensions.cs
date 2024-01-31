using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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

        var bldr = services.AddHttpClient(name);
        if (fc.PrimaryMessageHandler != null)
        {
            bldr.ConfigurePrimaryHttpMessageHandler(fc.PrimaryMessageHandler);
        }

        return services;
    }

    public static IServiceCollection AddFluentHttp<TType>(this IServiceCollection services, Action<IServiceProvider, IFluentClientBuilderAction> action)
            => services.AddFluentHttp(typeof(TType).FullName!, action);

    public static IServiceCollection AddFluentHttp(this IServiceCollection services, string name, Action<IServiceProvider, IFluentClientBuilderAction> action)
    {
        services.AddFluentHttp();

        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<IFluentHttpClientBuilder>();
        var client = builder.CreateClient(name);

        action(sp, client);

        var fc = (FluentHttpClientFactory)client;
        if (!fc.IsRegistered) fc.Register();

        var bldr = services.AddHttpClient(name);
        if (fc.PrimaryMessageHandler != null)
        {
            bldr.ConfigurePrimaryHttpMessageHandler(fc.PrimaryMessageHandler);
        }

        return services;
    }


    public static IServiceCollection AddFluentHttpClientFilters(this IServiceCollection services, Assembly[] assemblies)
    {
        var filterTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IHttpClientFilter).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

        foreach (var filterType in filterTypes)
        {
            services.AddTransient(typeof(IHttpClientFilter), filterType);
        }

        return services;
    }
}
