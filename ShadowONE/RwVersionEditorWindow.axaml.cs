using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ShadowONE.Models;
using ShadowONE.Services;

namespace ShadowONE
{
    public partial class RwVersionEditorWindow : Window
    {
        private readonly FileEntry _entry;
        private readonly Action<uint, uint, uint, uint, ushort> _onSave;

        private NumericUpDown? _versionBox;
        private NumericUpDown? _majorBox;
        private NumericUpDown? _minorBox;
        private NumericUpDown? _revisionBox;
        private NumericUpDown? _buildNumberBox;
        private TextBlock? _hexPreview;
        private TextBlock? _fileNameBlock;

        public RwVersionEditorWindow()
        {
            InitializeComponent();
            WindowsTitleBarHelper.SetDarkTitleBar(this);
            _entry = null!;
            _onSave = (_, _, _, _, _) => { };
        }

        public RwVersionEditorWindow(FileEntry entry, Action<uint, uint, uint, uint, ushort> onSave) : this()
        {
            _entry = entry;
            _onSave = onSave;
            
            Loaded += OnLoaded;
        }

        private void InitializeComponent()
        {
            Title = "Edit RW Version";
            Width = 350;
            Height = 350;
            MinWidth = 350;
            MinHeight = 350;
            CanResize = true;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var mainPanel = new StackPanel { Margin = new Avalonia.Thickness(15), Spacing = 12 };

            _fileNameBlock = new TextBlock
            {
                Text = "File: ",
                FontWeight = Avalonia.Media.FontWeight.Bold,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            mainPanel.Children.Add(_fileNameBlock);

            var fieldsPanel = new StackPanel { Spacing = 10 };

            var row1 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            row1.Children.Add(CreateFieldGroup("Version", ref _versionBox, 0, 15, 100));
            row1.Children.Add(CreateFieldGroup("Major", ref _majorBox, 0, 15, 100));
            row1.Children.Add(CreateFieldGroup("Minor", ref _minorBox, 0, 15, 100));
            fieldsPanel.Children.Add(row1);

            var row2 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            row2.Children.Add(CreateFieldGroup("Revision", ref _revisionBox, 0, 15, 100));
            row2.Children.Add(CreateFieldGroup("Build Number", ref _buildNumberBox, 0, 65535, 130));
            
            fieldsPanel.Children.Add(row2);

            mainPanel.Children.Add(fieldsPanel);

            var endPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Spacing = 12 };
            _hexPreview = new TextBlock
            {
                Text = "Version 0.0.0.0.0000",
                FontWeight = Avalonia.Media.FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            endPanel.Children.Add(_hexPreview);
            
            var saveButton = new Button 
            { 
                Content = "Save", 
                Width = 80, 
                Height = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            saveButton.Click += Save_Click;
            endPanel.Children.Add(saveButton);
            
            mainPanel.Children.Add(endPanel);

            var hintsPanel = new StackPanel { Spacing = 2, Margin = new Avalonia.Thickness(0, 2, 0, 0) };

            var hint1 = new TextBlock
            {
                Text = "Changing this does not change the RenderWare version tags inside the actual files; only the tags present inside the .ONE archive for each file or archive.",
                Foreground = Avalonia.Media.Brushes.Gray,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            hintsPanel.Children.Add(hint1);

            var hint2 = new TextBlock
            {
                Text = "The game ignores any files marked with a version higher than the game. Consider matching the game version or lower.",
                Foreground = Avalonia.Media.Brushes.Gray,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Padding = new Avalonia.Thickness(0,8,0,0)
            };
            hintsPanel.Children.Add(hint2);

            mainPanel.Children.Add(hintsPanel);

            Content = mainPanel;
        }

        private StackPanel CreateFieldGroup(string label, ref NumericUpDown? box, decimal min, decimal max, double width)
        {
            var panel = new StackPanel { Spacing = 3 };
            
            var labelBlock = new TextBlock
            {
                Text = label,
                FontSize = 12,
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            };
            panel.Children.Add(labelBlock);

            box = new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Increment = 1,
                FormatString = "0",
                Width = width,
                Height = 26,
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            panel.Children.Add(box);

            return panel;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (_entry != null)
            {
                _fileNameBlock!.Text = $"File: {_entry.FileName}";
                _versionBox!.Value = _entry.RwVersion;
                _majorBox!.Value = _entry.RwMajor;
                _minorBox!.Value = _entry.RwMinor;
                _revisionBox!.Value = _entry.RwRevision;
                _buildNumberBox!.Value = _entry.RwBuildNumber;
            }

            _versionBox!.ValueChanged += (_, _) => UpdatePreview();
            _majorBox!.ValueChanged += (_, _) => UpdatePreview();
            _minorBox!.ValueChanged += (_, _) => UpdatePreview();
            _revisionBox!.ValueChanged += (_, _) => UpdatePreview();
            _buildNumberBox!.ValueChanged += (_, _) => UpdatePreview();

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var version = (uint)(_versionBox?.Value ?? 0);
            var major = (uint)(_majorBox?.Value ?? 0);
            var minor = (uint)(_minorBox?.Value ?? 0);
            var revision = (uint)(_revisionBox?.Value ?? 0);
            var buildNumber = (ushort)(_buildNumberBox?.Value ?? 0);
            _hexPreview!.Text = $"Version: {version}.{major}.{minor}.{revision}.{buildNumber:X4}";
        }

        private void Save_Click(object? sender, RoutedEventArgs e)
        {
            var version = (uint)(_versionBox?.Value ?? 0);
            var major = (uint)(_majorBox?.Value ?? 0);
            var minor = (uint)(_minorBox?.Value ?? 0);
            var revision = (uint)(_revisionBox?.Value ?? 0);
            var buildNumber = (ushort)(_buildNumberBox?.Value ?? 0);

            _onSave(version, major, minor, revision, buildNumber);
            Close();
        }
    }
}
