using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using Bitmap = System.Drawing.Bitmap;
using Rect = System.Drawing.Rectangle;
using SColor = System.Drawing.Color;
using LuaInterface;

namespace LeagueOverlay
{
    public class LeagueInfo
    {
        public static string heroName = "";
        public static int currentLevel = 0;
        static int healthPixCount = -1;
        public static double healthPercent=100;
        
        public static bool[] canChooseAbility = new bool[4] { false, false, false, false };
        public MainWindow form;
        public static Dictionary<string, string> cnames = new Dictionary<string, string>();

        DateTime lastUpdate = DateTime.Now;
        int[] remainingCooldown;
        double[] lastCooldownPerc;
        Rect[] abilityRectangles;
        DateTime[] cooldownStartTime;

        public bool outOfLoadScreen = false;
        LoadScreenInfo loadScreenInfo = null;

        SummonerInfo[][] summonerInfo = new SummonerInfo[2][];
        public LeagueInfo(MainWindow f)
        {
            form = f;
            Regex r = new Regex(@"""(\w+)_(\w+)"" = ""([^""]+)");
            foreach (string s in File.ReadAllLines("C:\\Riot Games\\League of Legends\\game\\DATA\\Menu\\fontconfig_en_US.txt"))
            {
                
                Match m = r.Match(s);
               
                if (m.Success)
                {

                    string infoType = m.Groups[1].Value;
                    string infoID = m.Groups[2].Value;
                    string infoValue = m.Groups[3].Value;
                    if (infoType == "game_character_displayname"
                        && File.Exists("C:\\Riot Games\\League of Legends\\air\\assets\\images\\champions\\" + infoID + "_Square_0.png"))
                    {
                        cnames.Add(infoID, infoValue);
                    }

                }
            }
            abilityRectangles = new Rect[]{
                LeagueUI.cAbility1R,LeagueUI.cAbility2R,LeagueUI.cAbility3R,LeagueUI.cAbility4R,LeagueUI.sAbility1R,LeagueUI.sAbility2R};
            remainingCooldown = new int[abilityRectangles.Length];
            lastCooldownPerc = new double[abilityRectangles.Length];
            cooldownStartTime = new DateTime[abilityRectangles.Length];
        }

        //do update league info
        public void update()
        {
            if (form.windowImage.Width < 10 || form.windowImage.Height < 10) return;
            /*look for the load screen*/
            if (!outOfLoadScreen)
            {
                var w =  form.windowImage;
                bool allBlack = true;
                for (int x = w.Width / 2; x < w.Width; x++)
                {
                    for (int y = w.Height-5; y < w.Height; y++)
                    {
                        var c = w.GetPixel(x, y);
                        if (c.R != 0 || c.G != 0 || c.B != 0)
                        {
                            allBlack = false;
                            break;
                        }
                    }
                }
                if (allBlack) //process the load screen
                {
                    if (loadScreenInfo == null)
                    {
                        parseLoadingScreen();
                        return;
                    }

                    for (int i = 0; i < loadScreenInfo.topChampionCount; i++)
                    {
                        if (summonerInfo[0][i] == null)
                        summonerInfo[0][i] = getSummonerInfo(
                            (Bitmap)w, 
                            loadScreenInfo.minX + i * (loadScreenInfo.championWidth + loadScreenInfo.xpadding), 
                            loadScreenInfo.topPadding, 
                            loadScreenInfo.scale);
                    }

                    for (int i = 0; i < loadScreenInfo.botChampionCount; i++)
                    {
                        if (summonerInfo[1][i] == null)
                        summonerInfo[1][i] = getSummonerInfo(
                            (Bitmap)w, 
                            loadScreenInfo.minXBot + i * (loadScreenInfo.championWidth + loadScreenInfo.xpadding), 
                            w.Height - loadScreenInfo.topPadding - loadScreenInfo.championHeight + 1, 
                            loadScreenInfo.scale);
                    }
                    return;
                }
                else  //in game now
                {
                    form.scriptControl.raiseEvent("interfaceInit", "");
                    outOfLoadScreen = true;
                }
            }

            /* Set Current Hero Name */

            if (heroName == "")
            {
                string curName = "";
                double curRMS = 1000000.0;
                double rms = 0;

                Bitmap cBit = new Bitmap((Image)form.windowImage.Clone(LeagueUI.playerAvatar, System.Drawing.Imaging.PixelFormat.Undefined), new System.Drawing.Size(120, 120)); ;
                cBit = cBit.Clone(new System.Drawing.Rectangle((int)(cBit.Width * .25), (int)(cBit.Height * .25), (int)(cBit.Width * .5), (int)(cBit.Height * .5)), System.Drawing.Imaging.PixelFormat.Undefined);
                Bitmap bit;
                // loop through heroes and find the one with the lowest rms diff.
                foreach (string c in cnames.Keys)
                {
                    bit = new Bitmap("C:\\Riot Games\\League of Legends\\air\\assets\\images\\champions\\" + c + "_Square_0.png");
                    bit = bit.Clone(new System.Drawing.Rectangle((int)(bit.Width * .25), (int)(bit.Height * .25), (int)(bit.Width * .5), (int)(bit.Height * .5)), System.Drawing.Imaging.PixelFormat.Undefined);

                    rms = calcRMSDiff(cBit, bit);
                    bit.Dispose();
                    if (rms < curRMS)
                    {
                        curRMS = rms;
                        curName = c;
                    }
                    //  Console.WriteLine(cnames[i] + " " + rms);

                }
                heroName = curName;
                Console.WriteLine("Hero Name" + heroName);
            }

            /* End Setting Hero Name */

            /**********************************************************************/

            /* Set Current Hero Level */
            int tempLevel = 1;
            int thinkLevel = 1;
            Bitmap levelBit;
            Bitmap wLevelBit;

            //Console.WriteLine(LeagueUI.cLevel);
            wLevelBit = new Bitmap(form.windowImage.Clone(LeagueUI.cLevel, System.Drawing.Imaging.PixelFormat.Undefined),new System.Drawing.Size(12,8));
            double lrms = 0, curlrms = 1000000.0;

            //wLevelBit.Save("LEVEL.png");

            foreach (FileInfo f in (new DirectoryInfo("levelImages")).GetFiles())
            {
                string[] split = f.Name.Split("_.".ToCharArray());
                levelBit = new Bitmap(new Bitmap(f.FullName).Clone(new Rect(3, 3, (12), (8)), System.Drawing.Imaging.PixelFormat.Undefined));
                // if (Convert.ToInt32(split[1]) == 2) levelBit.Save("C://Level2.png");
                lrms = calcRMSDiff(levelBit, wLevelBit);
                // lrms = calcDiff(levelBit, wLevelBit);
                //Console.WriteLine(f.Name + " " + lrms);
               //levelBit.Save("C:\\LEVELTHink" + split[1]+".png");
                //Console.WriteLine(f.Name + " rms " + lrms);
                if (lrms < curlrms)
                {
                    curlrms = lrms;
                    thinkLevel = Convert.ToInt32(split[1]);
                    //levelBit.Save("LEVELTHink.png");
                }
                // tempLevel++;
            }

            if (thinkLevel > currentLevel)
            {
                currentLevel = thinkLevel;
                Console.WriteLine("Hero Level" + currentLevel);
                form.scriptControl.levelUp();
            }

            /* End Setting Hero Level*/

            /*********************************************************************/

            /* Set Available Abilities */
            //  Console.WriteLine(canChooseAbility[0] + " " + canChooseAbility[1] + " " + canChooseAbility[2] + " " + canChooseAbility[3]);
            /* End Setting Available Abilities */

            /*********************************************************************/

            /* Set Health Percent */
            Bitmap hBit = form.windowImage.Clone(LeagueUI.cHealth, System.Drawing.Imaging.PixelFormat.Undefined);
            SColor hColor;
            int hCount = 0;
            for (int i = 0; i < hBit.Width; i++)
            {
                for (int j = 0; j < hBit.Height; j++)
                {
                    hColor = hBit.GetPixel(i, j);
                    if (hColor.G > 100 && hColor.R < 100 && hColor.B < 100)
                        hCount++;

                }
            }

            if (healthPixCount == -1)
            {
                healthPercent = 100.0;
                healthPixCount = hCount;
            }
            else
            {
                healthPercent = (double)hCount / (double)healthPixCount;
            }
            Console.WriteLine("health percent" + healthPercent);
            /* Set Mana/Energy Percent */

            /* Set Ability Cooldowns */
            /*
            abilityRectangles = new Rect[]{
                LeagueUI.cAbility1R,LeagueUI.cAbility2R,LeagueUI.cAbility3R,LeagueUI.cAbility4R,LeagueUI.sAbility1R,LeagueUI.sAbility2R};
            for (int i = 0; i < abilityRectangles.Length; i++)
            {
                double curPerc = getAbilityCooldownPercent(abilityRectangles[i]);
                TimeSpan ts = DateTime.Now - lastUpdate;
                if (curPerc >= 0 && lastCooldownPerc[i] < 0)
                {
                    cooldownStartTime[i] = DateTime.Now;
                }
                else if (curPerc >= 0 && lastCooldownPerc[i] >= 0)
                {
                    //this is sort of wrong, but it may work decently
                    double c = (DateTime.Now - cooldownStartTime[i]).TotalSeconds / curPerc;
                    remainingCooldown[i] = (int)Math.Ceiling((1-curPerc)*c);

                }
                else
                {
                    remainingCooldown[i] = 0;
                }
                lastCooldownPerc[i] = curPerc;
                
            }
            */


            lastUpdate = DateTime.Now;
            
        }

        public double getAbilityCooldownPercent(Rect abilityRect)
        {
            int cx = abilityRect.Left + abilityRect.Width / 2;
            int cy = abilityRect.Top + abilityRect.Height / 2;
            int r = abilityRect.Width / 4;

            //check to see if its on cooldown at all
            int countAbove=0;
            int total = 0;
            int superBlueCount=0;
            for (int x = cx - r; x <= cx + r; x++)
            {
                for (int y = cy - r; y <= cy + r; y++)
                {
                    if (form.windowImage.GetPixel(x, y).B >= 150)
                    {
                        countAbove++;
                    }
                    if (form.windowImage.GetPixel(x, y).B >= 2 * form.windowImage.GetPixel(x, y).R && form.windowImage.GetPixel(x, y).B >= 2 * form.windowImage.GetPixel(x, y).G)
                    {
                        superBlueCount++;
                    }
                    total += 1;
                }
            }
            if (superBlueCount / (double)total > 0.8)
            {
                //Console.WriteLine("On cooldown:" + (countAbove / (double)total));
                return countAbove / (double)total;
            }
            else
            {
               // Console.WriteLine("no cooldown");              
            }

            return -1;
        }

        [AttrLuaFunc("GetHeroLevel")]
        public static int getHeroLevel()
        {
            return currentLevel;
        }

        [AttrLuaFunc("GetHeroName")]
        public static string getHeroName()
        {
            return heroName;
        }

        [AttrLuaFunc("GetHealthPercent")]
        public static double getHealthPercent()
        {
            return healthPercent;
        }
        [AttrLuaFunc("CanLevelAbility")]
        public static bool canLevelAbility(int abilityNum)
        {
            return canChooseAbility[abilityNum];
        }
        [AttrLuaFunc("GetAbilityCooldown")]
        public int getAbilityCooldown(int abilityNum)
        {
            return remainingCooldown[abilityNum];
        }
        public bool canLevelAbility(Rectangle r)
        {
            int count = 0;
            for (int i = 0; i < (int)(r.Width ); i++)
            {
                for (int j = 0; j < (int)(r.Height ); j++)
                {
                    var temp = form.windowImage.GetPixel(r.X + 1 + i, r.Y + 1 + j);
                    if (temp.R > 175 && temp.B < 150 && temp.G < 150) count++;
                }
            }

            double total = r.Width * r.Height;
            if ((double)count / (double)total > .1) return true;
            return false;
        }

        public double calcRMSDiff(Bitmap r, Bitmap b)
        {

            double sumR = 0, sumG = 0, sumB = 0, sum = 0;
            int w, h;
            w = r.Width;
            h = r.Height;

            System.Drawing.Imaging.BitmapData bd = r.LockBits(new System.Drawing.Rectangle(0, 0, r.Width, r.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ip = bd.Scan0;
            int bytes = bd.Stride * r.Height;
            byte[] rBytes = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ip, rBytes, 0, bytes);
            r.UnlockBits(bd);

            bd = b.LockBits(new System.Drawing.Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            ip = bd.Scan0;
            byte[] bBytes = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ip, bBytes, 0, bytes);
            b.UnlockBits(bd);

            for (int i = 0; i < w*h*3; i++)
            {
                     sum += Math.Pow(rBytes[i] - bBytes[i], 2);
            }

            return (1 / ((double)(h * w))) * sum;

        }

        public double calcRMSDiff(Bitmap r, Bitmap b, Rectangle rect)
        {

            double sumR = 0, sumG = 0, sumB = 0, sum = 0;
            int w, h;
            w = r.Width;
            h = r.Height;
            if (rect.Right > b.Width || rect.Height > b.Height || rect.X > b.Width || rect.Y > b.Height) return double.MaxValue;
            for (int j = rect.Top; j < rect.Bottom; j++)
            {
                for (int i = rect.Left; i < rect.Right; i++)
                {
                    var wi = r.GetPixel(i, j);
                    var bi = b.GetPixel(i, j);
                    //red
                    sumR = Math.Pow(wi.R - bi.R, 2);
                    //green
                    sumG = Math.Pow(wi.G - bi.G, 2);
                    //blue
                    sumB = Math.Pow(wi.B - bi.B, 2);

                    sum += (sumR + sumG + sumB);
                    sumR = sumG = sumB = 0;

                }
            }

            return (1 / ((double)(h * w))) * sum;

        }

        public bool[] levelableAbilities(int[] ab,int curLevel)
        {
            bool[] temp = new bool[4];

            //regular abilities
            for (int i = 0; i < 3; i++)
            {
                if (ab[i] * 2 + 1 <= curLevel)
                    temp[i] = true;
            }
            //ultimate
            if (curLevel == 18)
                temp[3] = true;
            else if (curLevel == 12)
                temp[3] = true;
            else if (curLevel > 12 && ab[3] < 2)
                temp[3] = true;
            else if (curLevel == 6)
                temp[3] = true;
            else if (curLevel > 6 && ab[3] < 1)
                temp[3] = true;

            return temp;
        }
        [AttrLuaFunc("GetSummonerCount")]
        public int getSummonerCount(int team)
        {
            if (summonerInfo[team] == null) return 0;
            return summonerInfo[team].Length;
        }

        [AttrLuaFunc("GetSummonerInfo")]
        public LuaTable getSummonerInfo(int team, int summoner, LuaTable infoTable)
        {
            try
            {
                SummonerInfo si = summonerInfo[team][summoner];

                infoTable["championName"] = cnames[si.championCodeName];
                infoTable["championCodeName"] = si.championCodeName;

                Console.WriteLine(si.summonerSpell1);
                infoTable["spell1CodeName"] = si.summonerSpell1;
                infoTable["spell1Name"] = si.summonerSpell1;

                infoTable["spell2CodeName"] = si.summonerSpell2;
                infoTable["spell2Name"] = si.summonerSpell2;
            }
            catch
            {
                infoTable["championName"] = "Ashe";
                infoTable["championCodeName"] = "Bowmaster";

                infoTable["spell1CodeName"] = "Spell_SummonerBoost";
                infoTable["spell1Name"] = "Cleanse";

                infoTable["spell2CodeName"] = "Spell_SummonerDot";
                infoTable["spell2Name"] = "Ignite";
            }
            return infoTable;
        }

        class SummonerInfo
        {
            public string championCodeName;
            public string summonerSpell1;
            public string summonerSpell2;
        }

        class LoadScreenInfo
        {
            public int 
                minX, 
                maxX, 
                minY,
                maxY,
                minXBot, 
                maxXBot,
                topChampionCount,
                botChampionCount,
                championWidth,
                championHeight,
                xpadding,
                topPadding;
            public double scale;
        }


            
        public void parseLoadingScreen()
        {
            Image b = form.windowImage;
            loadScreenInfo = new LoadScreenInfo();
            LoadScreenInfo lsi = loadScreenInfo;

            lsi.minX = b.Width - 1;
            lsi.maxX = 0;
            lsi.minXBot = b.Width - 1;
            lsi.maxXBot = 0;
            for (int x = 5; x < b.Width - 5; x++)
            {
                var c = ((Bitmap)b).GetPixel(x, b.Height / 4);
                if (c.B != 0 || c.R != 0 || c.G != 0)
                {
                    if (x < lsi.minX) lsi.minX = x;
                    if (x > lsi.maxX) lsi.maxX = x;
                }
                c = ((Bitmap)b).GetPixel(x, 3 * b.Height / 4);
                if (c.B != 0 || c.R != 0 || c.G != 0)
                {
                    if (x < lsi.minXBot) lsi.minXBot = x;
                    if (x > lsi.maxXBot) lsi.maxXBot = x;
                }
            }
            lsi.topChampionCount = (lsi.maxX - lsi.minX) / 180;
            lsi.botChampionCount = (lsi.maxXBot - lsi.minXBot) / 180;

            if (lsi.topChampionCount <= 0 || lsi.botChampionCount <= 0)
            {

                loadScreenInfo = null;
                return;
            }
            //find a safe bottom point
            int bot = b.Height / 2;
            for (int y = b.Height / 2; y >= 0; y--)
            {
                var c = ((Bitmap)b).GetPixel(lsi.minX + 20, y);
                if (c.B == 0 && c.R == 0 && c.G == 0)
                {
                    bot = y;
                    break;
                }
            }

            //get the champion heights
            lsi.minY = b.Height - 1;
            lsi.maxY = 0;
            for (int y = 5; y < bot; y++)
            {
                var c = ((Bitmap)b).GetPixel(lsi.minX + 20, y);
                if (c.B != 0 || c.R != 0 || c.G != 0)
                {
                    if (y < lsi.minY) lsi.minY = y;
                    if (y > lsi.maxY) lsi.maxY = y;
                }
            }


            

            lsi.scale = (lsi.maxY - lsi.minY) / 341.0;

            lsi.championWidth = (int)Math.Round(190 * lsi.scale);
            lsi.championHeight = (int)Math.Round(341 * lsi.scale);
            lsi.xpadding = (int)Math.Round(9 * lsi.scale);
            lsi.topPadding = (int)Math.Round(18 * lsi.scale);


            Console.WriteLine("Top champions:" + lsi.topChampionCount);
            Console.WriteLine("Bottom champions:" + lsi.botChampionCount);
            summonerInfo[0] = new SummonerInfo[lsi.topChampionCount];
            summonerInfo[1] = new SummonerInfo[lsi.botChampionCount];
            
             
        }

        SummonerInfo getSummonerInfo(Bitmap screenImage, int left, int top, double scale)
        {
            SummonerInfo si = new SummonerInfo();
            Bitmap champImg = screenImage.Clone(new Rectangle(left, top, (int)Math.Round(185 * scale), (int)Math.Round(305 * scale)), System.Drawing.Imaging.PixelFormat.DontCare);
            Bitmap sSpell1 = screenImage.Clone(new Rectangle(left + (int)Math.Round(72 * scale), top + (int)Math.Round(298 * scale), (int)Math.Round(23 * scale), (int)Math.Round(24 * scale)), System.Drawing.Imaging.PixelFormat.DontCare);
            Bitmap sSpell2 = screenImage.Clone(new Rectangle(left + (int)Math.Round(99 * scale), top + (int)Math.Round(298 * scale), (int)Math.Round(23 * scale), (int)Math.Round(24 * scale)), System.Drawing.Imaging.PixelFormat.DontCare);
            double minRMS1 = double.MaxValue;
            double minRMS2 = double.MaxValue;
            sSpell1 = new Bitmap(sSpell1, 64, 64);
            sSpell2 = new Bitmap(sSpell2, 64, 64);

            //check the color on sSpell1 if it is black summoner is not connected
            int blackCount=0;
            for (int x = 0; x < sSpell1.Width; x++)
            {
                for (int y = 0; y < sSpell1.Height; y++)
                {
                    var c = sSpell1.GetPixel(x, y);
                    if (c.R == 0 && c.G == 0 && c.B == 0) blackCount++;
                }
            }
            if (blackCount / (64.0 * 64.0) > 0.8) return null;

            foreach (FileInfo fi in new DirectoryInfo(@"C:\Riot Games\League of Legends\air\assets\images\spells").GetFiles("*.png"))
            {
                Bitmap temp = (Bitmap)Bitmap.FromFile(fi.FullName);
                double rms = calcRMSDiff(sSpell1, temp);
                if (rms < minRMS1)
                {
                    
                    si.summonerSpell1 = fi.Name.Split('.')[0];
                    minRMS1 = rms;
                }
                rms = calcRMSDiff(sSpell2, temp);
                if (rms < minRMS2)
                {
                    si.summonerSpell2 = fi.Name.Split('.')[0];
                    minRMS2 = rms;
                }
            }

            Console.WriteLine("S1 = " + si.summonerSpell1 + " minrms of" + minRMS1);
            Console.WriteLine("S2 = " + si.summonerSpell2);

            double minChampionRMS = double.MaxValue;
            champImg = new Bitmap(champImg, 307, 557);
            Rectangle compareRect = new Rectangle(0, 130, 150, 20);
            champImg.Clone(compareRect, System.Drawing.Imaging.PixelFormat.DontCare).Save("champ.png");
            foreach (FileInfo fi in new DirectoryInfo(@"C:\Riot Games\League of Legends\air\assets\images\champions").GetFiles("*.jpg"))
            {
                if (fi.Name.ToLower().Contains("_square_") || fi.Name.ToLower().Contains("_splash_") || fi.Name.Count(c => c == '_') != 1) continue;
                Bitmap temp = (Bitmap)Bitmap.FromFile(fi.FullName);
                if (fi.Name.ToLower().Contains("arms") && fi.Name.ToLower().Contains("0")) temp.Clone(compareRect, System.Drawing.Imaging.PixelFormat.DontCare).Save("morde.png");
                double rms = calcRMSDiff(champImg, temp, compareRect);
                if (rms < minChampionRMS)
                {
                    si.championCodeName = fi.Name.Split('_')[0];
                    temp.Clone(compareRect, System.Drawing.Imaging.PixelFormat.DontCare).Save("bestfit.png");
                    minChampionRMS = rms;
                }
            }
            Console.WriteLine("champion is " + si.championCodeName);
            return si;
        }

    }
}
