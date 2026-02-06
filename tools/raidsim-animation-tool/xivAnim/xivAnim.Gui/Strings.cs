namespace dev.susy_baka.xivAnim.EtoGui
{
    public static class Strings
    {
        // Application Info
        public const string AppTitle = "xivAnim";
        public const string AppVersion = "v.1.0.0";
        
        // Menu Items
        public const string MenuFile = "&File";
        public const string MenuEdit = "&Edit";
        public const string MenuOpen = "&Open...";
        public const string MenuSave = "&Save";
        public const string MenuSaveAs = "Save &As...";
        public const string MenuExit = "E&xit";
        public const string MenuClearJob = "&Clear Job";
        public const string MenuSettings = "&Settings...";

        // Dialog Titles
        public const string DialogTitleSettings = "Settings";
        public const string DialogTitleOpenJob = "Open job config";
        public const string DialogTitleSaveAsJob = "Save job config as...";
        public const string DialogTitleSelectWorkingDir = "Select working directory";
        public const string DialogTitleSelectExportDir = "Select export directory";
        public const string DialogTitleSelectGamePath = "Select FFXIV sqpack game directory";
        public const string DialogTitleSelectMultiAssistPath = "Select MultiAssist executable";
        public const string DialogTitleSelectBlenderPath = "Select Blender executable";

        // Buttons
        public const string ButtonRun = "Run";
        public const string ButtonCancel = "Cancel";
        public const string ButtonDeleteOutput = "Delete Output";
        public const string ButtonExit = "Exit";
        public const string ButtonSave = "Save";
        public const string ButtonBrowseFiles = "...";

        // Labels
        public const string LabelJobName = "Job Name:";
        public const string LabelWorkingDir = "Working Directory:";
        public const string LabelExportDir = "Export Directory:";
        public const string LabelSkeletonGamePath = "Skeleton Game Path:";
        public const string LabelModelPaths = "Model Paths:";
        public const string LabelPapGamePaths = "PAP Game Paths:";
        public const string LabelAppendFileNamesForPaths = "Append File Name Patterns:";
        public const string LabelLog = "Log Output:";
        public const string LabelGamePath = "FFXIV Game Path:";
        public const string LabelMultiAssistPath = "MultiAssist Path:";
        public const string LabelBlenderPath = "Blender Path:";

        // Tooltips
        public const string TooltipOpenFileBrowser = "Select through file browser";
        public const string TooltipModelPaths = "Local file path to one pre-extracted FBX model per line. Supports multiple demihuman body parts.";
        public const string TooltipPapGamePaths = "Internal game file path to one '.pap' animation file per line (e.g. chara/monster/.../idle_sp_1.pap). Same format as ResLogger2 path output.";
        public const string TooltipAppendFileNamesForPaths = "Optional: Regex patterns for matching .pap names or paths, that should get prefixed into the final animation FBX name to avoid overwriting individual animations.\nUsed if multiple .pap source files share the same internal animation names. Uses standard .NET regex format. \nImportant: If matching paths and not only names, always write any directory separators as forward slash \"/\" due to how the regex is matched. It will work on all platforms.";
        public const string TooltipLog = "Log output of the current session.";
        public const string TooltipJobName = "Short identifier for this model/job, used to name output files. Recommended to use the original name of the extracted model.";
        public const string TooltipWorkingDir = "Local folder for intermediate files like the extracted raw game files and FBX animations. Must be writable.";
        public const string TooltipExportDir = "Local folder where the final exported files like the FBX, Blend and XML file will be saved. Must be writable. Can be the same folder as Working Directory.";
        public const string TooltipSkeletonGamePath = "Internal game file path to the '.sklb' skeleton file (e.g. chara/monster/.../skl_*.sklb). Same format as ResLogger2 path output.";
        public const string TooltipRunButton = "Start processing the current job.";
        public const string TooltipCancelButton = "Cancel the current running job.";
        public const string TooltipDeleteOutputButton = "Delete all files created by this tool for a clean state.";
        public const string TooltipExit = "Close the application.";
        public const string TooltipSettings = "Open the settings dialog.";
        public const string TooltipMenuOpen = "Open a job file.";
        public const string TooltipMenuSave = "Save the current job to file.";
        public const string TooltipMenuSaveAs = "Save the current job to a new file.";
        public const string TooltipMenuClearJob = "Clear all fields in the current job.";
        public const string TooltipSettingGamePath = "Full local path to a \"sqpack\" folder of a FFXIV installation. Supports partial game installations. Used for extracting raw game files. \nImportant: Make sure this is the \"FINAL FANTASY XIV Online\\game\\sqpack\" folder that has dat and index files inside it in subfolders.";
        public const string TooltipSettingMultiAssistPath = "Full local path to a MultiAssist executable. Used for automatically extracting raw game .pap animation files into FBX files.";
        public const string TooltipSettingBlenderPath = "Optional: Full local path to a Blender executable. Used for automatically processing and exporting models to .blend and FBX files. \nImportant: Your Blender version must be 4.3 or above and you must have the \"Raidsim Tools\" add-on installed and enabled.";

        // MessageBoxes
        public const string MsgBoxUnsavedRun = "You have unsaved changes. Save before running?";
        public const string MsgBoxBlockDeleteRunning = "Cannot delete output while the pipeline is running.";
        public const string MsgBoxConfirmDeleteOutput = "Delete all generated output files for this job?\n('Animations/*', 'Exported Animations/*', 'Skeleton/*', export.fbx, export.blend, export.events.xml)";
        public const string MsgBoxRunningCancel = "The pipeline is currently running.\nCancel and exit?";
        public const string MsgBoxUnsavedExit = "You have unsaved changes. Save before exiting?";
        public const string MsgBoxUnsavedClose = "You have unsaved changes. Save before closing?";

        // Status
        public const string StatusPrefix = "Status: ";
        public const string StatusIdle = "Idle";
        public const string StatusRunning = "Running...";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusCancelRequested = "Cancel Requested";
        public const string StatusError = "Error - Check log";
    }
}
