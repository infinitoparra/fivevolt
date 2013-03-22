
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace VsMapleBuilder
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    throw new CompilerException("NOFILE", 0, ErrorLevel.Error, 1, "not enough arguments.");
                }
                string projectFileName = Path.GetFullPath(args[0]);
                string projectPath = Path.GetDirectoryName(projectFileName);
                string projectName = Path.GetFileNameWithoutExtension(projectFileName);
                Console.WriteLine("building " + projectFileName);

                XDocument projectDoc = LoadProject(projectFileName);
                if (projectDoc.Root == null)
                {
                    throw new Exception("Parse error. No root element.");
                }

                var files = projectDoc.Root
                    .Elements(projectDoc.Root.Name.Namespace + "ItemGroup")
                    .SelectMany(itemGroup =>
                        {
                            var includes = itemGroup
                                .Elements(projectDoc.Root.Name.Namespace + "ClInclude")
                                .Select(item => Path.Combine(projectPath, item.AttributeValue("Include")));
                            var compiles = itemGroup
                                .Elements(projectDoc.Root.Name.Namespace + "ClCompile")
                                .Select(item => Path.Combine(projectPath, item.AttributeValue("Include")));
                            var nones = itemGroup
                                .Elements(projectDoc.Root.Name.Namespace + "None")
                                .Select(item => Path.Combine(projectPath, item.AttributeValue("Include")));
                            return nones.Concat(includes.Concat(compiles));
                        }
                    )
                    .ToList();

                var compilableFiles = files
                    .Where(fileName => IsCompilableFile(fileName));
                var includeDirs = files
                    .Where(fileName => IsHeaderFile(fileName))
                    .Select(fileName => Path.GetDirectoryName(fileName))
                    .Distinct(StringComparer.InvariantCultureIgnoreCase);
                var linkerIncludes = files
                    .Where(fileName => IsLinkerIncludeFile(fileName));

                var compilerParameters = new CompilerParameters
                {
                    Board = "maple_RET6",
                    Density = "STM32_HIGH_DENSITY",
                    Mcu = "STM32F103RE",
                    ErrorLedPin = "5",
                    ErrorLedPort = "GPIOA",
                    OutputPath = Path.Combine(projectPath, "bin"),
                    VectBaseAddr = "VECT_TAB_FLASH",
                    IncludeDirs = includeDirs,
                    LinkerScript = Path.Combine(projectPath, "maple\\maple_RET6\\flash.ld"),
                    LibraryPaths = new[] { Path.Combine(projectPath, "maple\\maple_RET6") }
                };

                if (!Directory.Exists(compilerParameters.OutputPath))
                {
                    Directory.CreateDirectory(compilerParameters.OutputPath);
                }

                var elfOutputFileName = Path.Combine(compilerParameters.OutputPath, projectName + ".elf");
                var binOutputFileName = Path.Combine(compilerParameters.OutputPath, projectName + ".bin");

                foreach (var linkerInclude in linkerIncludes)
                {
                    File.Copy(linkerInclude, Path.Combine(compilerParameters.OutputPath, Path.GetFileName(linkerInclude)), true);
                }

                var outputFiles = new List<string>();
                if (!CompileFiles(compilerParameters, compilableFiles, outputFiles))
                {
                    return 1;
                }
                Link(compilerParameters, elfOutputFileName, outputFiles);
                ConvertToBinary(elfOutputFileName, binOutputFileName);

                Console.WriteLine("build complete");

                PrintSize(binOutputFileName);

                return 0;
            }
            catch (CompilerException ex)
            {
                Console.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static void PrintSize(string binOutputFileName)
        {
            const string compilerExe = "arm-none-eabi-size";
            var parameters = new List<string>{
                "--target=binary",
                "-A",
	            binOutputFileName
            };

            Exec(Path.GetDirectoryName(binOutputFileName), binOutputFileName, compilerExe, parameters);
        }

        private static bool CompileFiles(CompilerParameters compilerParameters, IEnumerable<string> files, List<string> outputFiles)
        {
            Console.WriteLine("starting compile");
            bool anyErrors = false;
            foreach (var file in files)
            {
                var errors = new List<CompilerException>();
                try
                {
                    CompileFile(compilerParameters, file, errors, outputFiles);
                }
                catch (CompilerException ex)
                {
                    Console.WriteLine(ex);
                    anyErrors = true;
                }
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                    anyErrors = true;
                }
            }
            Console.WriteLine("compile complete");
            return !anyErrors;
        }

        private static void CompileFile(CompilerParameters compilerParameters, string fileName, List<CompilerException> errors, List<string> outputFiles)
        {
            Console.WriteLine("compiling " + fileName + "...");
            var extension = Path.GetExtension(fileName);
            if (extension == null)
            {
                errors.Add(new CompilerException(fileName, 0, ErrorLevel.Error, 3, "Invalid file extension 'null'."));
                return;
            }
            var ext = extension.ToLowerInvariant();
            switch (ext)
            {
                case ".cpp":
                    CompileCpp(compilerParameters, fileName, outputFiles);
                    break;
                case ".c":
                    CompileC(compilerParameters, fileName, outputFiles);
                    break;
                default:
                    errors.Add(new CompilerException(fileName, 0, ErrorLevel.Error, 3, "Invalid file extension '" + ext + "'."));
                    break;
            }
        }

        private static void CompileCpp(CompilerParameters compilerParameters, string fileName, List<string> outputFiles)
        {
            var outputFileName = Path.Combine(compilerParameters.OutputPath, Path.GetFileNameWithoutExtension(fileName) + ".o");
            const string compilerExe = "arm-none-eabi-g++";
            var parameters = new List<string>{
                "-Os",
                "-g",
	            "-mcpu=cortex-m3",
	            "-mthumb",
	            "-march=armv7-m",
	            "-nostdlib",
	            "-ffunction-sections",
	            "-fdata-sections",
	            "-Wl,--gc-sections",
	            "-DBOARD_"+compilerParameters.Board,
	            "-DMCU_"+compilerParameters.Mcu,
	            "-D"+compilerParameters.Density,
	            "-D"+ compilerParameters.VectBaseAddr,
	            "-DERROR_LED_PORT="+compilerParameters.ErrorLedPort,
	            "-DERROR_LED_PIN="+compilerParameters.ErrorLedPin,
	            "-DMAPLE_IDE",
	            "-fno-rtti",
	            "-fno-exceptions",
	            "-Wall",
	            "-c",
                "-o" + outputFileName
            };

            foreach (var includeDir in compilerParameters.IncludeDirs)
            {
                parameters.Add("-I" + includeDir);
            }

            parameters.Add(fileName);

            Exec(Path.GetDirectoryName(outputFileName), fileName, compilerExe, parameters);

            outputFiles.Add(outputFileName);
        }

        private static void ConvertToBinary(string elfOutputFileName, string binOutputFileName)
        {
            Console.WriteLine("begin convert to bin... " + binOutputFileName);

            var parameters = new List<string>{
                "-v",
                "-Obinary",
                elfOutputFileName,
                binOutputFileName
            };
            Exec(Path.GetDirectoryName(binOutputFileName), elfOutputFileName, "arm-none-eabi-objcopy", parameters);

            Console.WriteLine("convert to bin complete");
        }

        private static void Link(CompilerParameters compilerParameters, string fileName, IEnumerable<string> outputFiles)
        {
            Console.WriteLine("begin linking... " + fileName);
            var outputFileName = Path.Combine(compilerParameters.OutputPath, Path.GetFileNameWithoutExtension(fileName) + ".elf");

            const string compilerExe = "arm-none-eabi-g++";
            var parameters = new List<string>{
	            "-mcpu=cortex-m3",
	            "-mthumb",
	            "-march=armv7-m",
	            "-Xlinker",
	            "--gc-sections",
	            "--print-gc-sections",
                "-Wall",
                "-T" + compilerParameters.LinkerScript,
                "-o" + outputFileName
            };

            foreach (var libraryPath in compilerParameters.LibraryPaths)
            {
                parameters.Add("-L" + libraryPath);
            }

            foreach (var outputFile in outputFiles)
            {
                parameters.Add(outputFile);
            }

            Exec(Path.GetDirectoryName(outputFileName), fileName, compilerExe, parameters);

            Console.WriteLine("link complete");
        }

        private static void CompileC(CompilerParameters compilerParameters, string fileName, List<string> outputFiles)
        {
            var outputFileName = Path.Combine(compilerParameters.OutputPath, Path.GetFileNameWithoutExtension(fileName) + ".o");
            const string compilerExe = "arm-none-eabi-gcc";
            var parameters = new List<string>{
                "-Os",
                "-g",
	            "-mcpu=cortex-m3",
	            "-mthumb",
	            "-march=armv7-m",
	            "-nostdlib",
	            "-ffunction-sections",
	            "-fdata-sections",
	            "-Wl,--gc-sections",
	            "-DBOARD_"+compilerParameters.Board,
	            "-DMCU_"+compilerParameters.Mcu,
	            "-D"+compilerParameters.Density,
	            "-D"+ compilerParameters.VectBaseAddr,
	            "-DERROR_LED_PORT="+compilerParameters.ErrorLedPort,
	            "-DERROR_LED_PIN="+compilerParameters.ErrorLedPin,
	            "-DMAPLE_IDE",
	            "-Wall",
	            "-c",
                "-o" + outputFileName
            };

            foreach (var includeDir in compilerParameters.IncludeDirs)
            {
                parameters.Add("-I" + includeDir);
            }

            parameters.Add(fileName);

            Exec(Path.GetDirectoryName(outputFileName), fileName, compilerExe, parameters);

            outputFiles.Add(outputFileName);
        }

        private static void Exec(string workingDirectory, string fileName, string compilerExe, IEnumerable<string> parameters)
        {
            //Console.WriteLine("running " + compilerExe + " " + parameters.JoinAsString(" "));
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = compilerExe,
                Arguments = parameters.JoinAsString(" "),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDirectory
            };
            using (Process process = Process.Start(start))
            {
                process.WaitForExit();
                ProcessCompilerStream(process.StandardOutput);
                ProcessCompilerStream(process.StandardError);
                if (process.ExitCode != 0)
                {
                    throw new CompilerException(fileName, 0, ErrorLevel.Error, 9999, "Failed to compile");
                }
            }
        }

        private static void ProcessCompilerStream(StreamReader stream)
        {
            using (StreamReader reader = stream)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine(ConvertGccToMsDev(line));
                }
            }
        }

        private static string ConvertGccToMsDev(string line)
        {
            // F:\\dev\\home\\fivevolt\\WiFiMp3\\maple\\include/gpio.c:108: undefined reference to `rcc_clk_enable'
            // or F:\dev\home\fivevolt\WiFiMp3\main.cpp:10:2: error: expected ';' before 'while'
            // or F:\dev\home\fivevolt\WiFiMp3\main.cpp: In function 'int main()':
            //            to
            // F:\dev\home\fivevolt\WiFiMp3\main.cpp(0): error C9999: Failed to compile
            try
            {
                var match = Regex.Match(line, "(.*?):([0-9]*?):([0-9]*?): (.*): (.*)");
                if (match.Success)
                {
                    return match.Groups[1].Value + "(" + match.Groups[2].Value + "): " + match.Groups[4].Value + " C0001: " + match.Groups[5].Value;
                }
                match = Regex.Match(line, "(.*?):([0-9]*?): (.*)");
                if (match.Success)
                {
                    return match.Groups[1].Value + "(" + match.Groups[2].Value + "): error C0002: " + match.Groups[3].Value;
                }
                match = Regex.Match(line, "(.*): (.*)");
                if (match.Success)
                {
                    return match.Groups[1].Value + "(0): error C0002: " + match.Groups[2].Value;
                }
            }
            catch (Exception)
            {
                // skip line
            }
            return line;
        }

        private static bool IsLinkerIncludeFile(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (extension == null)
            {
                return false;
            }
            var ext = extension.ToLowerInvariant();
            switch (ext)
            {
                case ".inc":
                case ".a":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsHeaderFile(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (extension == null)
            {
                return false;
            }
            var ext = extension.ToLowerInvariant();
            switch (ext)
            {
                case ".h":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsCompilableFile(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (extension == null)
            {
                return false;
            }
            var ext = extension.ToLowerInvariant();
            switch (ext)
            {
                case ".cpp":
                case ".S":
                case ".c":
                    return true;
                default:
                    return false;
            }
        }

        private static XDocument LoadProject(string projectFileName)
        {
            try
            {
                var result = XDocument.Load(projectFileName);
                return result;
            }
            catch (Exception ex)
            {
                throw new CompilerException(projectFileName, 0, ErrorLevel.Error, 1, "invalid project file (" + ex.Message + ").");
            }
        }
    }
}
