using System.Collections.Generic;

namespace UQLT.Models.DemoPlayer
{
    /// <summary>
    /// Model representing a demo play list.
    /// </summary>
    public class DemoPlaylist
    {
        public DemoPlaylist()
        {
            Demos = new List<DemoPlaylistDemo>();
        }

        /// <summary>
        /// Gets or sets the demos.
        /// </summary>
        /// <value>
        /// The demos.
        /// </value>
        public List<DemoPlaylistDemo> Demos
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the playlist_name.
        /// </summary>
        /// <value>
        /// The playlist_name.
        /// </value>
        public string playlist_name
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return playlist_name;
        }
    }
}