namespace FiveVolt.Lib.Services
{
    public class Port
    {
        private readonly string _name;

        public Port(string name)
        {
            _name = name;
        }

        public string Name { get { return _name; } }
    }
}