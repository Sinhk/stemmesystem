/*
using Duende.IdentityServer.Extensions;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using ProtoBuf.Grpc;
using Stemmesystem.Core;

namespace Stemmesystem.Server.Services;

[Authorize]
public class AuthorizedGreeterService : IAuthorizedGreeterService
{
    public ValueTask<HelloReply> SayHelloAsync(CallContext context)
    {
        return ValueTask.FromResult(new HelloReply
        {
            Message = "Hello " + context.ServerCallContext?.GetHttpContext().User.GetDisplayName()
        });
    }
}

public class GreeterService : IGreeterService
{
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }


    public ValueTask<HelloReply> SayHelloAsync(HelloRequest request)
    {
        return ValueTask.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}
*/
