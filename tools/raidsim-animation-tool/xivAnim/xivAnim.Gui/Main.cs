using Eto.Forms;
using dev.susy_baka.xivAnim.Core;

namespace dev.susy_baka.xivAnim.EtoGui
{
    public static class Main
    {
        public static MainForm CreateMainForm()
        {
            Log.MessageLogged += line => Console.WriteLine(line);

            var settings = SettingsService.Load();
            var job = JobService.LoadJob(AppPaths.DefaultJobPath);

            return new MainForm(settings, job, AppPaths.DefaultJobPath);
        }

        public static void StartEto(Application app)
        {
            app.Run(CreateMainForm());
        }
    }
}
