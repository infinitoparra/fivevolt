using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;

namespace ZigBeeCoordinator.Lib.Infrastructure
{
    public class MyLogManager
    {
        private static bool _configured = false;

        public static ILog GetLogger(Type type)
        {
            if (!_configured)
            {
                BasicConfigurator.Configure();
                _configured = true;
            }
            return LogManager.GetLogger(type);
        }

        public static void ConfigureFromXmlAtLocationOfAssembly(Assembly assembly)
        {
            string dir = Path.GetDirectoryName(assembly.Location);
            string fileName = Path.Combine(dir, "ZigBeeCoordinator.log4net.xml");
            if (!Configure(fileName))
            {
                Configure("c:/logs/ZigBeeCoordinator.log4net.xml");
            }
        }

        private static bool Configure(string fileName)
        {
            if (File.Exists(fileName))
            {
                XmlConfigurator.Configure(new FileInfo(fileName));
                _configured = true;
                GetLogger(typeof(MyLogManager)).Debug("Configured from file: " + fileName);
                return true;
            }
            return false;
        }

        public static void ConfigureFromWebConfig()
        {
            if (!Configure("c:/logs/ZigBeeCoordinator.web.log4net.xml"))
            {
                XmlConfigurator.Configure(new FileInfo("web.config"));
                GetLogger(typeof(MyLogManager)).Debug("Configured from web.config");
            }
        }
    }
}
