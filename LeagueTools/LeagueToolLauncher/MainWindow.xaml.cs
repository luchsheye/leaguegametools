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
using System.Net;
using System.IO;
using System.Diagnostics;
using Ionic.Zip;
using System.ComponentModel;

using Preferences = RecommendedItemTool.Preferences;

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

        Process leagueOverlayProcess;

        bool downloadingUpdater = false;

        BackgroundWorker initialLoadThread;
        public bool disclaimerAccepted = false;
        public bool preferencesLoaded = false;
        public bool heroDataBackedup = false;

        public MainWindow()
        {
            InitializeComponent();
            versionLbl.Content = "ver " + versionString;
            Canvas.SetLeft(updateBtn, Canvas.GetLeft(versionLbl) + ((string)versionLbl.Content).Length*7 + 5);
            Label l = new Label();
            l.FontFamily = new FontFamily("Miramonte");
            l.FontSize = 16;
            l.Foreground = new SolidColorBrush(Colors.White);
            l.Content = "Configuration in process...";
            l.Width = this.Width;
            l.Height = this.Height;
            l.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            l.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.Content = l;


            initialLoadThread = new BackgroundWorker();
            initialLoadThread.DoWork += new DoWorkEventHandler(initialLoadThread_DoWork);
            initialLoadThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(initialLoadThread_RunWorkerCompleted);
            initialLoadThread.RunWorkerAsync();
        }

        void initialLoadThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (heroDataBackedup)
            {
                this.Content = mainCanvas;
            }
            else if (!preferencesLoaded)
            {
                LeagueDirectoryWindow ldw = new LeagueDirectoryWindow(this)
                {
                    Visibility = Visibility.Visible,
                    Top = this.Top + 110,
                    Left = this.Left
                };
            }
            else if (!disclaimerAccepted)
            {
                Disclaimer disclaimer = new Disclaimer(this)
                {
                    Visibility = Visibility.Visible,
                    Top = this.Top + 110,
                    Left = this.Left + 55
                };
                return;
            }
            else
            {
                //load failed...
                this.Close();
            }
        }

        void initialLoadThread_DoWork(object sender, DoWorkEventArgs e)
        {
            //load preferences
            if (!preferencesLoaded)
            {
                if (!Preferences.Load()) return;
                preferencesLoaded = true;
            }
            disclaimerAccepted = Preferences.disclaimerAccepted;

            //show disclaimer??
            if (!disclaimerAccepted)
            {
                return;
            }

            //check or make hero backup      
            if (!File.Exists(Preferences.leagueFolder + "\\game\\HeroPak_client_ToolBackup.zip"))
            {
                File.Copy(Preferences.leagueFolder + "\\game\\HeroPak_client.zip", Preferences.leagueFolder + "\\game\\HeroPak_client_ToolBackup.zip");
            }
            heroDataBackedup = true;
        }

        public void preferenceSetupComplete()
        {
            Preferences.Save();
            preferencesLoaded = true;
            initialLoadThread.RunWorkerAsync();
        }

        public void AcceptDisclaimer()
        {
            Preferences.disclaimerAccepted = disclaimerAccepted = true;
            Preferences.Save();
            initialLoadThread.RunWorkerAsync();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            webClient.OpenReadAsync(new Uri("http://code.google.com/p/leaguegametools/wiki/VersionPage"));
            webClient.OpenReadCompleted+=new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
            webClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler(webClient_DownloadDataCompleted);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);

            updateOverlayButtons();
        }

        void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            downloadProgressBar.Value = e.ProgressPercentage;
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
            downloadingUpdater = true;
            webClient.DownloadDataAsync(new Uri(versionInformation["UpdaterLink"]));
            updateBtn.Visibility = Visibility.Hidden;
            downloadProgressBar.Visibility = Visibility.Visible;
            downloadProgressLbl.Visibility = Visibility.Visible;
            downloadProgressLbl.Content = "File 1 of 2";
        }

        void webClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (downloadingUpdater)
            {
                File.WriteAllBytes("Updater.zip", e.Result);
                webClient.DownloadDataAsync(new Uri(versionInformation["DownloadLink"]));
                using (ZipFile zip = ZipFile.Read("Updater.zip"))
                {
                    zip.ExtractAll("Updater", ExtractExistingFileAction.OverwriteSilently);
                }
                File.Delete("Updater.zip");
                downloadingUpdater = false;
                downloadProgressLbl.Content = "File 2 of 2";
            }
            else
            {
                downloadProgressLbl.Content = "Complete";
                File.WriteAllBytes("Updater\\newVersion.zip", e.Result);
                try
                {
                    MessageBox.Show("The launcher will now close and the update will be applied", "Download Complete");
                    Process.Start("Updater\\UpdateTool.exe");
                    this.Close();
                }
                catch
                {
                    Console.WriteLine("something went wrong :'(");
                }
            }
        }

        public void updateOverlayButtons()
        {
            if (leagueOverlayProcess == null)
            {
                overlayStartBtn.Content = "Enable";
                overlayStatusLbl.Content = "Disabled";
                overlayStatusLbl.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                overlayStartBtn.Content = "Disable";
                overlayStatusLbl.Content = "Enabled";
                overlayStatusLbl.Foreground = new SolidColorBrush(Colors.LightGreen);
            }
        }
        private void overlayStartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (leagueOverlayProcess == null)
            {
                leagueOverlayProcess = Process.Start("LeagueOverlay.exe");
                leagueOverlayProcess.Exited += new EventHandler(leagueOverlayProcess_Exited);
            }
            else
            {
                leagueOverlayProcess.Kill();
                leagueOverlayProcess = null;
            }
            updateOverlayButtons();
        }

        void leagueOverlayProcess_Exited(object sender, EventArgs e)
        {
            leagueOverlayProcess = null;
            updateOverlayButtons();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (leagueOverlayProcess != null)
            {
                leagueOverlayProcess.Kill();
            }
        }
    }
}
