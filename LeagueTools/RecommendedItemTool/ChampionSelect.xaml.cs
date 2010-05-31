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
using System.Media;

namespace RecommendedItemTool
{
    /// <summary>
    /// Interaction logic for ChampionSelect.xaml
    /// </summary>
    public partial class ChampionSelect : UserControl
    {
        public MainWindow parent;
        HeroData[] sortedChampions;

        double leftPadding; //this is automatically calculated to center it
        double topPadding = 50;
        double xPadding = 10;
        double yPadding = 13;
        double championSize = 70;
        int championsPerRow = 10;

        MediaPlayer soundPlayer = new MediaPlayer();

        public ChampionSelect(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
            sortedChampions = new HeroData[parent.heroData.Count];
            int i = 0;
            foreach (HeroData h in parent.heroData.Values.OrderBy(x => x.name))
            {
                sortedChampions[i] = h;
                i++;
            }
            leftPadding = (Width - championsPerRow * (championSize + xPadding)) / 2;

            //add the images to the form
            int championNum = 0;

            foreach (HeroData h in sortedChampions)
            {
                double imgX = leftPadding + (championNum % championsPerRow) * (championSize + xPadding);
                double imgY = topPadding + (championNum / championsPerRow) * (championSize + yPadding);
                Image cImg = new Image();
                Canvas.SetLeft(cImg, imgX);
                Canvas.SetTop(cImg, imgY);
                cImg.Width = championSize;
                cImg.Height = championSize;
                cImg.Source = h.icon;
                cImg.Tag = h.codeName;
                cImg.MouseDown += new MouseButtonEventHandler(cImg_MouseDown);
                cImg.MouseEnter += new MouseEventHandler(cImg_MouseEnter);
                cImg.MouseLeave += new MouseEventHandler(cImg_MouseLeave);

                Label cName = new Label();
                Canvas.SetLeft(cName, imgX - xPadding/2);
                Canvas.SetTop(cName, imgY + cImg.Height - 2);
                cName.Width = cImg.Width + xPadding;
                cName.Height = yPadding;
                cName.Padding = new Thickness(0);
                cName.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                cName.Content = h.name;
                cName.FontFamily = new FontFamily("Miramonte");
                cName.FontSize = 12;
                cName.Foreground = new SolidColorBrush(Colors.White);
                
                mainCanvas.Children.Add(cImg);
                mainCanvas.Children.Add(cName);
                championNum++;
            }
        }

        void cImg_MouseLeave(object sender, MouseEventArgs e)
        {
            Image img = (Image)sender;
            img.RenderTransform = TransformGroup.Identity;
        }

        void cImg_MouseEnter(object sender, MouseEventArgs e)
        {
            Image img = (Image)sender;
            TransformGroup tg = new TransformGroup();
            tg.Children.Add(new ScaleTransform(1.05, 1.05, championSize / 2, championSize / 2));
            tg.Children.Add(new RotateTransform(5,championSize / 2, championSize / 2));
            
            img.RenderTransform = tg;
        }

        void cImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string name = (string)(sender as Image).Tag;
            parent.selectChampion(name);
            parent.its.Visibility = Visibility.Visible;
            Canvas.SetZIndex(parent.backgroundImage, -10);
            Visibility = Visibility.Hidden;
            
            try
            {
                soundPlayer.Open(new Uri(Preferences.leagueFolder + "\\air\\assets\\sounds\\en_US\\champions\\" + parent.selectedChampion + ".mp3"));
                soundPlayer.Play();
            }
            catch{}
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            parent.its.Visibility = Visibility.Visible;
            Canvas.SetZIndex(parent.backgroundImage, -10);
            Visibility = Visibility.Hidden;
        }

    }
}
