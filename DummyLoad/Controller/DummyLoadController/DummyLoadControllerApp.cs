using WX;

namespace DummyLoadController
{
    public class DummyLoadControllerApp : App
    {
        public override bool OnInit()
        {
            MainFrame frame = new MainFrame();
            frame.Show(true);
            return base.OnInit();
        }
    }
}