using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;

namespace ZigBeeCoordinator.Lib.Infrastructure
{
    public class IocContainer
    {
        private static readonly ILog _log = MyLogManager.GetLogger(typeof(IocContainer));
        private static readonly IocContainer _instance = new IocContainer();
        private static readonly MethodInfo _exportMethod;
        private readonly AggregateCatalog _catalog;
        private readonly CompositionContainer _container;
        private readonly string _pluginsDirectory;

        static IocContainer()
        {
            _exportMethod = typeof(CompositionContainer).GetMethods()
                .Where(m => m.Name == "GetExport")
                .Where(m => m.GetParameters().Count() == 0)
                .FirstOrDefault(m => m.GetGenericArguments().Count() == 1);
        }

        public static IocContainer Instance
        {
            get { return _instance; }
        }

        public IocContainer()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            _pluginsDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Plugins");
            _log.Info("Plugin Directory set to: " + _pluginsDirectory);
            _catalog = new AggregateCatalog(
                new AssemblyCatalog(typeof(IocContainer).Assembly)
            );
            _container = new CompositionContainer(_catalog);
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = args.Name;
            _log.Debug("Resolving: " + name);
            int firstComma = name.IndexOf(',');
            if (firstComma > 0)
            {
                name = name.Substring(0, firstComma);
            }
            string path = Path.Combine(_pluginsDirectory, name + ".dll");
            _log.Debug("Trying to load:" + path);
            if (File.Exists(path))
            {
                return Assembly.LoadFrom(path);
            }
            return null;
        }

        public void Compose(object obj)
        {
            _container.ComposeParts(obj);
        }

        public T GetExport<T>()
        {
            Lazy<T> export = _container.GetExport<T>();
            if (export == null)
            {
                throw new NullReferenceException("Could not find export of type '" + typeof(T) + "'");
            }
            return export.Value;
        }

        public object GetExport(Type type)
        {
            MethodInfo mi = _exportMethod.MakeGenericMethod(type);
            return mi.Invoke(_container, new object[0]);
        }

        public void AddAssembly(Assembly assembly)
        {
            _catalog.Catalogs.Add(new AssemblyCatalog(assembly));
        }

        public IEnumerable<Lazy<T>> GetExports<T>()
        {
            return _container.GetExports<T>();
        }
    }
}
