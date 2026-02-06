using System.ComponentModel;
using dev.susy_baka.xivAnim.Core;
using Eto.Drawing;
using Eto.Forms;

namespace dev.susy_baka.xivAnim.EtoGui
{
    public class SettingsDialog : Dialog
    {
        private readonly AppSettings _settings;

        private bool _isDirty;

        private readonly TextBox _txtFfxivPath;
        private readonly TextBox _txtBlenderPath;
        private readonly TextBox _txtMultiAssistPath;

        public SettingsDialog(AppSettings settings)
        {
            _settings = settings;

            Title = Strings.DialogTitleSettings;
            ClientSize = new Size(600, 200);
            Resizable = false;

            _txtFfxivPath = new TextBox { Text = _settings.ffxivGamePath };
            _txtBlenderPath = new TextBox { Text = _settings.blenderPath };
            _txtMultiAssistPath = new TextBox { Text = _settings.multiAssistPath };

            // subscribe to change events
            _txtFfxivPath.TextChanged += MarkDirty;
            _txtBlenderPath.TextChanged += MarkDirty;
            _txtMultiAssistPath.TextChanged += MarkDirty;

            var okButton = new Button { Text = Strings.ButtonSave };
            okButton.Click += (_, _) => OnOk();

            var cancelButton = new Button { Text = Strings.ButtonCancel };
            cancelButton.Click += (_, _) => Close();

            Content = new TableLayout
            {
                Padding = new Padding(8),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow(
                        new Label 
                        { 
                            Text = Strings.LabelGamePath, 
                            VerticalAlignment = VerticalAlignment.Center, 
                            Width = 140,
                            ToolTip = Strings.TooltipSettingGamePath
                        },
                        new TableCell(CreatePathWithBrowse(_txtFfxivPath, OnBrowseFfxiv), true)
                    ),
                    new TableRow(
                        new Label
                        {
                            Text = Strings.LabelMultiAssistPath,
                            VerticalAlignment = VerticalAlignment.Center,
                            Width = 140,
                            ToolTip = Strings.TooltipSettingMultiAssistPath
                        },
                        new TableCell(CreatePathWithBrowse(_txtMultiAssistPath, OnBrowseMultiAssist), true)
                    ),
                    new TableRow(
                        new Label 
                        { 
                            Text = Strings.LabelBlenderPath, 
                            VerticalAlignment = VerticalAlignment.Center, 
                            Width = 140,
                            ToolTip = Strings.TooltipSettingBlenderPath
                        },
                        new TableCell(CreatePathWithBrowse(_txtBlenderPath, OnBrowseBlender), true)
                    ),
                    new TableRow(
                        null,
                        new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalContentAlignment = HorizontalAlignment.Right,
                            Spacing = 5,
                            Items = { okButton, cancelButton }
                        }
                    )
                }
            };
        }

        private void SaveConfig()
        {
            _settings.ffxivGamePath = _txtFfxivPath.Text ?? "";
            _settings.blenderPath = _txtBlenderPath.Text ?? "";
            _settings.multiAssistPath = _txtMultiAssistPath.Text ?? "";
            SettingsService.Save(_settings);
            _isDirty = false;
        }

        // --- Actions ---

        private void OnOk()
        {
            SaveConfig();
            Close();
        }

        private void OnBrowseFfxiv(object? sender, EventArgs e)
        {
            using var dlg = new SelectFolderDialog
            {
                Title = Strings.DialogTitleSelectGamePath,
            };

            if (dlg.ShowDialog(this) == DialogResult.Ok && !string.IsNullOrEmpty(dlg.Directory))
            {
                _txtFfxivPath.Text = dlg.Directory;
                _isDirty = true;
            }
        }

        private void OnBrowseMultiAssist(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = Strings.DialogTitleSelectMultiAssistPath,
                MultiSelect = false,
                Filters = { new FileFilter("Executable", ".exe", "") }
            };

            if (dlg.ShowDialog(this) == DialogResult.Ok && dlg.Filenames?.Count() > 0 && !string.IsNullOrEmpty(dlg.FileName))
            {
                _txtMultiAssistPath.Text = dlg.FileName;
                _isDirty = true;
            }
        }

        private void OnBrowseBlender(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = Strings.DialogTitleSelectBlenderPath,
                MultiSelect = false,
                Filters = { new FileFilter("Executable", ".exe", "") }
            };

            if (dlg.ShowDialog(this) == DialogResult.Ok && dlg.Filenames?.Count() > 0 && !string.IsNullOrEmpty(dlg.FileName))
            {
                _txtBlenderPath.Text = dlg.FileName;
                _isDirty = true;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Unsaved changes?
            if (_isDirty)
            {
                var res = MessageBox.Show(this,
                    Strings.MsgBoxUnsavedClose,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxType.Question);

                if (res == DialogResult.Yes)
                {
                    SaveConfig();
                }
                else if (res == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                // No = discard
                _isDirty = false;
            }

            base.OnClosing(e);
        }

        // --- Helpers ---

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
    }
}