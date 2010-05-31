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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Ionic.Zip;

namespace UpdateTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker bgWorker;
        int status = 0;
        public MainWindow()
        {
            InitializeComponent();
            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
            bgWorker.RunWorkerAsync();
        }

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (status == 0)
            {
                statusLbl.Content = "Completed!";
                statusLbl.Foreground = new SolidColorBrush(Colors.LightGreen);
                restartBtn.Visibility = System.Windows.Visibility.Visible;
            }
            else if (status == 1)
            {
                statusLbl.Content = "Failed: NewVersion.zip not found";
                statusLbl.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
           //do the actual patching
            try
            {
                using (ZipFile zip = ZipFile.Read("newVersion.zip"))
                {
                    zip.ExtractAll("..\\", ExtractExistingFileAction.OverwriteSilently);
                }
                File.Delete("newVersion.zip");
            }
            catch
            {
                Console.WriteLine("file not found:");
                status = 1;
            }
        }

        private void restartBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("../LeagueToolLauncher.exe");
            }
            catch
            {
                Console.WriteLine("definitely not good");
            }
        }

       
    }
}
