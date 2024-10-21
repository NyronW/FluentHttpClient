using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Xml.Linq;

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

    public static IServiceCollection AddFluentHttp<TConsumer>(this IServiceCollection services, Action<IFluentClientBuilderAction> action)
        where TConsumer : class
    {
        services.AddFluentHttp(typeof(TConsumer).FullName!, action);

        services.AddTransient<IFluentHttpClient<TConsumer>>(sp =>
        {
            var consumerName = typeof(TConsumer).FullName!;

            var factory = sp.GetRequiredService<IFluentHttpClientFactory>();
            var client = factory.Get(consumerName);

            return client == null
                ? throw new InvalidOperationException($"No registered client for {consumerName}")
                : new TypedFluentHttpClient<TConsumer>(client);
        });

        return services;
    }

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

    public static IServiceCollection AddFluentHttpClientFilter<TFilter>(this IServiceCollection services) where TFilter : IHttpClientFilter
    {
        var filterType = typeof(TFilter);
        if (!services.Any(d => d.ImplementationType == filterType))
            services.AddTransient(typeof(IHttpClientFilter), filterType);

        FluentHttpClientFactory.AddFilter(filterType);

        return services;
    }

    public static IServiceCollection AddFluentHttpClientFilters(this IServiceCollection services, Assembly[] assemblies)
    {
        var filterTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IHttpClientFilter).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

        foreach (var filterType in filterTypes)
        {
            FluentHttpClientFactory.AddFilter(filterType);

            if (services.Any(d => d.ImplementationType == filterType)) continue;
            services.AddTransient(typeof(IHttpClientFilter), filterType);
        }

        return services;
    }
}
