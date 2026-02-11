using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;

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

                MessageBox.Show("Success. Now scan the files.");
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

            MessageBox.Show($"{SoundFiles.Count} Sounds scanned. Restart Program to effect the changes", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveSoundList(List<string> sounds)
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundOverlay");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, "sounds.txt");
            File.WriteAllLines(filePath, sounds);
        }

        private void RestartProgram_Click(object sender, RoutedEventArgs e)
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;

            Process.Start(exePath);

            Application.Current.Shutdown();
        }

        private void readme_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
@"Getting Started:
---------------
1. Open SoundPad → Settings → Hotkeys.
2. Give a check in the tab Numpad-Hotkeys -   [KEY] + Index   'Allow Index Hotkeys'.
3. For easier use, it's recommended to turn off the other options below.

Autostart:
----------
1. Press [WIN] + [R] and enter: shell:startup
2. Create a shortcut to this program and place it in that folder.

How to Use:
-----------
- Activate the Numpad.
- Go to Settings and select your folder.
- Scan the files.
- Restart the program.
- Use NumPad 1–9 to switch pages (hotkeys are saved automatically, just edit them as needed).
- To use custom hotkeys:
  • Leftclick the Index inside of the Overlay and change the text to your desired hotkey.
- NumPad 0 toggles the overlay visibility.
- Disabling the Numpad will disable all keybinds until it's enabled again.
- Right-clicking closes the program.
- The program remembers its last position and opens in the same location next time.

In Progress:
------------
- Hotkey passthrough when overlay is disabled.
- Invisible overlay when hovering over it with the mouse (so you don’t need to use the hotkey every time; the hotkey can still hide it).

Discord:
--------
trofline_black ← for bug reports, help, or ideas."
);

        }
    }
}