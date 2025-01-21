using System.Diagnostics.CodeAnalysis;
using ProtoBuf;
using ProtoBuf.Grpc.Configuration;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Shared.Interfaces;

[Service]
public interface ISakService
{
    Task<SakDto?> HentSak(SakRequest request, CancellationToken cancellationToken = default);
    //Task<bool> ErNummerBrukt(int arrangementId, string? nummer);
    Task<LagreResult<SakDto>> LagreNySak(SakInputModel sak);
    Task<SakDto> OppdaterSak(SakInputModel sak);
    Task<VoteringDto?> HentVotering(HentVoteringRequest request, CancellationToken cancellationToken = default);
    Task<ICollection<VoteringDto>> HentVoteringer(HentVoteringerRequest request, CancellationToken cancellationToken = default);
    Task<LagreResult<VoteringDto>> LagreNyVotering(VoteringInputModel votering);
    Task<LagreResult<VoteringDto>> OppdaterVotering(VoteringInputModel votering);
    //Task<ICollection<SakDto>> HentSaker(int arrangementId);
    Task<ICollection<AdminSakDto>> HentSaker(SakerRequest request);
    Task<HentResult<SakInfoDto>> HentSakInfo(SakRequest request, CancellationToken cancellationToken = default);
}

[ProtoContract]
public record HentResult<T>
{
    private HentResult()
    {
    }
    public HentResult(T? value)
    {
        Value = value;
    }

    [MemberNotNullWhen(true,nameof(Value))]
    public bool Success => Value != null;
    [ProtoMember(1)]
    public T? Value { get; init; }
    
    public static HentResult<T> Null { get; } = new(default(T));

    public void Deconstruct(out T? value)
    {
        value = Value;
    }
}

[ProtoContract]
public record LagreResult<T>
{
    private LagreResult()
    {
        
    }
    public LagreResult(T? value, IDictionary<string, List<string>>? errors)
    {
        Value = value;
        Errors = errors;
    }
    public LagreResult(T? value)
    {
        Value = value;
    }

    [MemberNotNullWhen(true,nameof(Value))]
    public bool Success => Value != null && Errors?.Any() == false ;
    [ProtoMember(1)]
    public T? Value { get; init; }
    [ProtoMember(2)]
    public IDictionary<string, List<string>>? Errors { get; init; }

    public void Deconstruct(out T? value, out IDictionary<string, List<string>>? errors)
    {
        value = Value;
        errors = Errors;
    }

    public static LagreResult<T> Error(string error) => new(default, new Dictionary<string, List<string>> { [""] = new() { error } });
}

public static class LagreResult
{
    public static LagreResult<T> Success<T>(T value) => new(value);
}


public record SakRequest(int SakId);
public record SakerRequest(int ArrangementId);
public record HentVoteringRequest(int SakId, int VoteringId);
public record HentVoteringerRequest(int SakId);
