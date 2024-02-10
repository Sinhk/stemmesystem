using System.Diagnostics.CodeAnalysis;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using Stemmesystem.Core.Models;

namespace Stemmesystem.Core.Interfaces;

[Service]
public interface IDelegatService
{
    // Task<DelegatDto?> ValiderKode(string delegatKode, CancellationToken cancellationToken = default);
    // Task<DelegatDto?> HentDelegat(int arrangementId, int delegatId);
    //
    Task<HentDelegatResult> HentDelegatInfo(CallContext context = default);
}

public record HentDelegatResult(DelegatDto? Delegat);

[Service]
public interface IAdminDelegatService
{
    Task<AdminDelegatDto?> HentDelegat(HentDelegatRequest request);
    Task<ICollection<AdminDelegatDto>> HentDelegater(HentDelegatRequest request);
    // Task<bool> IsValidNo(int arrangement, int number);
    Task<AdminDelegatDto> OppdaterDelegat(DelegatInputModel model);
    Task<AdminDelegatDto> RegistrerNyDelegat(DelegatInputModel model);
}

public record HentDelegatRequest(int ArrangementId, int? DelgatId = null);