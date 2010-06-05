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
            textBox1.Text = mw.autoAbilityTool.abilityString;
            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string temp = textBox1.Text;
            int level = 1;
            int[] abLevels = new int[4] { 0, 0, 0, 0 };
           
            for (int i = 0; i < temp.Length; i++)
            {
               
              
                if (!AutoAbilityTool.canLevelAbility(AutoAbilityTool.abLetterToNum(temp[i]), level, abLevels))
                {
                    textBox1.Text = "INVALID STRING";
                    return;
                }
                abLevels[AutoAbilityTool.abLetterToNum(temp[i])]++;
                level++;

            }

            mw.autoAbilityTool.heroCurrentLevel = 1;

            for (int i = 0; i < 3; i++)
            {
                mw.autoAbilityTool.heroAbilityLevels[i] = 0;
            }
            mw.autoAbilityTool.heroAbilityLevels[3] = 0;
            mw.autoAbilityTool.abilityString = textBox1.Text;
            for (int i = 0; i < mw.autoAbilityTool.abilityString.Length; i++)
            {
                char num = mw.autoAbilityTool.abilityString[i];
                mw.autoAbilityTool.heroAbilityLevels[AutoAbilityTool.abLetterToNum(num)]++;
            }
            mw.autoAbilityTool.writeAbilityLabel();
            mw.autoAbilityTool.initAbilityButtons();
            
        }
    }
}
