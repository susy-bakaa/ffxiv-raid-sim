using Eto.Drawing;
using Eto.Forms;
using dev.susy_baka.xivAnim.Core;

namespace dev.susy_baka.xivAnim.Gui.Linux
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application app;

            try
            {
                app = new Application(new Eto.GtkSharp.Platform()); // Use GtkSharp for GTK backend on Linux
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
