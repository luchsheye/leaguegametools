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

namespace RecommendedItemTool
{
    /// <summary>
    /// Interaction logic for SetManager.xaml
    /// </summary>
    public partial class SetManager : UserControl
    {
        MainWindow parent;
        public SetManager(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void loadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!parent.loadItemString(itemStringTxt.Text))
            {
                itemStringTxt.Text = "Error: Invalid Item String";
                itemStringTxt.SelectAll();
            }
        }

        private void getCurrentBtn_Click(object sender, RoutedEventArgs e)
        {
            itemStringTxt.Text = parent.generateItemString();
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                itemStringTxt.Text = parent.generateItemString();
            }
        }
    }
}
