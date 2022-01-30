/*using ProtoBuf;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace Stemmesystem.Shared;

[ProtoContract]
public class HelloReply
{
    [ProtoMember(1)]
    public string? Message { get; set; }
}

[ProtoContract]
public class HelloRequest
{
    [ProtoMember(1)]
    public string? Name { get; set; }
}

[Service]
public interface IGreeterService
{
    ValueTask<HelloReply> SayHelloAsync(HelloRequest request);
}

[Service]
public interface IAuthorizedGreeterService
{
    ValueTask<HelloReply> SayHelloAsync(CallContext context = default);
}*/