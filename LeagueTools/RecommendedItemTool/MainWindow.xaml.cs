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
using Ionic.Zip;
using System.Xml;
using System.ComponentModel;
using System.Windows.Threading;

namespace RecommendedItemTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ChampionSelect cs;
        public ItemSelect its;
        HelpScreen hs;

        string abilityString = "";
        bool[] heroLevelableAbilities = new bool[4];
        int[] heroAbilityLevels = new int[4] { 0, 0, 0, 0 };
        int heroCurrentLevel = 1;

        public string[] recommendedItems = { "", "", "", "", "", "" };

        public Dictionary<string, ItemData> itemData = new Dictionary<string, ItemData>();

        public Dictionary<string, HeroData> heroData = new Dictionary<string, HeroData>();
        public string selectedChampion;

        Rectangle[] rPictureBoxes;

        public Rectangle draggingItemPic;
        string draggingItemID;

        public ItemToolTip itemToolTip;

        public SetManager setManager;
        public SavingOverlay savingOverlay;

        BackgroundWorker savingThread;
        DispatcherTimer draggingTimer;

        public bool isDraggingItem = false;
        public int draggingRecItem = -1;

        public Image backgroundImage;

        int currentSkinNum = 0;
        int skinCount = 0;

        public MainWindow()
        {
     
            
            InitializeComponent();
            rPictureBoxes = new Rectangle[] { recItemImg1, recItemImg2, recItemImg3, recItemImg4, recItemImg5, recItemImg6 };
            //register the events for the item boxes
            for (int i = 0; i < rPictureBoxes.Length; i++)
            {
                rPictureBoxes[i].Tag = i;
                rPictureBoxes[i].MouseEnter += new MouseEventHandler(rPictureBoxes_MouseEnter);
                rPictureBoxes[i].MouseLeave += new MouseEventHandler(rPictureBoxes_MouseLeave);
                rPictureBoxes[i].MouseDown += new MouseButtonEventHandler(rPictureBoxes_MouseDown);
            }

            
            
            //  button1.Image = Bitmap.FromFile("button.png");

            //compile the list of items
            //use regex...just for the hell of it
            Dictionary<string, string> championNames = new Dictionary<string, string>();
            Regex r = new Regex(@"""(\w+)_(\w+)"" = ""([^""]+)");
            foreach (string s in File.ReadAllLines(Preferences.leagueFolder + "\\game\\DATA\\Menu\\fontconfig_en_US.txt"))
            {
                Match m = r.Match(s);
                if (m.Success)
                {

                    string infoType = m.Groups[1].Value;
                    string infoID = m.Groups[2].Value;
                    string infoValue = m.Groups[3].Value;
                    if (infoType == "game_item_displayname")
                    {
                        itemData[infoID].name = infoValue;
                    }
                    else if (infoType == "game_character_lore" && infoValue.Length > 50
                        && File.Exists(Preferences.leagueFolder + "\\air\\assets\\images\\champions\\" + infoID + "_Square_0.png"))
                    {
                        heroData[infoID] = new HeroData(infoID, championNames[infoID]);
                    }
                    else if (infoType == "game_item_description")
                    {
                        itemData[infoID] = new ItemData(infoID);
                        itemData[infoID].description = infoValue;
                    }
                    else if (infoType == "game_character_displayname")
                        championNames[infoID] = infoValue;

                }

            }

            //extract the item inhibs...this is so bad...but for now
            string itemFolder = "data\\items";
            string alternateName = itemFolder.Replace('\\', '/');
            using (ZipFile zip = ZipFile.Read(Preferences.leagueFolder + "\\game\\HeroPak_client.zip"))
            {
                foreach (ZipEntry ze in zip)
                {
                    //Console.WriteLine(ze.FileName);
                    if ((ze.FileName.ToLower().Contains(itemFolder) || ze.FileName.ToLower().Contains(alternateName)) && ze.FileName.ToLower().Contains(".inibin"))
                    {
                        ze.Extract(ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }

            foreach (FileInfo d in (new DirectoryInfo(itemFolder)).GetFiles())
            {

                int zero = 0;
                if (d.FullName.Contains("inibin"))
                {
                    string itemID = new String(d.Name.ToCharArray(), 0, 4);
                    if (itemData.ContainsKey(itemID))
                    {
                        byte[] b = File.ReadAllBytes(d.FullName);
                        for (int i = 0; i < b.Length - 3; i++)
                        {
                            if (b[i] == 0)
                            {
                                zero = i;
                                continue;

                            }

                            if ((b[i] == ('d') && b[i + 1] == 'd' && b[i + 2] == 's') ||
                                (b[i] == 't' && b[i + 1] == 'g' && b[i + 2] == 'a') ||
                                (b[i] == 'D' && b[i + 1] == 'D' && b[i + 2] == 'S'))
                            {
                                itemData[itemID].iconFile = System.Text.ASCIIEncoding.ASCII.GetString(b, zero + 1, i - zero - 2) + ".png";

                                break;
                            }

                        }
                    }
                }

            }

            //cache the hero icons
            foreach (KeyValuePair<string, HeroData> kvp in heroData)
            {
                kvp.Value.icon = new BitmapImage(new Uri(Preferences.leagueFolder + "\\air\\assets\\images\\champions\\" + kvp.Key + "_Square_0.png"));
            }


            backgroundImage = new Image();
            backgroundImage.Width = Width;
            backgroundImage.Height = Height;
            Canvas.SetLeft(backgroundImage, 0);
            Canvas.SetTop(backgroundImage, -20);
            Canvas.SetZIndex(backgroundImage, -10);
            mainCanvas.Children.Add(backgroundImage);

            its = new ItemSelect(this);
            mainCanvas.Children.Add(its);
            Canvas.SetLeft(its, 20);
            Canvas.SetTop(its, 30);
            Canvas.SetZIndex(its, 0);
            its.Visibility = System.Windows.Visibility.Visible;

            //make Champion select and position, probably not pretty how i do it butttt its a start
            cs = new ChampionSelect(this);
            mainCanvas.Children.Add(cs);
            Canvas.SetLeft(cs, 0);
            Canvas.SetTop(cs, 0);
            Canvas.SetZIndex(cs, 20);
            cs.Visibility = System.Windows.Visibility.Hidden;



            //  hs.Show();
            hs = new HelpScreen(this);
            mainCanvas.Children.Add(hs);
            Canvas.SetZIndex(hs, 20);
            hs.Visibility = System.Windows.Visibility.Hidden;


            //init the item tooltip
            itemToolTip = new ItemToolTip();
            mainCanvas.Children.Add(itemToolTip);
            Canvas.SetZIndex(itemToolTip, 8);
            itemToolTip.Visibility = System.Windows.Visibility.Hidden;

            //init the dragging picturebox
            draggingItemPic = new Rectangle();
            draggingItemPic.Width = 50;
            draggingItemPic.Height = 50;
            draggingItemPic.RadiusX = 8;
            draggingItemPic.RadiusY = 8;
            draggingItemPic.Stroke = new SolidColorBrush(Colors.White);
            draggingItemPic.StrokeThickness = 2;
            draggingItemPic.Visibility = System.Windows.Visibility.Hidden;
            draggingItemPic.IsHitTestVisible = false;
            Canvas.SetZIndex(draggingItemPic, 9);
            mainCanvas.Children.Add(draggingItemPic);

            //set manager
            setManager = new SetManager(this);
            Canvas.SetLeft(setManager,
                Canvas.GetLeft(rPictureBoxes[0]) + 
                ((Canvas.GetLeft(rPictureBoxes[5]) + rPictureBoxes[5].Width - Canvas.GetLeft(rPictureBoxes[0])) - 
                setManager.Width) / 2);
            Canvas.SetTop(setManager, Canvas.GetTop(rPictureBoxes[0]) - setManager.Height - 2);
            //setManager.Left = rPictureBoxes[0].Left + ((rPictureBoxes[5].Right - rPictureBoxes[0].Left) - setManager.Width) / 2;
            //setManager.Top = rPictureBoxes[0].Top - setManager.Height;
            //setManager.Visible = false;
            setManager.Visibility = System.Windows.Visibility.Hidden;
            Canvas.SetZIndex(setManager, 0);
            mainCanvas.Children.Add(setManager);


            //saving overlay
            savingOverlay = new SavingOverlay();
            savingOverlay.Visibility = System.Windows.Visibility.Hidden;
            Canvas.SetZIndex(savingOverlay, 20);
            mainCanvas.Children.Add(savingOverlay);


            //background worker
            savingThread = new BackgroundWorker();
            savingThread.DoWork += new DoWorkEventHandler(savingThread_DoWork);

            //dragging timer
            draggingTimer = new DispatcherTimer();
            draggingTimer.Interval = TimeSpan.FromMilliseconds(30);
            draggingTimer.Tick += new EventHandler(draggingTimer_Tick);

            selectChampion(this.heroData.ElementAt(0).Value.codeName);
        }

        void rPictureBoxes_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int id = (int)((Rectangle)sender).Tag;
            draggingRecItem = id;
            startDraggingItem(recommendedItems[id]);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        void rPictureBoxes_MouseLeave(object sender, EventArgs e)
        {
            //hide the tooltip
            //toolTipLabel.Text = "";
            itemToolTip.Visibility = System.Windows.Visibility.Hidden;
            //(sender as Rectangle).Stroke = new SolidColorBrush(Colors.White);
        }


        void rPictureBoxes_MouseEnter(object sender, EventArgs e)
        {
            int boxNum = (int)(sender as Rectangle).Tag;
            string itemId = recommendedItems[boxNum];
            if (!itemData.ContainsKey(itemId)) return;

            itemToolTip.setItem(itemData[itemId]);
            Canvas.SetTop(itemToolTip, Canvas.GetTop((sender as Rectangle)) - itemToolTip.Height);
            Canvas.SetLeft(itemToolTip, Canvas.GetLeft((sender as Rectangle)) - itemToolTip.Width);

            itemToolTip.Visibility = System.Windows.Visibility.Visible;
            //(sender as Rectangle).Stroke = new SolidColorBrush(Colors.Orange);
        }

        public bool extractChampionData(string cName)
        {
            bool invalidFormat = false;
            string cFileName = "DATA\\Characters\\" + cName + "\\" + cName + ".inibin";
            using (ZipFile zip = ZipFile.Read(Preferences.leagueFolder + "\\game\\HeroPak_client.zip"))
            {
                ZipEntry ze = zip["DATA\\Characters\\" + cName + "\\" + cName + ".inibin"];

                if (ze == null)//sigh this hack is because of file format switching after a save
                {
                    string alternateName = cFileName.Replace('\\', '/');
                    foreach (ZipEntry z in zip)
                    {
                        if (z.FileName == cFileName || z.FileName == alternateName)
                        {
                            ze = z;
                            invalidFormat = true;
                            break;
                        }
                    }
                }
                if (ze != null)
                {
                    Console.WriteLine("Extracting data from zip");
                    ze.Extract("", ExtractExistingFileAction.OverwriteSilently);
                }
            }
            return invalidFormat;
        }
        public void selectChampion(string name)
        {
            
                
            string cName = name;
            selectedChampion = cName;
            getAutoAbilityInfo();
            initAbilityButtons();
            bool setManagerVisible = setManager.IsVisible;
            if (setManagerVisible) setManager.Visibility = Visibility.Hidden;

            //get the champion data from the zip (stored in "DATA\\Characters\\" + cName + "\\" + cName + ".inibin")
            extractChampionData(cName);

            string cFileName = "DATA\\Characters\\" + cName + "\\" + cName + ".inibin";
            bool inXml = false;
            if (File.Exists("itemBuilds.xml"))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("itemBuilds.xml");
                for (int i = 0; i < xmlDoc.DocumentElement.ChildNodes.Count; i++)
                {
                    if (xmlDoc.DocumentElement.ChildNodes[i].Name == name)
                    {
                        inXml = true;
                        string[] items = xmlDoc.DocumentElement.ChildNodes[i].InnerText.Split('-');
                        for (int j = 0; j < 6; j++)
                        {
                            recommendedItems[j] = items[j];

                        }

                    }
                }
            }

            if (File.Exists(cFileName) && inXml == false)
            {
                FileStream fs = new FileStream(cFileName, FileMode.Open);
                FileInfo fi = new FileInfo(cFileName);
                byte[] cBytes = new byte[fi.Length];
                int read = fs.Read(cBytes, 0, cBytes.Length);
                fs.Close();
                int pos = 0;
                while (pos < cBytes.Length - 25)
                {
                    if (cBytes[pos] == 0 && cBytes[pos + 5] == 0 && cBytes[pos + 10] == 0 && cBytes[pos + 15] == 0 && cBytes[pos + 20] == 0 && cBytes[pos + 25] == 0)
                    {
                        //Console.WriteLine("found at pos:" + pos + " out of " + cBytes.Length + " " + read + " from stream :: real size ");
                        //Console.WriteLine("real name: " + championNames[cName]);
                        bool validNames = true;
                        for (int i = 0; i < 6; i++)
                        {
                            recommendedItems[i] = System.Text.ASCIIEncoding.ASCII.GetString(cBytes, pos + 1 + 5 * i, 4);
                            if (!stringOnlyHasNumbers(recommendedItems[i]))
                            {
                                validNames = false;
                                break;
                            }
                            // Console.WriteLine(recommendedItems[i] + ":" + itemData[recommendedItems[i]].name);
                        }
                        if (validNames)
                        {
                            Console.WriteLine("Champion Loaded sucessfully");
                            break;
                        }
                    }
                    pos++;
                }

            }

            updateItemDisplay();
            skinCount = 1;

            while (File.Exists(Preferences.leagueFolder + "\\air\\assets\\images\\champions\\" + cName + "_Splash_" + skinCount + ".jpg"))
                skinCount++;
            Random r = new Random();
            currentSkinNum = r.Next(0, skinCount);
            backgroundImage.Source =
                new BitmapImage(
                    new Uri(Preferences.leagueFolder + "\\air\\assets\\images\\champions\\" + cName + "_Splash_" + currentSkinNum + ".jpg"));

            //mark the champion name
            championNameLbl.Content = heroData[cName].name;
            if (setManagerVisible) setManager.Visibility = System.Windows.Visibility.Visible;
        }
        public bool stringOnlyHasNumbers(string s)
        {
            for (int c = 0; c < s.Length; c++)
            {
                if (!Char.IsNumber(s, c)) return false;
            }
            return true;
        }
        public void updateItemDisplay()
        {
            for (int i = 0; i < 6; i++)
            {
                rPictureBoxes[i].Fill = new ImageBrush(
                    new BitmapImage(new Uri(Preferences.leagueFolder + "\\air\\assets\\images\\items\\" + itemData[recommendedItems[i]].iconFile)));

            }
        }


        private void saveBtn_Click(object sender, EventArgs e)
        {
            savingOverlay.Visibility = Visibility.Visible;
            savingThread.RunWorkerAsync();
        }
        public string generateItemString()
        {
            string items = "";
            items += recommendedItems[0];
            for (int i = 1; i < 6; i++)
            {
                items += "-" + recommendedItems[i];
            }
            return items;
        }
        public bool loadItemString(string items)
        {
            items = items.Trim();
            string[] potentialItems = items.Split('-');

            if (potentialItems.Length == 6)
            {

                bool validItems = true;
                for (int i = 0; i < 6; i++)
                {
                    if (!itemData.ContainsKey(potentialItems[i]))
                    {
                        validItems = false;
                        break;
                    }
                }
                if (validItems)
                {
                    recommendedItems = potentialItems;
                    updateItemDisplay();
                    return true;
                }
            }
            return false;
        }

        public void startDraggingItem(string ItemID)
        {
            draggingItemID = ItemID;
            
            draggingItemPic.Fill = new ImageBrush(new BitmapImage(
                new Uri(Preferences.leagueFolder + "\\air\\assets\\images\\items\\" + itemData[ItemID].iconFile)));
            Point mouseLoc = Mouse.GetPosition(mainCanvas);
            Canvas.SetLeft(draggingItemPic, mouseLoc.X);
            Canvas.SetTop(draggingItemPic, mouseLoc.Y);
            draggingItemPic.Visibility = Visibility.Visible;
            draggingTimer.Start();
            isDraggingItem = true;
        }
        public void stopDraggingItem()
        {
            draggingTimer.Stop();
            draggingItemPic.Visibility = Visibility.Hidden;
            //check for drop over a box
            Point p = Mouse.GetPosition(mainCanvas);
            for (int i = 0; i < rPictureBoxes.Length; i++)
            {
                Rect picturePos = new Rect(
                    Canvas.GetLeft(rPictureBoxes[i]),
                    Canvas.GetTop(rPictureBoxes[i]),
                    rPictureBoxes[i].Width,
                    rPictureBoxes[i].Height);
                if (picturePos.Contains(p))
                {
                    if (draggingRecItem >= 0) recommendedItems[draggingRecItem] = recommendedItems[i];
                    recommendedItems[i] = draggingItemID;
                    
                    itemToolTip.setItem(itemData[recommendedItems[i]]);
                    Canvas.SetTop(itemToolTip, Canvas.GetTop(rPictureBoxes[i]) - itemToolTip.Height);
                    Canvas.SetLeft(itemToolTip, Canvas.GetLeft(rPictureBoxes[i]) - itemToolTip.Width);

                    updateItemDisplay();
                    break;
                }
            }
            isDraggingItem = false;
            draggingRecItem = -1;
        }

        private void draggingTimer_Tick(object sender, EventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                stopDraggingItem();
            }
        }

        private void savingThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            savingOverlay.Visibility = Visibility.Hidden;
        }

        private void savingThread_DoWork(object sender, DoWorkEventArgs e)
        {

            saveBuild(selectedChampion, generateItemString(), "", true);
            //two step process - save to file then that file to the zip

            //three step process - Extract, save to extracted file then insert file to the zip

            string cFileName = "DATA\\Characters\\" + selectedChampion + "\\" + selectedChampion + ".inibin";
            bool invalidFormat = false;
            //extract
            invalidFormat = extractChampionData(selectedChampion);

            //fix the zip if the format is invalid
            if (invalidFormat)
            {
                using (ZipFile zip = ZipFile.Read(Preferences.leagueFolder + "\\game\\HeroPak_client.zip"))
                {
                    zip.RemoveEntry(zip[0]);//will do nothing
                    zip.Save();
                }
            }

            if (File.Exists(cFileName))
            {
                FileStream fs = new FileStream(cFileName, FileMode.Open);
                FileInfo fi = new FileInfo(cFileName);
                byte[] cBytes = new byte[fi.Length];
                int read = fs.Read(cBytes, 0, cBytes.Length);
                fs.Close();
                int pos = 0;
                while (pos < cBytes.Length - 25)
                {
                    if (cBytes[pos] == 0 && cBytes[pos + 5] == 0 && cBytes[pos + 10] == 0 && cBytes[pos + 15] == 0 && cBytes[pos + 20] == 0 && cBytes[pos + 25] == 0)
                    {
                        bool validNames = true;
                        for (int i = 0; i < 6; i++)
                        {
                            string item = System.Text.ASCIIEncoding.ASCII.GetString(cBytes, pos + 1 + 5 * i, 4);
                            if (!stringOnlyHasNumbers(item))
                            {
                                validNames = false;
                                break;
                            }
                        }
                        if (validNames)
                        {
                            int rItemNum = 0;
                            string rItem = recommendedItems[rItemNum];
                            for (int i = 0; i < recommendedItems.Length; i++)
                            {
                                for (int c = 0; c < 4; c++)
                                {
                                    cBytes[pos + i * 5 + 1 + c] = (byte)recommendedItems[i][c];
                                }
                            }
                            File.WriteAllBytes(cFileName, cBytes);
                            break;
                        }
                    }
                    pos++;
                }
            }
            else
            {
                //save failed...no message about this for now
                return;
            }

            using (ZipFile zip = ZipFile.Read(Preferences.leagueFolder + "\\game\\HeroPak_client.zip"))
            {
                ZipEntry toRemove = zip[cFileName];
                if (toRemove != null)
                {
                    zip.RemoveEntry(toRemove);
                    zip.AddFile(cFileName);
                    zip.Save();
                }
            }
        }



        private void saveBuild(string champName, string itemBuild, string buildName, bool isDefault)
        {

            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists("itemBuilds.xml"))
            {
                xmlDoc.Load("itemBuilds.xml");
            }
            else
            {
                XmlElement elem = xmlDoc.CreateElement("Item_Builds");
                xmlDoc.AppendChild(elem);
                xmlDoc.Save("itemBuilds.xml");
            }



            bool hasName = false;
            XmlNodeList xmlList = (xmlDoc.DocumentElement.ChildNodes);
            for (int i = 0; i < xmlList.Count; i++)
            {
                if (xmlList[i].Name == champName)
                {
                    hasName = true;
                    break;
                }
            }

            if (hasName)
            {

                //when we're ready for multiple builds, you can check if this one is already defined and create a new one if needed
                xmlDoc.DocumentElement[champName]["Build" + 1].InnerText = itemBuild;


            }
            else
            {
                XmlElement champNode = xmlDoc.CreateElement(champName);
                xmlDoc.DocumentElement.AppendChild(champNode);
                XmlElement elem2 = xmlDoc.CreateElement("Build" + 1);
                elem2.InnerText = itemBuild;
                XmlAttribute att = xmlDoc.CreateAttribute("Build_Name");
                att.Value = buildName;
                XmlAttribute defAtt = xmlDoc.CreateAttribute("Default");
                defAtt.Value = isDefault == true ? "true" : "false";
                elem2.Attributes.Append(att);
                elem2.Attributes.Append(defAtt);

                xmlDoc.DocumentElement[champName].AppendChild(elem2);
            }

            xmlDoc.Save("itemBuilds.xml");

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //clean up if some files are left behind
            try
            {
                if (Directory.Exists("DATA")) Directory.Delete("DATA", true);
            }
            catch
            {
                //not much can be done
            }
        }

        private void championSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            Canvas.SetZIndex(backgroundImage, 10);
            its.Visibility = Visibility.Hidden;
            cs.Visibility = Visibility.Visible;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingItem) return;
            Point p = e.GetPosition(mainCanvas);
            Canvas.SetLeft(draggingItemPic, p.X);
            Canvas.SetTop(draggingItemPic, p.Y);
        }

        private void mainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDraggingItem) stopDraggingItem();
        }

        private void helpBtn_Click(object sender, RoutedEventArgs e)
        {
            Canvas.SetZIndex(backgroundImage, 10);
            its.Visibility = Visibility.Hidden ;
            hs.Visibility = Visibility.Visible;
            
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            currentSkinNum = (currentSkinNum + 1) % skinCount;
            backgroundImage.Source =
                new BitmapImage(
                    new Uri(Preferences.leagueFolder + "\\air\\assets\\images\\champions\\" + selectedChampion + "_Splash_" + currentSkinNum + ".jpg"));
        }


        public bool[] levelableAbilities(int[] ab, int curLevel)
        {
            bool[] temp = new bool[4];

            //regular abilities
            for (int i = 0; i < 3; i++)
            {
                if (ab[i] * 2 + 1 <= curLevel)
                    temp[i] = true;
            }
            //ultimate
            if (curLevel == 16)
                temp[3] = true;
            else if (curLevel == 11)
                temp[3] = true;
            else if (curLevel > 11 && ab[3] < 2)
                temp[3] = true;
            else if (curLevel == 6)
                temp[3] = true;
            else if (curLevel > 6 && ab[3] < 1)
                temp[3] = true;

            return temp;
        }
        bool canLevelAbility(int aNum)
        {
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

        private void saveAbilityBtn_Click(object sender, RoutedEventArgs e)
        {
           // Clipboard.SetText(abilityString);
            StreamReader sr = new StreamReader("abilities.txt");
            string outString="";
            string s;
            bool found = false; ;
            while (!sr.EndOfStream)
            {
                s = sr.ReadLine();
                if (s.ToLower() == selectedChampion.ToLower())
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
            if (!found)
            {
                outString += selectedChampion + Environment.NewLine;
                outString += abilityString + Environment.NewLine;

            }
            StreamWriter sw = new StreamWriter("abilities.txt");
            sw.Write(outString);
            sw.Close();
        }

        void getAutoAbilityInfo()
        {
            if (!File.Exists("abilities.txt")) return;
            StreamReader sr = new StreamReader("abilities.txt");
            string s;
            
            while (!sr.EndOfStream)
            {
                s = sr.ReadLine();
               
                if (s.ToLower() == selectedChampion.ToLower())
                {
                    heroCurrentLevel = 18;
                    s=sr.ReadLine();
                    
                    abilityString = s;
                    for (int i = 0; i < s.Length; i++)
                    {
                        char num = s[i];
                        heroAbilityLevels[abLetterToNum(num)]++;
                    }
                    sr.Close();
                    writeAbilityLabel();
                    return;
                }

            }
            heroCurrentLevel =1;
            abilityString = "";
            for (int i = 0; i < 4; i++) heroAbilityLevels[i] = 0;
            writeAbilityLabel();
            sr.Close();
            
        }
        string abNumToLetter(int num)
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

        int abLetterToNum(char s)
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
        public void initAbilityButtons()
        {
            if (!canLevelAbility(0))
            {
                Q_btn.Opacity = .5;
                Q_btn.IsEnabled = false;
                label_Q.Content = Math.Abs(heroAbilityLevels[0] - 5);
            }
            else
            {
                Q_btn.Opacity =1;
                Q_btn.IsEnabled = true;
                label_Q.Content = Math.Abs(heroAbilityLevels[0] - 5);
            }
            if (!canLevelAbility(1))
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
            if (!canLevelAbility(2))
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
            if (!canLevelAbility(3))
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
        private void Q_btn_Click(object sender, RoutedEventArgs e)
        {

            if (abilityString.Length == 18) return;
            if (canLevelAbility(0))
            {
                heroAbilityLevels[0]++;
                abilityString += abNumToLetter(0);
                heroCurrentLevel++;
            }
            writeAbilityLabel();
            initAbilityButtons();
        }

        private void W_btn_Click(object sender, RoutedEventArgs e)
        {
            if (abilityString.Length == 18) return;
            if (canLevelAbility(1))
            {
                heroAbilityLevels[1]++;
                abilityString += abNumToLetter(1);
                heroCurrentLevel++;
            }
            writeAbilityLabel();
            initAbilityButtons();
        }

        private void E_btn_Click(object sender, RoutedEventArgs e)
        {
            if (abilityString.Length == 18) return;
            if (canLevelAbility(2))
            {
                heroAbilityLevels[2]++;
                abilityString += abNumToLetter(2);
                heroCurrentLevel++;
            }
            writeAbilityLabel();
            initAbilityButtons();
        }

        private void R_btn_Click(object sender, RoutedEventArgs e)
        {
            
            if (abilityString.Length == 18) return;
            if (canLevelAbility(3))
            {
                heroAbilityLevels[3]++;
                abilityString += abNumToLetter(3);
                heroCurrentLevel++;

            }
            writeAbilityLabel();
            initAbilityButtons();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            heroCurrentLevel = 1;
            abilityString = "";
            for (int i = 0; i < 4; i++) heroAbilityLevels[i] = 0;
            writeAbilityLabel();
            initAbilityButtons();
        }

        void writeAbilityLabel()
        {
            string s = "";
            for (int i = 0; i < abilityString.Length; i++)
            {
                s += abilityString[i];
                if (i < 9) s += "  ";
                else s += "    ";
            }
            abilityLabel.Content = s;
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

      

       

       

       

      
        
    }
}