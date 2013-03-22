using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using WiFiMp3.Lib.FirmwareModels;
using WiFiMp3.Lib.Helpers;

namespace WiFiMp3.SettingsCreator
{
    public class Program
    {
        public static void Main(string[] argsArray)
        {
            List<string> args = new List<string>(argsArray);
            if (args.Count < 1)
            {
                Console.WriteLine("Invalid mode");
                return;
            }
            string mode = args[0];
            args.RemoveAt(0);
            if (mode == "iptoint")
            {
                IpToInt(args);
            }
            else if (mode == "blank")
            {
                CreateBlankSettingsXml(args);
            }
            else if (mode == "convert")
            {
                ConvertXmlToBinary(args);
            }
            else
            {
                Console.WriteLine("Invalid mode");
            }
        }

        private static void ConvertXmlToBinary(List<string> args)
        {
            if (args.Count != 2)
            {
                Console.WriteLine("infile and outfile required");
                return;
            }
            string inFile = args[0];
            string outFile = args[1];

            WifiMp3Settings settings = ReadSettingsFromFile(inFile);
            byte[] data = MarshalHelpers.ToBytes(settings);
            File.WriteAllBytes(outFile, data);
        }

        private static void CreateBlankSettingsXml(List<string> args)
        {
            if (args.Count != 1)
            {
                Console.WriteLine("outfile required");
                return;
            }
            string outFile = args[0];

            WifiMp3Settings settings = new WifiMp3Settings{
                wifi ={
                    gateway = IpAddressToInt("192.168.0.1"),
                    ipAddr = IpAddressToInt("192.168.0.225"),
                    securityKey = "secret",
                    ssid = "ssid",
                    subnetMask = IpAddressToInt("255.255.255.0"),
                    encryption = (byte) DigiWifiEncryption.Wpa2,
                    ipAddrMode = (byte) DigiWifiAddressMode.Static
                }
            };
            WriteSettingsToFile(outFile, settings);
        }

        private static void IpToInt(List<string> args)
        {
            if (args.Count != 1)
            {
                Console.WriteLine("ipaddr required");
                return;
            }
            string ipaddrStr = args[0];
            var i = IpAddressToInt(ipaddrStr);
            Console.WriteLine(i);
        }

        private static uint IpAddressToInt(string ipaddrStr)
        {
            var ipaddr = IPAddress.Parse(ipaddrStr);
            var bytes = ipaddr.GetAddressBytes();
            UInt32 i = (UInt32)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | (bytes[3] << 0));
            return i;
        }

        private static void WriteSettingsToFile(string outFile, object settings)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(WifiMp3Settings));
            using (TextWriter writer = new StreamWriter(outFile))
            {
                serializer.Serialize(writer, settings);
            }
        }

        private static WifiMp3Settings ReadSettingsFromFile(string inFile)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(WifiMp3Settings));
            using (TextReader textReader = new StreamReader(inFile))
            {
                return (WifiMp3Settings)deserializer.Deserialize(textReader);
            }
        }
    }
}
