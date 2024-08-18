using System.Diagnostics.CodeAnalysis;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using Stemmesystem.Core.Models;

namespace Stemmesystem.Core.Interfaces;

[Service]
[SuppressMessage("Usage", "PBN2008:ServiceContractAnalyzer.PossiblyNotSerializable")]
public interface IDelegatService
{
    // Task<DelegatDto?> ValiderKode(string delegatKode, CancellationToken cancellationToken = default);
    // Task<DelegatDto?> HentDelegat(int arrangementId, int delegatId);
    //
    Task<HentDelegatResult> HentDelegatInfo(CallContext context = default);
}

public record HentDelegatResult(DelegatDto? Delegat);

[Service]
[SuppressMessage("Usage", "PBN2008:ServiceContractAnalyzer.PossiblyNotSerializable")]
public interface IAdminDelegatService
{
    Task<AdminDelegatDto?> HentDelegat(HentDelegatRequest request, CancellationToken cancellationToken = default);
    Task<ICollection<AdminDelegatDto>> HentDelegater(HentDelegatRequest request, CancellationToken cancellationToken = default);
    // Task<bool> IsValidNo(int arrangement, int number);
    Task<AdminDelegatDto> OppdaterDelegat(DelegatInputModel model, CancellationToken cancellationToken = default);
    Task<AdminDelegatDto> RegistrerNyDelegat(DelegatInputModel model, CancellationToken cancellationToken = default);
}

public record HentDelegatRequest(int ArrangementId, int? DelgatId = null);