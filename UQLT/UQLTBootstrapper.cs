using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Caliburn.Micro;
using UQLT.ViewModels;

namespace UQLT
{
    /// <summary>
    /// Necessary Caliburn Micro boilerplate
    /// </summary>
    public class UqltBootstrapper : BootstrapperBase
    {
        private CompositionContainer _container;

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

            if (exports.Count() > 0)
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
            // Display the main view of the application.
            DisplayRootViewFor<LoginViewModel>();
        }
    }
}