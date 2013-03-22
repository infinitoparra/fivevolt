using System.Reflection;

namespace DummyLoadController.Helpers
{
    public static class AssemblyHelpers
    {
        public static WX.Bitmap GetManifestResourceWxBitmap(this Assembly assembly, string name)
        {
            return new WX.Bitmap(name, assembly);
        }
    }
}
