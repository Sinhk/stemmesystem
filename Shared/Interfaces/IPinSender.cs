using ProtoBuf.Grpc.Configuration;

namespace Stemmesystem.Core.Interfaces;

[Service]
public interface IPinSender
{
    Task<SendPinResult> SendEmail(SendPinRequest request, CancellationToken cancellationToken = default);
    Task<SendPinResult> SendSms(SendPinRequest request, CancellationToken cancellationToken = default);
}

public record SendPinRequest(int DelegatId, string BaseUrl);
public record SendPinResult(bool Success);