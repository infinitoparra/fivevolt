using System;

namespace VsMapleBuilder
{
    public class CompilerException : Exception
    {
        private readonly string _fileName;
        private readonly int _lineNumber;
        private readonly ErrorLevel _level;
        private readonly int _errorCode;
        private readonly string _message;

        public CompilerException(string fileName, int lineNumber, ErrorLevel level, int errorCode, string message)
        {
            _fileName = fileName;
            _lineNumber = lineNumber;
            _level = level;
            _errorCode = errorCode;
            _message = message;
        }

        public override string ToString()
        {
            string levelString = _level.ToString().ToLower();
            return _fileName + "(" + _lineNumber + "): " + levelString + " C" + _errorCode.ToString("0000") + ": " + _message;
        }
    }
}
