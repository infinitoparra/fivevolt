using System.ServiceProcess;
using ZigBeeCoordinator.Lib.Infrastructure;

namespace ZigBeeCoordinator.Service
{
    public static class Program
    {
        public static void Main()
        {
            MyLogManager.ConfigureFromXmlAtLocationOfAssembly(typeof(Program).Assembly);
            MyLogManager.GetLogger(typeof(Program)).Debug("Main");
            ServiceBase.Run(new ZigBeeCoordinatorService());
        }
    }
}
