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

namespace LeagueToolLauncher
{
    /// <summary>
    /// Interaction logic for Disclaimer.xaml
    /// </summary>
    public partial class Disclaimer : Window
    {
        MainWindow parent;
        public Disclaimer(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            parent.AcceptDisclaimer();
            this.Close();
        }

        private void declineBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            parent.Close();
        }
    }
}
