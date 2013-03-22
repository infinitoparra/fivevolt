
using System.Collections.Generic;

namespace ZigBeeCoordinator.Console
{
    public interface IConsoleCommand
    {
        void Execute(List<string> args);
        void PrintHelp();
        string Name { get; }
        string Description { get; }
    }
}
