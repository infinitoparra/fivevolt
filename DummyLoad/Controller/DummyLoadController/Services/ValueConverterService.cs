using System;
using System.Text.RegularExpressions;

namespace DummyLoadController.Services
{
    public class ValueConverterService
    {
        public bool TryParseValueWithUnits(string value, out double result)
        {
            Match match = Regex.Match(value, @"([0-9\.]*)([a-zA-Z]*)");
            if (!match.Success)
            {
                result = 0.0;
                return false;
            }

            result = double.Parse(match.Groups[1].Value);
            string units = match.Groups[2].Value;
            if (units.StartsWith("m"))
            {
                result *= 0.001;
            }
            return true;
        }

        public string ToValueWithUnits(double val, string unit)
        {
            if (val < 1)
            {
                return (val * 1000.0).ToString("##0") + "m" + unit;
            }
            return val.ToString("0.000") + unit;
        }
    }
}
