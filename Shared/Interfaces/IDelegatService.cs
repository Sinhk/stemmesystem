using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Shared.Interfaces;

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
#pragma warning disable PBN2008
    Task<AdminDelegatDto?> HentDelegat(HentDelegatRequest request);
    Task<ICollection<AdminDelegatDto>> HentDelegater(HentDelegatRequest request);
#pragma warning restore PBN2008
    // Task<bool> IsValidNo(int arrangement, int number);
    Task<AdminDelegatDto> OppdaterDelegat(DelegatInputModel model);
    Task<AdminDelegatDto> RegistrerNyDelegat(DelegatInputModel model);
#pragma warning disable PBN2008
    Task SetTilStede(SetTilstedeRequest request, CancellationToken cancellationToken = default);
    Task SetTilStedeForAll(SetTilstedeForAllRequest request, CancellationToken cancellationToken = default);
#pragma warning restore PBN2008
}

public record HentDelegatRequest(int ArrangementId, int? DelgatId = null);
public record SetTilstedeRequest(int DelegatId, bool TilStede);
public record SetTilstedeForAllRequest(int ArrangementId, bool TilStede);