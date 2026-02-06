using System.ComponentModel;
using System.Text;
using Eto.Forms;
using Eto.Drawing;
using dev.susy_baka.xivAnim.Core;

namespace dev.susy_baka.xivAnim.EtoGui
{
    public class MainForm : Form
    {
        private readonly AppSettings _settings;
        private readonly ModelJob _job;
        private string _currentJobPath;

        private bool _isRunning;
        private bool _isDirty;
        private CancellationTokenSource? _cts;

        // UI controls
        private readonly TextBox _txtJobName;
        private readonly TextBox _txtWorkingDir;
        private readonly TextBox _txtExportDir;
        private readonly TextBox _txtSkeletonPath;

        private readonly TextArea _txtModelPaths;
        private readonly TextArea _txtPapPaths;
        private readonly TextArea _txtAppendPatterns;

        private readonly TextArea _txtLog;

        private Label? _lblStatus;

        private Button? _btnRun;
        private Button? _btnCancel;
        private Button? _btnDeleteOutput;
        private Button? _btnExit;

        public MainForm(AppSettings settings, ModelJob job, string jobPath)
        {
            Title = Strings.AppTitle;
            ClientSize = new Size(1000, 700);

            _settings = settings;
            _job = job;
            _currentJobPath = jobPath;

            // --- MENU BAR ---
            Menu = CreateMenuBar();

            // --- FIELDS ---
            _txtJobName = new TextBox { Text = _job.name };
            _txtWorkingDir = new TextBox { Text = _job.workingDirectory };
            _txtExportDir = new TextBox { Text = _job.exportDirectory };
            _txtSkeletonPath = new TextBox { Text = _job.skeletonGamePath };

            _txtModelPaths = new TextArea
            {
                Text = string.Join(Environment.NewLine, _job.modelPaths),
                AcceptsReturn = true,
                Wrap = false,
                ToolTip = Strings.TooltipModelPaths
            };
            _txtPapPaths = new TextArea
            {
                Text = string.Join(Environment.NewLine, _job.papGamePaths),
                AcceptsReturn = true,
                Wrap = false,
                ToolTip = Strings.TooltipPapGamePaths
            };
            _txtAppendPatterns = new TextArea
            {
                Text = string.Join(Environment.NewLine, _job.appendFileNamesForPaths),
                AcceptsReturn = true,
                Wrap = false,
                ToolTip = Strings.TooltipAppendFileNamesForPaths
            };

            _txtLog = new TextArea
            {
                ReadOnly = true,
                Wrap = false,
                ToolTip = Strings.TooltipLog
            };

            _lblStatus = new Label { Text = Strings.StatusPrefix + Strings.StatusIdle };

            // subscribe to change events
            _txtJobName.TextChanged += MarkDirty;
            _txtWorkingDir.TextChanged += MarkDirty;
            _txtExportDir.TextChanged += MarkDirty;
            _txtSkeletonPath.TextChanged += MarkDirty;

            _txtModelPaths.TextChanged += MarkDirty;
            _txtPapPaths.TextChanged += MarkDirty;
            _txtAppendPatterns.TextChanged += MarkDirty;

            // subscribe to log
            Log.MessageLogged += OnLogMessage;

            Content = CreateMainLayout();
        }

        private MenuBar CreateMenuBar()
        {
            var fileOpen = new ButtonMenuItem { Text = Strings.MenuOpen, ToolTip = Strings.TooltipMenuOpen };
            fileOpen.Click += (_, _) => OnOpenJob();

            var fileSave = new ButtonMenuItem { Text = Strings.MenuSave, ToolTip = Strings.TooltipMenuSave };
            fileSave.Click += (_, _) => SaveJob();

            var fileSaveAs = new ButtonMenuItem { Text = Strings.MenuSaveAs, ToolTip = Strings.TooltipMenuSaveAs };
            fileSaveAs.Click += (_, _) => OnSaveJobAs();

            var fileExit = new ButtonMenuItem { Text = Strings.MenuExit, ToolTip = Strings.TooltipExit };
            fileExit.Click += (_, _) => Close();

            var editClear = new ButtonMenuItem { Text = Strings.MenuClearJob, ToolTip = Strings.TooltipMenuClearJob };
            editClear.Click += (_, _) => ClearJob();

            var editSettings = new ButtonMenuItem { Text = Strings.MenuSettings, ToolTip = Strings.TooltipSettings };
            editSettings.Click += (_, _) => OnSettings();

            return new MenuBar
            {
                Items =
                {
                    new ButtonMenuItem { Text = Strings.MenuFile, Items = { fileOpen, fileSave, fileSaveAs, fileExit } },
                    new ButtonMenuItem { Text = Strings.MenuEdit, Items = { editClear, editSettings } }
                }
            };
        }

        private Control CreateMainLayout()
        {
            // --- TOP FIELDS ---

            var topFields = new TableLayout
            {
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow(
                        new Label 
                        { 
                            Text = Strings.LabelJobName, 
                            VerticalAlignment = VerticalAlignment.Center, 
                            Width = 120, 
                            ToolTip = Strings.TooltipJobName 
                        },
                        new TableCell(_txtJobName, true)
                    ),
                    new TableRow(
                        new Label 
                        { 
                            Text = Strings.LabelWorkingDir, 
                            VerticalAlignment = VerticalAlignment.Center, 
                            Width = 120,
                            ToolTip = Strings.TooltipWorkingDir
                        },
                        new TableCell(CreatePathWithBrowse(_txtWorkingDir, OnBrowseWorkingDir), true)
                    ),
                    new TableRow(
                        new Label 
                        { 
                            Text = Strings.LabelExportDir, 
                            VerticalAlignment = VerticalAlignment.Center, 
                            Width = 120,
                            ToolTip = Strings.TooltipExportDir
                        },
                        new TableCell(CreatePathWithBrowse(_txtExportDir, OnBrowseExportDir), true)
                    ),
                    new TableRow(
                        new Label 
                        { 
                            Text = Strings.LabelSkeletonGamePath, 
                            VerticalAlignment = VerticalAlignment.Center, 
                            Width = 120,
                            ToolTip = Strings.TooltipSkeletonGamePath
                        },
                        new TableCell(_txtSkeletonPath, true) // game-internal path, no file picker
                    ),
                }
            };

            // --- MIDDLE LISTS ---

            var listsLayout = new TableLayout
            {
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow(
                        new Label { Text = Strings.LabelModelPaths, ToolTip = Strings.TooltipModelPaths },
                        new Label { Text = Strings.LabelPapGamePaths, ToolTip = Strings.TooltipPapGamePaths },
                        new Label { Text = Strings.LabelAppendFileNamesForPaths, ToolTip = Strings.TooltipAppendFileNamesForPaths }
                    ),
                    new TableRow(
                        new TableCell(new Scrollable
                        {
                            Content = _txtModelPaths,
                            Size = new Size(-1, 120),
                            MinimumSize = new Size(0, 120)
                        }, true),
                        new TableCell(new Scrollable
                        {
                            Content = _txtPapPaths,
                            Size = new Size(-1, 120),
                            MinimumSize = new Size(0, 120)
                        }, true),
                        new TableCell(new Scrollable
                        {
                            Content = _txtAppendPatterns,
                            Size = new Size(-1, 120),
                            MinimumSize = new Size(0, 120)
                        }, true)
                    )
                }
            };

            // --- LOG + RUN BUTTON ---

            _btnRun = new Button { Text = Strings.ButtonRun, Width = 120, ToolTip = Strings.TooltipRunButton };
            _btnRun.Click += (_, _) => OnRun();

            _btnCancel = new Button { Text = Strings.ButtonCancel, Width = 120, ToolTip = Strings.TooltipCancelButton };
            _btnCancel.Click += (_, _) => OnCancel();

            _btnDeleteOutput = new Button { Text = Strings.ButtonDeleteOutput, Width = 120, ToolTip = Strings.TooltipDeleteOutputButton };
            _btnDeleteOutput.Click += (_, _) => OnDeleteOutput();

            _btnExit = new Button { Text = Strings.ButtonExit, Width = 120, ToolTip = Strings.TooltipExit };
            _btnExit.Click += (_, _) => Close();

            var buttonRow = new TableLayout
            {
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow(
                        new TableCell(
                            new StackLayout
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalContentAlignment = HorizontalAlignment.Right,
                                Spacing = 5,
                                Items =
                                {
                                    _btnRun,
                                    _btnCancel,
                                    _btnDeleteOutput,
                                    _btnExit,
                                    _lblStatus
                                }
                            },
                            true
                        )
                    )
                }
            };

            var logSection = new TableLayout
            {
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow(new Label { Text = Strings.LabelLog, ToolTip = Strings.TooltipLog }),
                    new TableRow(new TableCell(new Scrollable
                    {
                        Content = _txtLog,
                        MinimumSize = new Size(0, 140),
                        Size = new Size(-1, 140)
                    }, true)),
                    new TableRow(buttonRow)
                }
            };

            // --- COMBINE EVERYTHING ---

            SetStatus(Strings.StatusIdle);
            UpdateButtons();

            return new TableLayout
            {
                Padding = new Padding(8),
                Spacing = new Size(10, 10),
                Rows =
                {
                    new TableRow(topFields),
                    new TableRow(listsLayout),
                    new TableRow(logSection)
                }
            };
        }

        // ----------------- Job <-> UI sync -----------------

        private void SyncUiToJob()
        {
            _job.name = _txtJobName.Text ?? "";
            _job.workingDirectory = _txtWorkingDir.Text ?? "";
            _job.exportDirectory = _txtExportDir.Text ?? "";
            _job.skeletonGamePath = _txtSkeletonPath.Text ?? "";

            _job.modelPaths = SplitLines(_txtModelPaths.Text);
            _job.papGamePaths = SplitLines(_txtPapPaths.Text);
            _job.appendFileNamesForPaths = SplitLines(_txtAppendPatterns.Text);
        }

        private void SyncJobToUi()
        {
            _txtJobName.Text = _job.name;
            _txtWorkingDir.Text = _job.workingDirectory;
            _txtExportDir.Text = _job.exportDirectory;
            _txtSkeletonPath.Text = _job.skeletonGamePath;

            _txtModelPaths.Text = string.Join(Environment.NewLine, _job.modelPaths);
            _txtPapPaths.Text = string.Join(Environment.NewLine, _job.papGamePaths);
            _txtAppendPatterns.Text = string.Join(Environment.NewLine, _job.appendFileNamesForPaths);
        }

        private static List<string> SplitLines(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            return text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }

        // ----------------- Menu actions -----------------

        private void SaveJob()
        {
            SyncUiToJob();
            JobService.SaveJob(_currentJobPath, _job);
            SettingsService.Save(_settings);
            _isDirty = false;
            Log.Info($"Saved job to {_currentJobPath}");
        }

        private void OnSaveJobAs()
        {
            SyncUiToJob();

            using var dlg = new SaveFileDialog
            {
                Title = Strings.DialogTitleSaveAsJob,
                FileName = "job.json",
                Filters = { new FileFilter("JSON files", ".json") }
            };

            if (dlg.ShowDialog(this) == DialogResult.Ok && !string.IsNullOrEmpty(dlg.FileName))
            {
                _currentJobPath = dlg.FileName;
                JobService.SaveJob(_currentJobPath, _job);
                SettingsService.Save(_settings);
                _isDirty = false;
                Log.Info($"Saved job to {_currentJobPath}");
            }
        }

        private void OnOpenJob()
        {
            using var dlg = new OpenFileDialog
            {
                Title = Strings.DialogTitleOpenJob,
                MultiSelect = false,
                Filters = { new FileFilter("JSON files", ".json") }
            };

            if (dlg.ShowDialog(this) == DialogResult.Ok &&
                dlg.Filenames?.Count() > 0 &&
                !string.IsNullOrEmpty(dlg.FileName))
            {
                _currentJobPath = dlg.FileName;
                var job = JobService.LoadJob(_currentJobPath);

                _job.name = job.name;
                _job.workingDirectory = job.workingDirectory;
                _job.exportDirectory = job.exportDirectory;
                _job.modelPaths = job.modelPaths;
                _job.skeletonGamePath = job.skeletonGamePath;
                _job.papGamePaths = job.papGamePaths;
                _job.appendFileNamesForPaths = job.appendFileNamesForPaths;

                SyncJobToUi();
                _isDirty = false;
                Log.Info($"Loaded job from {_currentJobPath}");
            }
        }

        private void ClearJob()
        {
            _job.name = "";
            _job.workingDirectory = "";
            _job.exportDirectory = "";
            _job.modelPaths.Clear();
            _job.skeletonGamePath = "";
            _job.papGamePaths.Clear();
            _job.appendFileNamesForPaths.Clear();

            SyncJobToUi();
            _isDirty = false;
            Log.Info("Cleared current job.");
        }

        private void OnSettings()
        {
            using var dlg = new SettingsDialog(_settings);
            dlg.ShowModal(this);
            // Settings are applied inside dialog; we just persist when saving jobs or running.
        }

        private void OnRun()
        {
            if (_isRunning)
                return;

            if (_isDirty)
            {
                var result = MessageBox.Show(this,
                    Strings.MsgBoxUnsavedRun,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxType.Question);
                if (result == DialogResult.Yes)
                {
                    SaveJob();
                }
                else if (result == DialogResult.Cancel)
                {
                    return; // abort run
                }
                // No = continue without saving and revert UI to job state
                SyncJobToUi();
            }

            _isRunning = true;
            SetStatus(Strings.StatusRunning);
            UpdateButtons();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(() =>
            {
                try
                {
                    var pipeline = new PipelineService(_settings);
                    var jobPath = _currentJobPath;
                    pipeline.RunJob(jobPath, token);
                    Log.Info("Job finished.");
                    Application.Instance.AsyncInvoke(() => SetStatus(Strings.StatusCompleted));
                }
                catch (OperationCanceledException)
                {
                    Log.Info("Job canceled.");
                    Application.Instance.AsyncInvoke(() => SetStatus(Strings.StatusCancelled));
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    Application.Instance.AsyncInvoke(() => SetStatus(Strings.StatusError));
                }
                finally
                {
                    Application.Instance.AsyncInvoke(() =>
                    {
                        _isRunning = false;
                        _cts = null;
                        SetStatus(Strings.StatusIdle);
                        UpdateButtons();
                    });
                }
            });
        }

        private void OnCancel()
        {
            if (!_isRunning || _cts == null)
                return;

            SetStatus(Strings.StatusCancelRequested);
            _cts.Cancel();
        }

        private void OnDeleteOutput()
        {
            if (_isRunning)
            {
                MessageBox.Show(this,
                    Strings.MsgBoxBlockDeleteRunning,
                    MessageBoxType.Warning);
                return;
            }

            var result = MessageBox.Show(this,
                Strings.MsgBoxConfirmDeleteOutput,
                MessageBoxButtons.YesNo,
                MessageBoxType.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                var pipeline = new PipelineService(_settings);
                pipeline.ClearJob(_currentJobPath);
                Log.Info("Output cleared.");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                MessageBox.Show(this, ex.Message, MessageBoxType.Error);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // If pipeline is running, ask first
            if (_isRunning)
            {
                var res = MessageBox.Show(this,
                    Strings.MsgBoxRunningCancel,
                    MessageBoxButtons.YesNo,
                    MessageBoxType.Warning);

                if (res == DialogResult.Yes)
                {
                    _cts?.Cancel();
                    // don't block here; we kill external processes via token
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Unsaved changes?
            if (_isDirty)
            {
                var res = MessageBox.Show(this,
                    Strings.MsgBoxUnsavedExit,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxType.Question);

                if (res == DialogResult.Yes)
                {
                    SaveJob();
                }
                else if (res == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                // No = discard
            }

            base.OnClosing(e);
        }

        private void OnBrowseWorkingDir(object? sender, EventArgs e)
        {
            using var dlg = new SelectFolderDialog
            {
                Title = Strings.DialogTitleSelectWorkingDir
            };

            if (dlg.ShowDialog(this) == DialogResult.Ok && !string.IsNullOrEmpty(dlg.Directory))
            {
                _txtWorkingDir.Text = dlg.Directory;
            }
        }

        private void OnBrowseExportDir(object? sender, EventArgs e)
        {
            using var dlg = new SelectFolderDialog
            {
                Title = Strings.DialogTitleSelectExportDir
            };

            if (dlg.ShowDialog(this) == DialogResult.Ok && !string.IsNullOrEmpty(dlg.Directory))
            {
                _txtExportDir.Text = dlg.Directory;
            }
        }

        // ----------------- Log handling -----------------

        private readonly StringBuilder _logBuilder = new();

        private void OnLogMessage(string line)
        {
            Application.Instance.AsyncInvoke(() =>
            {
                _logBuilder.AppendLine(line);
                _txtLog.Text = _logBuilder.ToString();

                // Move caret to end and scroll there
                var len = _txtLog.Text?.Length ?? 0;
                _txtLog.CaretIndex = len;
                _txtLog.Selection = new Range<int>(len, len);
                _txtLog.ScrollToEnd();
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            Log.MessageLogged -= OnLogMessage;
            base.OnClosed(e);
        }

        // ----------------- Helpers -----------------

        private Control CreatePathWithBrowse(TextBox textBox, EventHandler<EventArgs> onBrowseClicked)
        {
            var browseButton = new Button { Text = Strings.ButtonBrowseFiles, ToolTip = Strings.TooltipOpenFileBrowser };
            browseButton.Click += onBrowseClicked;

            return new TableLayout
            {
                Spacing = new Size(5, 0),
                Rows =
                {
                    new TableRow(
                        new TableCell(textBox, true),
                        new TableCell(browseButton, false)
                    )
                }
            };
        }

        private void MarkDirty(object? sender, EventArgs e)
        {
            _isDirty = true;
        }

        private void UpdateButtons()
        {
            if (_btnRun != null)
                _btnRun.Enabled = !_isRunning;
            if (_btnCancel != null)
                _btnCancel.Enabled = _isRunning;
            if (_btnDeleteOutput != null)
                _btnDeleteOutput.Enabled = !_isRunning;
            if (_btnExit != null)
                _btnExit.Enabled = true;
        }

        private void SetStatus(string text)
        {
            if (_lblStatus != null)
                _lblStatus.Text = Strings.StatusPrefix + text;
        }
    }
}
