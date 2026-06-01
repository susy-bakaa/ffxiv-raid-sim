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
            app = new Application(new Eto.Wpf.Platform());
        }
        catch (Exception ex)
        {
            Log.Error("Failed to initialize Eto.Forms application:");
            Log.Error(ex.ToString());
            return;
        }

        var form = dev.susy_baka.xivAnim.EtoGui.Main.CreateMainForm();

        form.RequestAttention = f =>
        {
            WindowsAttention.FlashUntilFocused(f);
        };

        form.GotFocus += (_, _) =>
        {
            WindowsAttention.StopFlashing(form);
        };

        app.Run(form);
    }
    }
}
