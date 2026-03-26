using System.Diagnostics.CodeAnalysis;
using ProtoBuf;
using ProtoBuf.Grpc.Configuration;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Shared.Interfaces;

[Service]
public interface ICaseService
{
    Task<CaseDto?> GetCase(CaseRequest request, CancellationToken cancellationToken = default);
    Task<SaveResult<CaseDto>> CreateCase(CaseInputModel caseModel);
    Task<CaseDto> UpdateCase(CaseInputModel caseModel);
    Task<BallotDto> GetBallot(GetBallotRequest request, CancellationToken cancellationToken = default);
    Task<ICollection<BallotDto>> GetBallots(GetBallotsRequest request, CancellationToken cancellationToken = default);
    Task<SaveResult<BallotDto>> CreateBallot(BallotInputModel ballot);
    Task<SaveResult<BallotDto>> UpdateBallot(BallotInputModel ballot);
    Task<ICollection<AdminCaseDto>> GetCases(CasesRequest request);
    Task<GetResult<CaseInfoDto>> GetCaseInfo(CaseRequest request, CancellationToken cancellationToken = default);
}

[ProtoContract]
public record GetResult<T>
{
    private GetResult()
    {
    }
    public GetResult(T? value)
    {
        Value = value;
    }

    [MemberNotNullWhen(true,nameof(Value))]
    public bool Success => Value != null;
    [ProtoMember(1)]
    public T? Value { get; init; }
    
    public static GetResult<T> Null { get; } = new(default(T));

    public void Deconstruct(out T? value)
    {
        value = Value;
    }
}

[ProtoContract]
public record SaveResult<T>
{
    private SaveResult()
    {
        
    }
    public SaveResult(T? value, IDictionary<string, List<string>>? errors)
    {
        Value = value;
        Errors = errors;
    }
    public SaveResult(T? value)
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

    public static SaveResult<T> Error(string error) => new(default, new Dictionary<string, List<string>> { [""] = new() { error } });
}

public static class SaveResult
{
    public static SaveResult<T> Success<T>(T value) => new(value);
}


public record CaseRequest(int CaseId);
public record CasesRequest(int ArrangementId);
public record GetBallotRequest(int CaseId, int BallotId);
public record GetBallotsRequest(int CaseId);
