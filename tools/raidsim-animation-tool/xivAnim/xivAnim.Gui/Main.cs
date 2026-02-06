using Eto.Forms;
using dev.susy_baka.xivAnim.Core;

namespace dev.susy_baka.xivAnim.EtoGui
{
    public static class Main
    {
        public static void StartEto(Application app)
        {
            Log.MessageLogged += line => Console.WriteLine(line);

            // Load initial settings & job
            var settings = SettingsService.Load();
            var job = JobService.LoadJob(AppPaths.DefaultJobPath);

            var mainForm = new MainForm(settings, job, AppPaths.DefaultJobPath);
            app.Run(mainForm);
        }
    }
}
