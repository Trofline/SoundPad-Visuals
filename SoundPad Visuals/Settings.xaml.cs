using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Windows;
using System.Collections.Generic;

namespace SoundPad_Visuals
{
    public partial class SettingsWindow : Window
    {
        public string SelectedFolder { get; set; }
        public List<string> SoundFiles { get; set; } = new List<string>();

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSavedFolder();
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                FolderName = SelectedFolder,
                ShowHiddenItems = true,
                ValidateNames = true
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.FolderName))
            {
                SelectedFolder = dialog.FolderName;
                FolderPathText.Text = SelectedFolder;
                SaveFolderPath(SelectedFolder);
            }
        }

        private void SaveFolderPath(string folder)
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundOverlay");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(Path.Combine(dir, "sound_folder.txt"), folder);
        }

        public void LoadSavedFolder()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundOverlay", "sound_folder.txt");
            if (File.Exists(file))
            {
                SelectedFolder = File.ReadAllText(file);
                FolderPathText.Text = SelectedFolder;
            }
        }

        private void ScanFiles_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedFolder) || !Directory.Exists(SelectedFolder))
            {
                MessageBox.Show("Please first select a folder", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var files = Directory.GetFiles(SelectedFolder, "*.*", SearchOption.TopDirectoryOnly)
                     .Where(f => f.EndsWith(".wav", System.StringComparison.OrdinalIgnoreCase)
                              || f.EndsWith(".mp3", System.StringComparison.OrdinalIgnoreCase)
                              || f.EndsWith(".m4a", System.StringComparison.OrdinalIgnoreCase)
                              || f.EndsWith(".mp4", System.StringComparison.OrdinalIgnoreCase))
                     .Select(f => Path.GetFileNameWithoutExtension(f))
                     .ToList();

            SoundFiles = files;
            SaveSoundList(SoundFiles);

            MessageBox.Show($"{SoundFiles.Count} Sounds scanned", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveSoundList(List<string> sounds)
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundOverlay");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, "sounds.txt");
            File.WriteAllLines(filePath, sounds);
        }
    }
}