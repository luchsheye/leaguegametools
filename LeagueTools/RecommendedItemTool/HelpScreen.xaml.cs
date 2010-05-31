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
using System.IO;

namespace RecommendedItemTool
{
    /// <summary>
    /// Interaction logic for HelpScreen.xaml
    /// </summary>
    public partial class HelpScreen : UserControl
    {
        MainWindow parent;
        public HelpScreen(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Canvas.SetZIndex(parent.backgroundImage, -10);
            parent.its.Visibility = Visibility.Visible;
            this.Visibility = Visibility.Hidden;
        }

        private void restoreBtn_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(Preferences.leagueFolder + "\\game\\HeroPak_client.zip");
            File.Move(Preferences.leagueFolder + "\\game\\HeroPak_client_ToolBackup.zip", Preferences.leagueFolder + "\\game\\HeroPak_client.zip");
            MessageBox.Show(parent, "The original HeroPak has been restored. If you are going to patch your league of legends client, do not reopen this program until the patch is complete. This program will now close.", "Restore");
            parent.Close();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
