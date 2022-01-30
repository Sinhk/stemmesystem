using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;

namespace Stemmesystem.Client;

public static class GrpcClientServiceExtensions
{
    public static IServiceCollection AddGrpcClient<TClient>(this IServiceCollection services) where TClient : class
    {
        services.AddTransient(ser =>
        {
            var channel = ser.GetRequiredService<GrpcChannel>();
            return channel.CreateGrpcService<TClient>();
        });

        return services;
    }
}