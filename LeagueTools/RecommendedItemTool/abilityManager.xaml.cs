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
    /// Interaction logic for abilityManager.xaml
    /// </summary>
    public partial class AbilityManager : UserControl
    {
        MainWindow mw;
        public AbilityManager(MainWindow parent)
        {
            mw = parent;
            InitializeComponent();
            cancel.Content = "X";
            label1.Content = "Copy/Load Ability String";
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            mw.abilityManager.Visibility = Visibility.Hidden;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            textBox1.Text = mw.abilityString;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            mw.heroCurrentLevel = 1;

            for (int i = 0; i < 4; i++)
            {
                mw.heroAbilityLevels[i]=0;
            }
            mw.abilityString = textBox1.Text;
            for (int i = 0; i < mw.abilityString.Length; i++)
            {
                char num = mw.abilityString[i];
                mw.heroAbilityLevels[mw.abLetterToNum(num)]++;
            }
            mw.writeAbilityLabel();
            mw.initAbilityButtons();
        }
    }
}
