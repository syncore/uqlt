﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using UQLT.ViewModels;

namespace UQLT
{
    //-----------------------------------------------------------------------------------------------------
    /// <summary>
    /// Necessary Caliburn Micro boilerplate
    /// </summary>
    public class UQLTBootstrapper : Bootstrapper<LoginViewModel>
    {
        private CompositionContainer container;

        //-----------------------------------------------------------------------------------------------------
        protected override void Configure()
        {
            container = new CompositionContainer(new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));

            CompositionBatch batch = new CompositionBatch();

            // standard caliburn window manager
            //batch.AddExportedValue<IWindowManager>(new WindowManager());
            // custom window manager implementation
            batch.AddExportedValue<IWindowManager>(new UQLTWindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(container);

            container.Compose(batch);
        }

        //-----------------------------------------------------------------------------------------------------
        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = container.GetExportedValues<object>(contract);

            if (exports.Count() > 0)
            {
                return exports.First();
            }

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }
    }
}