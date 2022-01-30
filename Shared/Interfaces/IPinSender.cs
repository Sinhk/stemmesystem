using System.ServiceModel;
using ProtoBuf.Grpc.Configuration;

namespace Stemmesystem.Shared.Interfaces;

[Service]
public interface IPinSender
{
    Task SendEmail(SendPinRequest request);
    Task SendSms(SendPinRequest request);
}

public record SendPinRequest(int DelegatId, string BaseUrl);