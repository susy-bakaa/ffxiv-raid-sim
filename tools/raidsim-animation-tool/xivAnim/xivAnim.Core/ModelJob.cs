namespace dev.susy_baka.xivAnim.Core
{
    public class ModelJob
    {
        public string name { get; set; } = "";
        public string workingDirectory { get; set; } = "";
        public string exportDirectory { get; set; } = "";
        public List<string> modelPaths { get; set; } = new();
        public string skeletonGamePath { get; set; } = "";
        public List<string> papGamePaths { get; set; } = new();
        public List<string> appendFileNamesForPaths { get; set; } = new();
    }
}
