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
using System.Text.RegularExpressions;
using System.IO;

namespace RecommendedItemTool
{
    /// <summary>
    /// Interaction logic for ItemSelect.xaml
    /// </summary>
    public partial class ItemSelect : UserControl
    {
        public int
            attackFlag = 1,
            armorFlag = 1 << 1,
            healthFlag = 1 << 2,
            hrFlag = 1 << 3,
            consumableFlag = 1 << 4,
            apFlag = 1 << 5,
            lsFlag = 1 << 6,
            asFlag = 1 << 7,
            csFlag = 1 << 8,
            manaFlag = 1 << 9,
            movementFlag = 1 << 10,
            mregFlag = 1 << 11,
            cdFlag = 1 << 12,
            mresFlag = 1 << 13;

        MainWindow parent;
        Rectangle[] iPictureBoxes;
        CheckBox[] checkBoxes = new CheckBox[14];
        public ItemSelect(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            iPictureBoxes = new Rectangle[40];

            for (int i = 0; i < iPictureBoxes.Length; i++)
            {

                iPictureBoxes[i] = new Rectangle();
                iPictureBoxes[i].Visibility = Visibility.Hidden;
                iPictureBoxes[i].MouseDown += new MouseButtonEventHandler(ItemSelect_MouseDown);
                iPictureBoxes[i].MouseEnter += new MouseEventHandler(ItemSelect_MouseEnter);
                iPictureBoxes[i].MouseLeave += new MouseEventHandler(ItemSelect_MouseLeave);
                iPictureBoxes[i].MouseUp += new MouseButtonEventHandler(ItemSelect_MouseUp);
                iPictureBoxes[i].Style = parent.recItemImg1.Style;
                mainCanvas.Children.Add(iPictureBoxes[i]);
            }

            double cposy = 40;

            SolidColorBrush checkBoxBrush = new SolidColorBrush(Color.FromArgb(255, 108,181,248));
            FontFamily ff = new FontFamily("Miramonte");
            for (int i = 0; i < checkBoxes.Length; i++)
            {
                checkBoxes[i] = new CheckBox();
                checkBoxes[i].Height = 15;
                mainCanvas.Children.Add(checkBoxes[i]);
                Canvas.SetLeft(checkBoxes[i], 10);
                Canvas.SetTop(checkBoxes[i], cposy);
                cposy += checkBoxes[i].Height + 7;
                checkBoxes[i].Foreground = checkBoxBrush;
                checkBoxes[i].Click += new RoutedEventHandler(ItemSelect_Click);
                checkBoxes[i].FontFamily = ff;


            }


            checkBoxes[0].Content = "Attack";
            checkBoxes[0].Tag = this.attackFlag;
            checkBoxes[1].Content = "Armor";
            checkBoxes[1].Tag = this.armorFlag;
            checkBoxes[2].Content = "Health";
            checkBoxes[2].Tag = this.healthFlag;
            checkBoxes[3].Content = "Health Regen";
            checkBoxes[3].Tag = this.hrFlag;
            checkBoxes[4].Content = "Consumable";
            checkBoxes[4].Tag = this.consumableFlag;
            checkBoxes[5].Content = "Ability Power";
            checkBoxes[5].Tag = this.apFlag;
            checkBoxes[6].Content = "Life Steal";
            checkBoxes[6].Tag = this.lsFlag;
            checkBoxes[7].Content = "Attack Speed";
            checkBoxes[7].Tag = this.asFlag;
            checkBoxes[8].Content = "Critical Strike";
            checkBoxes[8].Tag = this.csFlag;
            checkBoxes[9].Content = "Mana";
            checkBoxes[9].Tag = this.manaFlag;
            checkBoxes[10].Content = "Movement";
            checkBoxes[10].Tag = this.movementFlag;
            checkBoxes[11].Content = "Mana Regen";
            checkBoxes[11].Tag = this.mregFlag;
            checkBoxes[12].Content = "Cooldown";
            checkBoxes[12].Tag = this.cdFlag;
            checkBoxes[13].Content = "Magic Resist";
            checkBoxes[13].Tag = this.mresFlag;

            Regex armorRx = new Regex(@"[0-9] armor( [^pen]|.||,)");
            Regex healthRegenRx = new Regex(@"[0-9] health (regen|per [0-9])");
            Regex magicResistRx = new Regex(@"[0-9] magic resist");
            Regex healthRx = new Regex(@"[0-9] (health[\s,][^r^p])|(health$)");
            Regex damageRx = new Regex(@"(\+[0-9]+ damage|attack damage)");
            Regex consumableRx = new Regex(@"click to consume");
            Regex abilityPowerRx = new Regex(@"ability power");
            Regex lifeStealRx = new Regex(@"lifesteal");
            Regex attackSpeedRx = new Regex(@"% attack speed");
            Regex criticalRx = new Regex(@"critical");
            Regex manaRx = new Regex(@"0 (mana[\s,][^r])|(mana$)");
            Regex moveRx = new Regex(@"[^s] move");
            Regex manaRegenRx = new Regex(@"([0-9] mana (regen|per [0-9])|restores [0-9] (mana))");
            Regex cooldownRx = new Regex(@"cooldowns");
            

            foreach (KeyValuePair<string, ItemData> id in parent.itemData)
            {
                string lowerDesc = id.Value.description.ToLower();
                if (armorRx.Match(lowerDesc).Success)
                    id.Value.type |= this.armorFlag;
                if (healthRegenRx.Match(lowerDesc).Success)
                    id.Value.type |= this.hrFlag;
                if (magicResistRx.Match(lowerDesc).Success)
                    id.Value.type |= this.mresFlag;
                if (healthRx.Match(lowerDesc).Success)
                    id.Value.type |= this.healthFlag;
                if (damageRx.Match(lowerDesc).Success)
                    id.Value.type |= this.attackFlag;
                if (consumableRx.Match(lowerDesc).Success)
                    id.Value.type |= this.consumableFlag;
                if (abilityPowerRx.Match(lowerDesc).Success)
                    id.Value.type |= this.apFlag;
                if (lifeStealRx.Match(lowerDesc).Success)
                    id.Value.type |= this.lsFlag;
                if (attackSpeedRx.Match(lowerDesc).Success)
                    id.Value.type |= this.asFlag;
                if (criticalRx.Match(lowerDesc).Success)
                    id.Value.type |= this.csFlag;
                if (manaRx.Match(lowerDesc).Success)
                    id.Value.type |= this.manaFlag;
                if (moveRx.Match(lowerDesc).Success)
                    id.Value.type |= this.movementFlag;
                if (manaRegenRx.Match(lowerDesc).Success)
                    id.Value.type |= this.mregFlag;
                if (cooldownRx.Match(lowerDesc).Success)
                    id.Value.type |= this.cdFlag;
            }
        }

        void ItemSelect_MouseLeave(object sender, MouseEventArgs e)
        {
            parent.itemToolTip.Visibility = Visibility.Hidden;
        }

        void ItemSelect_MouseEnter(object sender, MouseEventArgs e)
        {
            if (parent.isDraggingItem) return;
            Rectangle pic = sender as Rectangle;
            parent.itemToolTip.setItem(parent.itemData[(string)pic.Tag]);
            Canvas.SetLeft(parent.itemToolTip, Canvas.GetLeft(pic) + pic.Width + Canvas.GetLeft(this) + 3);
            Canvas.SetTop(parent.itemToolTip, Canvas.GetTop(pic) +  Canvas.GetTop(this));
            parent.itemToolTip.Visibility = Visibility.Visible;
        }

        void ItemSelect_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        void ItemSelect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle pic = sender as Rectangle;
            string item = (string)pic.Tag;
            parent.startDraggingItem(item);
            parent.itemToolTip.Visibility = Visibility.Hidden;
        }

        void ItemSelect_Click(object sender, RoutedEventArgs e)
        {
            updateDisplayedItems();
        }

        private void nameFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateDisplayedItems();
        }
        public void updateDisplayedItems()
        {
            tooManyItemsLabel.Visibility = Visibility.Hidden;
            foreach (Rectangle p in iPictureBoxes)
            {
                p.Visibility = Visibility.Hidden;
            }
            int flag = 0;
            for (int j = 0; j < checkBoxes.Length; j++)
            {
                if (checkBoxes[j].IsChecked == true)
                {
                    flag |= (int)checkBoxes[j].Tag;
                }
            }

            
            if (flag == 0 && nameFilterBox.Text.Length == 0)
            {
                foreach (Rectangle p in iPictureBoxes) p.Visibility = Visibility.Hidden;
                return;
            }
            

            double posx = 110;
            double posy = 30;

            int i = 0;
            
            foreach (KeyValuePair<string, ItemData> id in parent.itemData)
            {

                if ((flag & id.Value.type) == flag && id.Value.iconFile != null && id.Value.name.ToLower().Contains(nameFilterBox.Text))
                {

                    if (iPictureBoxes[0].Width + posx + 20 > this.Width)
                    {
                        posx = 110;
                        posy += 40 + 10;
                    }
                    if (i >= iPictureBoxes.Length)
                    {
                        tooManyItemsLabel.Visibility = Visibility.Visible;
                        break;
                    }
                    iPictureBoxes[i].Visibility = Visibility.Visible;
                    iPictureBoxes[i].Height = 40;
                    iPictureBoxes[i].Width = 40;
                    Canvas.SetLeft(iPictureBoxes[i], posx);
                    Canvas.SetTop(iPictureBoxes[i], posy);
                    if (File.Exists(Preferences.leagueFolder + "\\air\\assets\\images\\items\\" + id.Value.iconFile))
                    {
                        iPictureBoxes[i].Fill = new ImageBrush(new BitmapImage(new Uri(Preferences.leagueFolder + "\\air\\assets\\images\\items\\" + id.Value.iconFile)));
                    }
                    else
                    {
                        iPictureBoxes[i].Fill = new ImageBrush(new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\images\\image_not_found.png")));
                    }
                        
                    
                    iPictureBoxes[i].Tag = id.Value.id;
                    //iPictureBoxes[i].Click += new EventHandler(selectItem);
                    posx += iPictureBoxes[i].Width + 10;
                    i++;
                }

            }
        }
    }
}
