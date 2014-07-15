namespace UQLT.Core.ServerBrowser
{
    /// <summary>
    /// This enum represents the possible category types of elo searches.
    /// </summary>
    /// <remarks>There are only three possible categories based on what QLRanks supports: duel games, team games, or both duel and team games.</remarks>
    public enum EloSearchCategoryTypes
    {
        BothDuelAndTeamGames,
        DuelGamesOnly,
        TeamGamesOnly,
        
    }
}