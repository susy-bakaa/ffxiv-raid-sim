using Eto.Drawing;
using Eto.Forms;
using dev.susy_baka.xivAnim.Core;

namespace dev.susy_baka.xivAnim.Gui.Windows
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application app;

            try
            {
                app = new Application(new Eto.Wpf.Platform()); // Use Eto.Wpf.Platform for WPF backend on Windows
            }
            catch (Exception ex)
            {
                Log.Error("Failed to initialize Eto.Forms application:");
                Log.Error(ex.ToString());
                return;
            }

            dev.susy_baka.xivAnim.EtoGui.Main.StartEto(app);
        }
    }
}
