using UQLT.Core.ServerBrowser;

namespace UQLT.Models.Configuration
{
    /// <summary>
    /// Model representing an item in the server browser elo search combobox.
    /// </summary>
    public class ServerBrowserEloItem
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of gametype represented by this item.
        /// </summary>
        /// <value>The elo type.</value>
        public EloSearchTypes EloSearchGameType
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}