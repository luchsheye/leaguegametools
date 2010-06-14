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
    /// Interaction logic for AutoAbilityTool.xaml
    /// </summary>
    public partial class AutoAbilityTool : UserControl
    {

        MainWindow parent;

        public string abilityString = "";
        Label[] championLevelLabels;
        public bool[] heroLevelableAbilities = new bool[4];
        public int[] heroAbilityLevels = new int[4] { 0, 0, 0, 0 };
        public int heroCurrentLevel = 1;

        public AutoAbilityTool(MainWindow parent)
        {
            InitializeComponent();

            this.parent = parent;
            //init champion level labels
            championLevelLabels = new Label[18];
            int championLevelLabelWidth = 19;
            int championLevelLabelHeight = 15;
            FontFamily championLevelLabelFF = new FontFamily("Miramonte");
            SolidColorBrush championLevelLabelColor = new SolidColorBrush(Colors.White);

            for (int i = 0; i < 18; i++)
            {
                Label l = new Label();
                l.Content = (i + 1).ToString();
                l.Width = championLevelLabelWidth;
                l.Height = championLevelLabelHeight;
                l.FontFamily = championLevelLabelFF;
                l.Foreground = championLevelLabelColor;
                l.FontSize = 13;
                l.Padding = new Thickness(0);
                l.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                l.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                Canvas.SetLeft(l, 3 + (championLevelLabelWidth) * i);
                Canvas.SetTop(l, 40 - championLevelLabelHeight);
                Canvas.SetZIndex(l, 0);
                mainCanvas.Children.Add(l);

                l = new Label();
                l.Content = "";
                l.Width = championLevelLabelWidth;
                l.Height = championLevelLabelHeight;
                l.FontFamily = championLevelLabelFF;
                l.Foreground = championLevelLabelColor;
                l.FontSize = 13;
                l.Padding = new Thickness(0);
                l.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                l.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;

                Canvas.SetLeft(l, 3 + (championLevelLabelWidth) * i);
                Canvas.SetTop(l, 45);
                Canvas.SetZIndex(l, 0);
                mainCanvas.Children.Add(l);
                championLevelLabels[i] = l;

            }
        }
        public void writeAbilityLabel()
        {
            for (int i = 0; i < 18 && i < abilityString.Length; i++) championLevelLabels[i].Content = abilityString[i];
            for (int i = abilityString.Length; i < 18; i++) championLevelLabels[i].Content = "";
        }

        public void getAutoAbilityInfo()
        {
            if (!File.Exists("abilities.txt")) return;
            StreamReader sr = new StreamReader("abilities.txt");
            string s;

            while (!sr.EndOfStream)
            {
                s = sr.ReadLine();

                if (s.ToLower() == parent.selectedChampion.ToLower())
                {

                    s = sr.ReadLine();

                    abilityString = s;
                    for (int i = 0; i < s.Length; i++)
                    {
                        char num = s[i];
                        heroAbilityLevels[abLetterToNum(num)]++;
                    }
                    sr.Close();
                    heroCurrentLevel = s.Length + 1;
                    writeAbilityLabel();
                    return;
                }

            }
            heroCurrentLevel = 1;
            abilityString = "";
            for (int i = 0; i < 4; i++) heroAbilityLevels[i] = 0;
            writeAbilityLabel();
            sr.Close();

        }
        public static string abNumToLetter(int num)
        {
            switch (num)
            {
                case 0:
                    return "q";
                case 1:
                    return "w";
                case 2:
                    return "e";
                case 3:
                    return "r";
                default:
                    return "";

            }

        }

        public static int abLetterToNum(char s)
        {
            switch (s)
            {
                case 'q':
                    return 0;
                case 'w':
                    return 1;
                case 'e':
                    return 2;
                case 'r':
                    return 3;
                default:
                    return -1;

            }

        }

        public bool canLevelAbility(int aNum, string name)
        {
            if (name.ToLower() == "udyr")
            {
                if ((heroAbilityLevels[aNum] * 2 + 1 <= heroCurrentLevel && heroAbilityLevels[aNum] < 5))
                {
                    if (aNum == 3 && heroAbilityLevels[aNum] < 3)
                        return true;
                    else if (aNum < 3) return true;
                    else return false;
                }
                else return false;
            }

            if (aNum < 3 && (heroAbilityLevels[aNum] * 2 + 1 <= heroCurrentLevel && heroAbilityLevels[aNum] < 5))
            {
                return true;
            }
            else if (aNum == 3)
            {
                if (heroCurrentLevel == 16)
                    return true;
                else if (heroCurrentLevel > 16 && heroAbilityLevels[3] < 3)
                    return true;
                else if (heroCurrentLevel == 11)
                    return true;
                else if (heroCurrentLevel > 11 && heroAbilityLevels[3] < 2)
                    return true;
                else if (heroCurrentLevel == 6)
                    return true;
                else if (heroCurrentLevel > 6 && heroAbilityLevels[3] < 1)
                    return true;
            }
            return false; ;
        }

        public static bool canLevelAbility(int aNum,int curLevel,int [] abLevels)
        {
            if (aNum < 3 && (abLevels[aNum] * 2 + 1 <= curLevel && abLevels[aNum] < 5))
            {
                return true;
            }
            else if (aNum == 3)
            {
                if (curLevel == 16)
                    return true;
                else if (curLevel > 16 && abLevels[3] < 3)
                    return true;
                else if (curLevel == 11)
                    return true;
                else if (curLevel > 11 && abLevels[3] < 2)
                    return true;
                else if (curLevel == 6)
                    return true;
                else if (curLevel > 6 && abLevels[3] < 1)
                    return true;
            }
            return false; ;
        }

        public void initAbilityButtons()
        {
            if (!canLevelAbility(0,parent.selectedChampion))
            {
                Q_btn.Opacity = .5;
                Q_btn.IsEnabled = false;
                label_Q.Content = Math.Abs(heroAbilityLevels[0] - 5);
            }
            else
            {
                Q_btn.Opacity = 1;
                Q_btn.IsEnabled = true;
                label_Q.Content = Math.Abs(heroAbilityLevels[0] - 5);
            }
            if (!canLevelAbility(1, parent.selectedChampion))
            {
                W_btn.Opacity = .5;
                W_btn.IsEnabled = false;
                label_W.Content = Math.Abs(heroAbilityLevels[1] - 5);
            }
            else
            {
                W_btn.Opacity = 1;
                W_btn.IsEnabled = true;
                label_W.Content = Math.Abs(heroAbilityLevels[1] - 5);

            }
            if (!canLevelAbility(2, parent.selectedChampion))
            {
                E_btn.Opacity = .5;
                E_btn.IsEnabled = false;
                label_E.Content = Math.Abs(heroAbilityLevels[2] - 5);
            }
            else
            {
                E_btn.Opacity = 1;
                E_btn.IsEnabled = true;
                label_E.Content = Math.Abs(heroAbilityLevels[2] - 5);
            }
            if (!canLevelAbility(3, parent.selectedChampion))
            {
                R_btn.Opacity = .5;
                R_btn.IsEnabled = false;
                label_R.Content = Math.Abs(3 - heroAbilityLevels[3]);
            }
            else
            {
                R_btn.Opacity = 1;
                R_btn.IsEnabled = true;
                label_R.Content = Math.Abs(3 - heroAbilityLevels[3]);
            }
        }
        private void saveAbilityBtn_Click(object sender, RoutedEventArgs e)
        {
            // Clipboard.SetText(abilityString);
            bool found = false;
            string outString = "";
            if (File.Exists("abilities.txt"))
            {
                StreamReader sr = new StreamReader("abilities.txt");

                string s;

                while (!sr.EndOfStream)
                {
                    s = sr.ReadLine();
                    if (s.ToLower() == parent.selectedChampion.ToLower())
                    {
                        found = true;
                        outString += s + Environment.NewLine;
                        outString += abilityString + Environment.NewLine;
                        sr.ReadLine();
                    }
                    else
                    {
                        outString += s + Environment.NewLine;
                    }

                }

                sr.Close();
            }
            if (!found)
            {
                outString += parent.selectedChampion + Environment.NewLine;
                outString += abilityString + Environment.NewLine;

            }
            StreamWriter sw = new StreamWriter("abilities.txt");
            sw.Write(outString);
            sw.Close();
        }
        public void Q_btn_Click(object sender, RoutedEventArgs e)
        {

            if (abilityString.Length == 18) return;
            if (canLevelAbility(0, parent.selectedChampion))
            {
                heroAbilityLevels[0]++;
                abilityString += abNumToLetter(0);
                championLevelLabels[heroCurrentLevel - 1].Content = abNumToLetter(0);
                heroCurrentLevel++;
            }
            initAbilityButtons();
        }

        public void W_btn_Click(object sender, RoutedEventArgs e)
        {
            if (abilityString.Length == 18) return;
            if (canLevelAbility(1, parent.selectedChampion))
            {
                heroAbilityLevels[1]++;
                abilityString += abNumToLetter(1);
                championLevelLabels[heroCurrentLevel - 1].Content = abNumToLetter(1);
                heroCurrentLevel++;
            }
            initAbilityButtons();
        }

        public void E_btn_Click(object sender, RoutedEventArgs e)
        {
            if (abilityString.Length == 18) return;
            if (canLevelAbility(2, parent.selectedChampion))
            {
                heroAbilityLevels[2]++;
                abilityString += abNumToLetter(2);
                championLevelLabels[heroCurrentLevel - 1].Content = abNumToLetter(2);
                heroCurrentLevel++;
            }
            initAbilityButtons();
        }

        public void R_btn_Click(object sender, RoutedEventArgs e)
        {

            if (abilityString.Length == 18) return;
            if (canLevelAbility(3, parent.selectedChampion))
            {
                heroAbilityLevels[3]++;
                abilityString += abNumToLetter(3);
                championLevelLabels[heroCurrentLevel - 1].Content = abNumToLetter(3);
                heroCurrentLevel++;
            }
            initAbilityButtons();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            heroCurrentLevel = 1;
            abilityString = "";
            for (int i = 0; i < 4; i++) heroAbilityLevels[i] = 0;
            for (int i = 0; i < 18; i++) championLevelLabels[i].Content = "";
            initAbilityButtons();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {

            if (heroCurrentLevel > 1) heroCurrentLevel--;
            if (abilityString.Length > 0)
            {
                Console.Write(abilityString[abilityString.Length - 1]);
                heroAbilityLevels[abLetterToNum(abilityString[abilityString.Length - 1])]--;
                abilityString = abilityString.Substring(0, abilityString.Length - 1);
            }
            writeAbilityLabel();
            initAbilityButtons();

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            parent.abilityManager.Visibility = Visibility.Visible;
        }
    }
}
