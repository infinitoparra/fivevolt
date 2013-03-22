using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FixAnsi
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFileName = args[0];
            string outputFileName = args[1];

            byte[] inputData = File.ReadAllBytes(inputFileName);
            using (MemoryStream outputData = new MemoryStream())
            {
                for (int i = 0; i < inputData.Length; )
                {
                    if (i < inputData.Length - 2
                        && inputData[i] == 27
                        && inputData[i + 1] == '[')
                    {
                        i += 2; // escape and [

                        StringBuilder parameters = new StringBuilder();
                        while (i < inputData.Length
                            && (char.IsDigit((char)inputData[i]) || inputData[i] == ';'))
                        {
                            parameters.Append((char)inputData[i++]);
                        }
                        char code = (char)inputData[i++];

                        string[] parameterParts = parameters.ToString().Split(';');
                        foreach (string parameterPart in parameterParts)
                        {
                            byte[] parameterPartBytes = Encoding.ASCII.GetBytes(parameterPart);
                            outputData.WriteByte((byte)27);
                            outputData.WriteByte((byte)'[');
                            outputData.Write(parameterPartBytes, 0, parameterPartBytes.Length);
                            outputData.WriteByte((byte)code);
                        }
                    }
                    else
                    {
                        outputData.WriteByte(inputData[i++]);
                    }
                }

                File.WriteAllBytes(outputFileName, outputData.ToArray());
            }
        }
    }
}
