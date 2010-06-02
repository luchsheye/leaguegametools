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
        public static List<string> cnames = new List<string>();

        DateTime lastUpdate = DateTime.Now;
        int[] remainingCooldown;
        double[] lastCooldownPerc;
        Rect[] abilityRectangles;
        DateTime[] cooldownStartTime;
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
                    if (infoType == "game_character_lore" && infoValue.Length > 50
                        && File.Exists("C:\\Riot Games\\League of Legends\\air\\assets\\images\\champions\\" + infoID + "_Square_0.png"))
                    {
                        cnames.Add(infoID);
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
            //   canChooseAbility[0] = (canLevelAbility(LeagueUI.ab1Plus) ? true : false);
            // canChooseAbility[1] = (canLevelAbility(LeagueUI.ab2Plus) ? true : false);
            // canChooseAbility[2] = (canLevelAbility(LeagueUI.ab3Plus) ? true : false);
            //canChooseAbility[3] = (canLevelAbility(LeagueUI.ab4Plus) ? true : false);

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
                for (int i = 0; i < cnames.Count; i++)
                {
                    bit = new Bitmap("C:\\Riot Games\\League of Legends\\air\\assets\\images\\champions\\" + cnames[i] + "_Square_0.png");
                    bit = bit.Clone(new System.Drawing.Rectangle((int)(bit.Width * .25), (int)(bit.Height * .25), (int)(bit.Width * .5), (int)(bit.Height * .5)), System.Drawing.Imaging.PixelFormat.Undefined);

                    rms = calcRMSDiff(cBit, bit);
                    bit.Dispose();
                    if (rms < curRMS)
                    {
                        curRMS = rms;
                        curName = cnames[i];
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

            Console.WriteLine(LeagueUI.cLevel);
            wLevelBit = new Bitmap(form.windowImage.Clone(LeagueUI.cLevel, System.Drawing.Imaging.PixelFormat.Undefined),new System.Drawing.Size(12,8));
            double lrms = 0, curlrms = 1000000.0;

            wLevelBit.Save("C:\\LEVEL.png");
            foreach (FileInfo f in (new DirectoryInfo("levelImages")).GetFiles())
            {
                string[] split = f.Name.Split("_.".ToCharArray());
                levelBit = new Bitmap(new Bitmap(f.FullName).Clone(new Rect(3, 3, (12), (8)), System.Drawing.Imaging.PixelFormat.Undefined));
                // if (Convert.ToInt32(split[1]) == 2) levelBit.Save("C://Level2.png");
                lrms = calcRMSDiff(levelBit, wLevelBit);
                // lrms = calcDiff(levelBit, wLevelBit);
                //Console.WriteLine(f.Name + " " + lrms);
               levelBit.Save("C:\\LEVELTHink" + split[1]+".png");
                Console.WriteLine(f.Name + " rms " + lrms);
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
        public int calcDiff(Bitmap r, Bitmap b)
        {
            int w, h;
            w = r.Width;
            h = r.Height;
            int count =0;



            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var wi = r.GetPixel(i, j);
                    var bi = b.GetPixel(i, j);

                    if (bi.R > 50 && wi.R > 50) count++;


                }
            }
            return count;
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


        public double[] lastCooldownPercent { get; set; }
    }
}
