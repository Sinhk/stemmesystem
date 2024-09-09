namespace Stemmesystem.Server.Features.MinSpeiding;

public static class MinSpeidingEndpoints
{
    public static void MapMinSpeidingEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/arrangement/{arrangementId:int}/participants", async (int arrangementId, MinSpeidingService service) =>
        {
            var result = await service.GetArrangementParticipants(arrangementId);
            
            return result.Match(
                participants => Results.Ok(participants),
                exception => Results.BadRequest(exception.Message)
            );
        })
        .RequireAuthorization("admin");
    }
}