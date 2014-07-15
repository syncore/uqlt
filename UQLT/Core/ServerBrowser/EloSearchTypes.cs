namespace UQLT.Core.ServerBrowser
{
    /// <summary>
    /// This enum represents the single type of elo search that can be conducted from <see cref="EloSearchCategoryTypes"/> at any given time.
    /// </summary>
    /// <remarks>Only one of four possible <see cref="EloSearchCategoryTypes"/> can be searched at a time: duel minimum elo 
    /// value, duel maximum elo value, team minimum elo value, or text maximum elo value.</remarks>
    public enum EloSearchTypes
    {
        DuelMinSearch,
        DuelMaxSearch,
        TeamOneTeamMinSearch,
        TeamBothTeamsMinSearch,
        TeamOneTeamMaxSearch,
        TeamBothTeamsMaxSearch,
        // NOTE: "None" is only used for setting an empty string in the statusbar.
        None,

    }
}