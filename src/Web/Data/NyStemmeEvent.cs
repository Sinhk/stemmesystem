namespace Stemmesystem.Web.Data
{
    public record NyStemmeEvent(int VoteringId){}
    public record VoteringStartetEvent(int VoteringId) { }
    public record VoteringStoppetEvent(int VoteringId) { }
}