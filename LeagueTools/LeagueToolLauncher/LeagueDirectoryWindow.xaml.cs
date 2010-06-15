using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using Preferences = RecommendedItemTool.Preferences;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
namespace LeagueToolLauncher
{
    /// <summary>
    /// Interaction logic for LeagueDirectoryWindow.xaml
    /// </summary>
    public partial class LeagueDirectoryWindow : Window
    {
        MainWindow parent;
        bool done = false;

        public LeagueDirectoryWindow(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("Software\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache");
            string clientString = "Riot Games\\League of Legends\\air\\LolClient.exe";
            if (rk != null)
            {
                foreach (string r in rk.GetValueNames())
                {
                    if (r.Contains(clientString))
                    {
                        Preferences.leagueFolder = r.Replace(clientString, "Riot Games\\League of Legends");
                        leagueDirectoryLabel.Text = Preferences.leagueFolder;
                        break;
                    }
                }
            }
            leagueDirectoryLabel.Text = Preferences.leagueFolder;
        }

        private void changeDirectoryBtn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            var result = fbd.ShowDialog();
            if (fbd.SelectedPath != "")
            {
                Preferences.leagueFolder = fbd.SelectedPath;
                leagueDirectoryLabel.Text = Preferences.leagueFolder;
            }
        }

        private void doneBtn_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Preferences.leagueFolder + "\\game\\HeroPak_Client.zip"))
            {

                parent.preferenceSetupComplete();
                done = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(this, "League of Legends not found at the given directory", "Error");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
           if(!done) parent.Close();
        }
    }
}
