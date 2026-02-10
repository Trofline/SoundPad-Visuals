using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace SoundPad_Visuals
{
    public partial class MainWindow : Window
    {
        // Windows API
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_NONE = 0x0000;
        private const int WM_HOTKEY = 0x0312;

        private readonly string WindowSettingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SoundOverlay", "window_pos.txt");
        private string hotkeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundOverlay", "hotkeys.txt");

        private SettingsWindow Settings;
        private bool OverlayVisible = true;
        private int CurrentPage = 0;
        private int SoundsPerPage = 10;

        // zentrale Datenstruktur für Hotkeys aller Pages
        private Dictionary<int, string[]> PageHotkeys = new Dictionary<int, string[]>();

        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += MainWindow_KeyDown;

            LoadWindowPosition();
            ChainLoader();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            var hwnd = helper.Handle;
            var source = HwndSource.FromHwnd(hwnd);
            source.AddHook(HwndHook);

     
            
            // Hotkeys registrieren: NumPad0–9
            for (int i = 0; i <= 9; i++)
            {
                RegisterHotKey(hwnd, 100 + i, MOD_NONE, (uint)(0x60 + i)); // NumPad0 = 0x60
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                int page = -1;

                if (id >= 100 && id <= 109) page = id - 100; // NumPad0–9
                
                if (page >= 0)
                {
                    if (id == 100) // NumPad0
                    {
                        OverlayVisible = !OverlayVisible;
                        this.Visibility = OverlayVisible ? Visibility.Visible : Visibility.Hidden;
                    }
                    else
                    {
                        LoadPage(page);
                    }

                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            var hwnd = helper.Handle;

            for (int i = 0; i <= 9; i++)
            {
                UnregisterHotKey(hwnd, i);
                UnregisterHotKey(hwnd, 100 + i);
            }

            // Alle Hotkeys speichern
            SaveAllHotkeys();

            base.OnClosing(e);
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            int page = -1;

            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                page = e.Key - Key.NumPad0;
            }

            if (page >= 0)
            {
                LoadPage(page);
            }
        }

        private void ChainLoader()
        {
            if (Settings == null)
                Settings = new SettingsWindow();

            Settings.LoadSavedFolder();
            LoadSounds();
            LoadAllHotkeys();
            LoadPage(1);
        }

        private void LoadSounds()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundOverlay");
            string filePath = Path.Combine(dir, "sounds.txt");

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Keine Soundliste gefunden. Bitte zuerst im Settings-Fenster scannen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Settings.SoundFiles = File.ReadAllLines(filePath).ToList();
        }

        private void LoadPage(int num)
        {
            // vorherige Änderungen speichern
            SaveCurrentPageHotkeys();

            CurrentPage = num;

            // Hotkeys der aktuellen Page laden oder Standard setzen
            if (!PageHotkeys.ContainsKey(num))
            {
                string[] defaults = Enumerable.Range(1, 10).Select(i => $"F{i}").ToArray();
                PageHotkeys[num] = defaults;
            }

            string[] hk = PageHotkeys[num];

            h1.Text = hk[0];
            h2.Text = hk[1];
            h3.Text = hk[2];
            h4.Text = hk[3];
            h5.Text = hk[4];
            h6.Text = hk[5];
            h7.Text = hk[6];
            h8.Text = hk[7];
            h9.Text = hk[8];
            h10.Text = hk[9];

            ShowPage(num);
        }

        private void ShowPage(int page)
        {
            var sounds = Settings.SoundFiles
                .Skip((page - 1) * SoundsPerPage)
                .Take(SoundsPerPage)
                .ToList();

            // Sound-TextBoxes setzen
            TextBox[] soundBoxes = { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10 };
            for (int i = 0; i < sounds.Count; i++)
            {
                soundBoxes[i].Text = sounds[i];
            }
            for (int i = sounds.Count; i < 10; i++)
            {
                soundBoxes[i].Text = $"Sound {i + 1}";
            }
        }

        private void SaveCurrentPageHotkeys()
        {
            if (CurrentPage < 1) return;
            string[] hk = new string[10]
            {
                h1.Text, h2.Text, h3.Text, h4.Text, h5.Text,
                h6.Text, h7.Text, h8.Text, h9.Text, h10.Text
            };
            PageHotkeys[CurrentPage] = hk;
        }

        private void LoadAllHotkeys()
        {
            if (!File.Exists(hotkeyFile)) return;

            var lines = File.ReadAllLines(hotkeyFile);
            int currentPage = 0;
            string[] hk = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("[Page"))
                {
                    if (currentPage != 0 && hk != null)
                        PageHotkeys[currentPage] = hk;

                    currentPage = int.Parse(line.Replace("[Page", "").Replace("]", ""));
                    hk = new string[10];
                }
                else if (line.Contains("=") && hk != null)
                {
                    var parts = line.Split('=');
                    int index = int.Parse(parts[0].Split(' ')[1]) - 1; // Sound 1 = Index 0
                    hk[index] = parts[1];
                }
            }

            if (currentPage != 0 && hk != null)
                PageHotkeys[currentPage] = hk;
        }

        private void SaveAllHotkeys()
        {
            SaveCurrentPageHotkeys(); // sicherstellen, dass die aktuelle Page gespeichert wird

            StringBuilder sb = new StringBuilder();
            for (int page = 1; page <= 10; page++)
            {
                if (!PageHotkeys.ContainsKey(page))
                {
                    PageHotkeys[page] = Enumerable.Range(1, 10).Select(i => $"F{i}").ToArray();
                }

                sb.AppendLine($"[Page{page}]");
                for (int i = 0; i < 10; i++)
                {
                    sb.AppendLine($"Sound {i + 1}={PageHotkeys[page][i]}");
                }
            }

            File.WriteAllText(hotkeyFile, sb.ToString());
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SaveWindowPosition();
            Settings?.Close();
            
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowPosition();
        }

        private void SaveWindowPosition()
        {
            var dir = Path.GetDirectoryName(WindowSettingsFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(WindowSettingsFile, $"{Left};{Top}");
        }

        private void LoadWindowPosition()
        {
            if (!File.Exists(WindowSettingsFile)) return;

            var parts = File.ReadAllText(WindowSettingsFile).Split(';');
            if (parts.Length != 2) return;

            if (double.TryParse(parts[0], out double x))
                Left = x;
            if (double.TryParse(parts[1], out double y))
                Top = y;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (Settings == null)
            {
                Settings = new SettingsWindow();
                Settings.Closed += (s, args) => { Settings = null; };
                Settings.Owner = this;
                Settings.ShowDialog();
            }
            else
            {
                Settings.ShowDialog();
            }
        }

    }
}
