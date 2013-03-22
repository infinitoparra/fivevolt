using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using log4net;
using ZigBeeCoordinator.Lib.Infrastructure;

namespace ZigBeeCoordinator.Console
{
    public class Program
    {
        private static readonly ILog _log = MyLogManager.GetLogger(typeof(Program));

        [ImportMany(typeof(IConsoleCommand))]
        public List<IConsoleCommand> Commands { get; set; }

        public static void Main(string[] args)
        {
            _log.Debug("Begin Console");
            Program program = new Program();
            IocContainer.Instance.AddAssembly(typeof(Program).Assembly);
            IocContainer.Instance.Compose(program);
            program.Execute(new List<string>(args));
            _log.Debug("End Console");
        }

        private void Execute(List<string> args)
        {
            if (args.Count == 0)
            {
                PrintHelp();
                return;
            }
            IConsoleCommand command = GetCommand(args[0]);
            if (command == null)
            {
                _log.Error("Could not find command '" + args[0] + "'");
                PrintHelp();
                return;
            }
            args.RemoveAt(0);

            _log.Debug("Executing command [" + command.GetType() + "]");
            command.Execute(args);
        }

        private IConsoleCommand GetCommand(string commandName)
        {
            return Commands.FirstOrDefault(c => string.Equals(c.Name, commandName, StringComparison.InvariantCultureIgnoreCase));
        }

        private void PrintHelp()
        {
            System.Console.Out.WriteLine("ZigBeeCoordinator.Console");
            foreach (IConsoleCommand command in Commands.OrderBy(c => c.Name))
            {
                System.Console.Out.WriteLine(string.Format("  {0,10} - {1}", command.Name, command.Description));
            }
        }
    }
}
