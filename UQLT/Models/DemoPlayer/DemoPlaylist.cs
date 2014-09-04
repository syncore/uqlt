using System.Collections.Generic;

namespace UQLT.Models.DemoPlayer
{
    /// <summary>
    /// Model representing a demo play list.
    /// </summary>
    public class DemoPlaylist
    {
        public DemoPlaylist(string name)
        {
            Name = name;
            Demos = new List<PlaylistDemo>();
        }

        /// <summary>
        /// Gets or sets the demos.
        /// </summary>
        /// <value>
        /// The demos.
        /// </value>
        public List<PlaylistDemo> Demos
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the playlist.
        /// </summary>
        /// <value>
        /// The playlist_name.
        /// </value>
        public string Name
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
            return Name;
        }
    }
}