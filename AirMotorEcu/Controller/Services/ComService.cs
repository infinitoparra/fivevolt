using FiveVolt.Lib.Services;

namespace AirMotorEcuController.Services
{
    public class ComService : ComServiceBase
    {
        public override string ResetStringPrefix
        {
            get { return "AirMotorEcu"; }
        }

        public void SendParameters(double airOn, double airOff, double exhaustOn, double exhaustOff)
        {
            Send("setairon " + FormatParameter(airOn) + "\n");
            Send("setairoff " + FormatParameter(airOff) + "\n");
            Send("setexhauston " + FormatParameter(exhaustOn) + "\n");
            Send("setexhaustoff " + FormatParameter(exhaustOff) + "\n");
        }

        private string FormatParameter(double d)
        {
            return (d * 10.0).ToString("###0");
        }
    }
}

