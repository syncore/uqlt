using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using UQLT.Helpers;
using UQLT.Interfaces;
using UQLT.ViewModels;

namespace UQLT.Core
{
    /// <summary>
    /// Necessary Caliburn Micro boilerplate
    /// </summary>
    public class UqltBootstrapper : BootstrapperBase
    {
        private CompositionContainer _container;
        private MsgBoxService _msgBoxService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UqltBootstrapper"/> class.
        /// </summary>
        public UqltBootstrapper()
        {
            Initialize();
        }

        /// <summary>
        /// Override to configure the framework and setup your IoC container.
        /// </summary>
        protected override void Configure()
        {
            _container = new CompositionContainer(new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));

            var batch = new CompositionBatch();

            // standard caliburn window manager
            //batch.AddExportedValue<IWindowManager>(new WindowManager());
            // custom window manager implementation
            batch.AddExportedValue<IWindowManager>(new UqltWindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue<IMsgBoxService>(new MsgBoxService());
            batch.AddExportedValue(_container);

            _container.Compose(batch);
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = _container.GetExportedValues<object>(contract);

            if (exports.Any())
            {
                return exports.First();
            }

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        /// <summary>
        /// Override this to add custom behavior to execute after the application starts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            // Reduce FPS from 60 to 30 to improve performance:
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 30 });

            // Create UQLT related directories.
            UQltFileUtils.CreateUqltDataDirectories();

            // Extract embedded resources in case the user has deleted them.
            UQltFileUtils.VerifyUqltFiles();

            // TODO: will improve this later
            // QL installation detection
            if (!QLDirectoryUtils.IsQuakeLiveInstalled())
            {
                Debug.WriteLine("Unable to locate Quake Live game executable.");
                _msgBoxService = new MsgBoxService();
                _msgBoxService.ShowError("Unable to locate Quake Live game exectuable.", "Cannot find Quake Live!");
            }
            else
            {
                Debug.WriteLine("Quake live executable detected.");
            }
            // Create Quake Live related directories.
            QLDirectoryUtils.CreateBaseQ3MapDirectory(QuakeLiveTypes.Production);
            QLDirectoryUtils.CreateBaseQ3HomeDirectory(QuakeLiveTypes.Production);
            QLDirectoryUtils.CreateDemoDirectory(QuakeLiveTypes.Production);
            //TODO: more elaborate focus detection / have a secret method for allowing focus users to use UQLT
            //if (QLDirectoryUtils.IsQuakeLiveFocusInstalled())
            //{
            //    QLDirectoryUtils.CreateBaseQ3MapDirectory(QuakeLiveTypes.Focus);
            //    QLDirectoryUtils.CreateBaseQ3HomeDirectory(QuakeLiveTypes.Focus);
            //    QLDirectoryUtils.CreateDemoDirectory(QuakeLiveTypes.Focus);
            //}

            // Display the main view of the application.
            DisplayRootViewFor<LoginViewModel>();
        }
    }
}