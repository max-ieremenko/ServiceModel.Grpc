using Microsoft.Extensions.DependencyInjection;
using SimpleChat.Server.Shared.Internal;

namespace SimpleChat.Server.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChatServer(this IServiceCollection services)
    {
        services.AddSingleton<IChatServerRoom, ChatServerRoom>();

        return services;
    }
}