using System.Collections.Generic;

namespace VsMapleBuilder
{
    public class CompilerParameters
    {
        public string OutputPath { get; set; }
        public IEnumerable<string> IncludeDirs { get; set; }
        public string Board { get; set; }
        public string Mcu { get; set; }
        public string Density { get; set; }
        public string VectBaseAddr { get; set; }
        public string ErrorLedPort { get; set; }
        public string ErrorLedPin { get; set; }
        public string LinkerScript { get; set; }
        public IEnumerable<string> LibraryPaths { get; set; }
    }
}
