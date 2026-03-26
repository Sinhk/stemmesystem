using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Shared.Interfaces;

[Service]
public interface IDelegateService
{
    Task<GetDelegateResult> GetDelegateInfo(CallContext context = default);
}

public record GetDelegateResult(DelegateDto? Delegate);

[Service]
public interface IAdminDelegateService
{
#pragma warning disable PBN2008
    Task<AdminDelegateDto?> GetDelegate(GetDelegateRequest request);
    Task<ICollection<AdminDelegateDto>> GetDelegates(GetDelegateRequest request);
    Task<AdminDelegateDto> UpdateDelegate(DelegateInputModel model);
    Task<AdminDelegateDto> RegisterNewDelegate(DelegateInputModel model);
    Task DeleteDelegate(DeleteDelegateRequest request, CancellationToken cancellationToken = default);
    Task SetPresent(SetPresentRequest request, CancellationToken cancellationToken = default);
    Task SetPresentForAll(SetPresentForAllRequest request, CancellationToken cancellationToken = default);
#pragma warning restore PBN2008
}

public record GetDelegateRequest(int ArrangementId, int? DelegateId = null);
public record SetPresentRequest(int DelegateId, bool Present);
public record SetPresentForAllRequest(int ArrangementId, bool Present);
public record DeleteDelegateRequest(int ArrangementId, int DelegateId);