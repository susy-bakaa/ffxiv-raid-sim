using System.Text.Json;

namespace dev.susy_baka.xivAnim.Core
{
    public static class JobService
    {
        public static ModelJob LoadJob(string path)
        {
            if (!File.Exists(path))
                return new ModelJob();

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ModelJob>(json) ?? new ModelJob();
        }

        public static void SaveJob(string path, ModelJob job)
        {
            var json = JsonSerializer.Serialize(job, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
