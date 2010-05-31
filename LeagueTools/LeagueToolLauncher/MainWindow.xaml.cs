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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LeagueOverlay;
using System.Net;
using System.IO;
using System.Diagnostics;
using Ionic.Zip;

namespace LeagueToolLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WebClient webClient = new WebClient();
        bool versionCheckDone = false;
        Dictionary<string, string> versionInformation = new Dictionary<string, string>();
        string versionString = "1.13";

        public MainWindow()
        {
            InitializeComponent();
            versionLbl.Content = "ver " + versionString;
            Canvas.SetLeft(updateBtn, Canvas.GetLeft(versionLbl) + ((string)versionLbl.Content).Length*7 + 5);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            webClient.OpenReadAsync(new Uri("http://code.google.com/p/leaguegametools/wiki/VersionPage"));
            webClient.OpenReadCompleted+=new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
            
        }

        void  webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if(e.Error == null)
            {
                try
                {
                    StreamReader sr = new StreamReader(e.Result);
                    string pageText = sr.ReadToEnd();
                    string[] ps = pageText.Split("[[[[[".ToCharArray());
                    foreach (string p in ps)
                    {
                        string[] keyAndValue = (p.Split("]]]]]".ToCharArray()))[0].Split('|');
                        if (keyAndValue.Length == 2)
                        {
                           versionInformation[keyAndValue[0]] = keyAndValue[1];
                        }
                    }
                    versionCheckDone = true;


                    if (versionInformation["Version"] != versionString)
                    {
                        updateBtn.Visibility = Visibility.Visible;
                    }
                }
                catch
                {
                    Console.WriteLine("page parse error");
                }
            }
            else
            {
                Console.WriteLine("page not found error");
            }
        }

        private void openRecItemToolBtn_Click(object sender, RoutedEventArgs e)
        {
            RecommendedItemTool.MainWindow mw = new RecommendedItemTool.MainWindow();
            mw.Visibility = System.Windows.Visibility.Visible;
        }

        private void startLeagueBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(RecommendedItemTool.Preferences.leagueFolder + "\\lol.launcher.exe");
        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            webClient.DownloadFile(versionInformation["UpdaterLink"],"Updater.zip");
            
            using (ZipFile zip = ZipFile.Read("Updater.zip"))
            {
                zip.ExtractAll("Updater", ExtractExistingFileAction.OverwriteSilently);
            }
            webClient.DownloadFile(versionInformation["DownloadLink"], "Updater\\newVersion.zip");
            File.Delete("Updater.zip");
            try
            {
                Process.Start("Updater\\UpdateTool.exe");
                this.Close();
            }
            catch
            {
                Console.WriteLine("something went wrong :'(");
            }
        }
    }
}
