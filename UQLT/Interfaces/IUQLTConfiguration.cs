namespace UQLT.Interfaces
{
    /// <summary>
    /// Necessary methods for saving various configuration options.
    /// </summary>
    internal interface IUQLTConfiguration
    {
        /// <summary>
        /// Checks whether the configuration already exists
        /// </summary>
        /// <returns><c>true</c> if configuration exists, otherwise <c>false</c></returns>
        bool ConfigExists();

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        void LoadConfig();

        /// <summary>
        /// Loads the default configuration.
        /// </summary>
        void LoadDefaultConfig();

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        void SaveConfig();
    }
}