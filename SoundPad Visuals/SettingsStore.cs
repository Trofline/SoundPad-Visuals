using System.IO;

namespace SoundPad_Visuals
{
    internal static class SettingsStore
    {
        private static readonly string AppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundOverlay");
        private static readonly string FolderFile = Path.Combine(AppDir, "sound_folder.txt");
        private static readonly string SoundsFile = Path.Combine(AppDir, "sounds.txt");

        public static string SelectedFolder { get; private set; }
        public static List<string> SoundFiles { get; set; } = new List<string>();

        public static void LoadSavedFolder()
        {
            if (File.Exists(FolderFile))
            {
                SelectedFolder = File.ReadAllText(FolderFile);
            }
            else
            {
                SelectedFolder = string.Empty;
            }
        }

        public static void SaveFolderPath(string folder)
        {
            if (!Directory.Exists(AppDir))
                Directory.CreateDirectory(AppDir);

            File.WriteAllText(FolderFile, folder ?? string.Empty);
            SelectedFolder = folder;
        }

        public static void LoadSoundList()
        {
            if (!File.Exists(SoundsFile))
            {
                SoundFiles = new List<string>();
                return;
            }

            SoundFiles = new List<string>(File.ReadAllLines(SoundsFile));
        }

        public static void SaveSoundList(List<string> sounds)
        {
            if (!Directory.Exists(AppDir))
                Directory.CreateDirectory(AppDir);

            File.WriteAllLines(SoundsFile, sounds ?? new List<string>());
            SoundFiles = new List<string>(sounds ?? new List<string>());
        }
    }
}
