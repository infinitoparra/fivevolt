using System;

namespace FiveVolt.Lib.Services
{
    public class StatusChangedArgs : EventArgs
    {
        private readonly Status _status;

        public StatusChangedArgs(Status status)
        {
            _status = status;
        }

        public Status Status { get { return _status; } }
    }
}