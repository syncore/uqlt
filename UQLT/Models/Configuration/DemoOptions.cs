namespace UQLT.Models.Configuration
{
    /// <summary>
    /// Model that represents the demo options.
    /// </summary>
    public class DemoOptions
    {
        /// <summary>
        /// Gets or sets file path to the custom demo config to be used with Quake Live client.
        /// </summary>
        /// <value>
        /// File path to the custom demo config to be used with Quake Live client.
        /// </value>
        public string demo_ql_cfgpath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a custom demo config should be used with the Quake Live client to play new .dm_90 demos.
        /// </summary>
        /// <value>
        /// <c>true</c> Use a custom demo cfg with Quake Live client; otherwise, <c>false</c>.
        /// </value>
        public bool demo_use_cust_ql_cfg { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a custom demo config should be used with WolfcamQL to play old .dm_73 demos.
        /// </summary>
        /// <value>
        /// <c>true</c> Use a custom demo cfg with WolfcamQL; otherwise, <c>false</c>.
        /// </value>
        public bool demo_use_cust_wolfcamql_cfg { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a custom demo config should be used with Wolf Whisperer to play old .dm_73 demos.
        /// </summary>
        /// <value>
        /// <c>true</c> Use a custom demo cfg with Wolf Whisperer; otherwise, <c>false</c>.
        /// </value>
        public bool demo_use_cust_wolfwhisperer_cfg { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether WolfcamQL should be used to play old
        /// .dm_73 demos.
        /// </summary>
        /// <value>
        ///   <c>true</c> if WolfcamQL should be used to play old .dm_73 demos; otherwise, <c>false</c>.
        /// </value>
        public bool demo_use_wolfcamql { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Wolf Whisperer should be used to play old
        /// .dm_73 demos.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Wolf Whisperer should be used to play old .dm_73 demos; otherwise, <c>false</c>.
        /// </value>
        public bool demo_use_wolfwhisperer { get; set; }

        /// <summary>
        /// Gets or sets file path to the custom demo config to be used with WolfcamQL.
        /// </summary>
        /// <value>
        /// File path to the custom demo config to be used with WolfcamQL.
        /// </value>
        public string demo_wolfcamql_cfgpath { get; set; }

        /// <summary>
        /// Gets or sets file path to the WolfcamQL executable.
        /// </summary>
        /// <value>
        /// The WolfcamQL executable file path.
        /// </value>
        public string demo_wolfcamql_exepath { get; set; }

        /// <summary>
        /// Gets or sets file path to the custom demo config to be used with Wolf Whisperer.
        /// </summary>
        /// <value>
        /// File path to the custom demo config to be used with Wolf Whisperer.
        /// </value>
        public string demo_wolfwhisperer_cfgpath { get; set; }

        /// <summary>
        /// Gets or sets the file path to the Wolf Whisperer executable.
        /// </summary>
        /// <value>
        /// The Wolf Whisperer executable file path.
        /// </value>
        public string demo_wolfwhisperer_exepath { get; set; }
    }
}