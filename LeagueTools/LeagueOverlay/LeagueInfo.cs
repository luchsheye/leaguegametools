﻿using System;
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

        public bool outOfLoadScreen = false;
        LoadScreenInfo loadScreenInfo = null;

        SummonerInfo[][] summonerInfo = new SummonerInfo[2][];

        Dictionary<string, SummonerSpellInfo> summonerSpellInfo = new Dictionary<string, SummonerSpellInfo>();

        public LeagueInfo(MainWindow f)
        {
            form = f;
            Regex r = new Regex(@"""(\w+)_(\w+)"" = ""([^""]+)");
            foreach (string s in File.ReadAllLines(Preferences.leagueFolder + "\\game\\DATA\\Menu\\fontconfig_en_US.txt"))
            {
                
                Match m = r.Match(s);
               
                if (m.Success)
                {

                    string infoType = m.Groups[1].Value;
                    string infoID = m.Groups[2].Value;
                    string infoValue = m.Groups[3].Value;
                    if (infoType == "game_character_displayname"
                        && File.Exists(Preferences.leagueFolder + "\\air\\assets\\images\\champions\\" + infoID + "_Square_0.png"))
                    {
                        cnames.Add(infoID, infoValue);
                    }

                }
            }


            //init summoner spell info table
            SummonerSpellInfo ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerBoost";
            ssi.name = "Cleanse";
            ssi.cooldown = 130;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerClairvoyance";
            ssi.name = "Clairvoyance";
            ssi.cooldown = 55;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerDot";
            ssi.name = "Ignite";
            ssi.cooldown = 120;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerExhaust";
            ssi.cooldown = 210;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerFlash";
            ssi.name = "Flash";
            ssi.cooldown = 225;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerFortify";
            ssi.name = "Fortify";
            ssi.cooldown = 300;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerHaste";
            ssi.name = "Ghost";
            ssi.cooldown = 210;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerHeal";
            ssi.name = "Heal";
            ssi.cooldown = 270;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerMana";
            ssi.name = "Clarity";
            ssi.cooldown = 180;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerPromote";
            ssi.name = "Promote";
            ssi.cooldown = 300;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerRally";
            ssi.name = "Rally";
            ssi.cooldown = 360;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerRevive";
            ssi.name = "Revive";
            ssi.cooldown = 520;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerSmite";
            ssi.name = "Smite";
            ssi.cooldown = 75;
            summonerSpellInfo[ssi.codeName] = ssi;

            ssi = new SummonerSpellInfo();
            ssi.codeName = "Spell_SummonerTeleport";
            ssi.name = "Teleport";
            ssi.cooldown = 240;
            summonerSpellInfo[ssi.codeName] = ssi;


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
                    bit = new Bitmap(Preferences.leagueFolder + "\\air\\assets\\images\\champions\\" + c + "_Square_0.png");
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
            int thinkLevel = 1;
            Bitmap wLevelBit;

            //Console.WriteLine(LeagueUI.cLevel);
            wLevelBit = new Bitmap(form.windowImage.Clone(LeagueUI.cLevel, System.Drawing.Imaging.PixelFormat.Undefined),new System.Drawing.Size(12,8));
            double lrms = 0, curlrms = 1000000.0;
            System.Drawing.Imaging.BitmapData bd;
            IntPtr ip;
            bd = wLevelBit.LockBits(new System.Drawing.Rectangle(0, 0, wLevelBit.Width,wLevelBit.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            ip = bd.Scan0;
            int bytes= bd.Stride * wLevelBit.Height;
            byte[] wBytes = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ip, wBytes, 0, LeagueUI.levelBitSize);
            wLevelBit.UnlockBits(bd);
            wLevelBit.Save("LEVEL.png");

           for (int i =0;i<LeagueUI.levelBmBytes.Length;i++)
            {
              
                
                    // if (Convert.ToInt32(split[1]) == 2) levelBit.Save("C://Level2.png");
                lrms = calcRMSDiff(LeagueUI.levelBmBytes[i], wBytes);
               // lrms = calcRMSDiff(levelBit, wLevelBit);
                //Console.WriteLine(f.Name + " " + lrms);
               //levelBit.Save("C:\\LEVELTHink" + split[1]+".png");
                //Console.WriteLine(f.Name + " rms " + lrms);
                if (lrms < curlrms)
                {
                    curlrms = lrms;
                    thinkLevel = i+1;
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
            //Console.WriteLine("health percent" + healthPercent);
            /* Set Mana/Energy Percent */


            lastUpdate = DateTime.Now;
            
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

            double sum = 0;
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
        //two byte arrays
        public double calcRMSDiff(byte []  rBytes, byte[] bBytes)
        {
           
            double sum = 0;
            int w = 12;
            int h = 8;

            for (int i = 0; i < LeagueUI.levelBitSize; i++)
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
            if (summonerInfo[team] == null || summonerInfo[team][summoner] == null)
            {
                infoTable["championName"] = "Ashe";
                infoTable["championCodeName"] = "Bowmaster";

                infoTable["spell1CodeName"] = "Spell_SummonerBoost";
                infoTable["spell1Name"] = "Cleanse";
                infoTable["spell1Cooldown"] = 10;

                infoTable["spell2CodeName"] = "Spell_SummonerDot";
                infoTable["spell2Name"] = "Ignite";
                infoTable["spell2Cooldown"] = 10;
            }
            else
            {
                SummonerInfo si = summonerInfo[team][summoner];

                if (cnames.ContainsKey(si.championCodeName))
                    infoTable["championName"] = cnames[si.championCodeName];
                else
                    infoTable["championName"] = "Bad Name";
                infoTable["championCodeName"] = si.championCodeName;

                infoTable["spell1CodeName"] = si.summonerSpell1;
                if (summonerSpellInfo.ContainsKey(si.summonerSpell1))
                {
                    infoTable["spell1Name"] = summonerSpellInfo[si.summonerSpell1].name;
                    infoTable["spell1Cooldown"] = summonerSpellInfo[si.summonerSpell1].cooldown;
                }
                else
                {
                    infoTable["spell1Name"] = "??";
                    infoTable["spell1Cooldown"] = 10;
                }

                infoTable["spell2CodeName"] = si.summonerSpell2;
                if (summonerSpellInfo.ContainsKey(si.summonerSpell2))
                {
                    infoTable["spell2Name"] = summonerSpellInfo[si.summonerSpell2].name;
                    infoTable["spell2Cooldown"] = summonerSpellInfo[si.summonerSpell2].cooldown;
                }
                else
                {
                    infoTable["spell2Name"] = "??";
                    infoTable["spell2Cooldown"] = 10;
                }
            }
            return infoTable;
        }

        class SummonerInfo
        {
            public string championCodeName;
            public string summonerSpell1;
            public string summonerSpell2;
        }
        class SummonerSpellInfo
        {
            public string codeName;
            public string name;
            public int cooldown;
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

            bool foundNonBlack = false;
            for (int x = 5; x < b.Width - 5; x++)
            {
                var c = ((Bitmap)b).GetPixel(x, b.Height / 4);
                if (c.B != 0 || c.R != 0 || c.G != 0)
                {
                    if (x < lsi.minX) lsi.minX = x;
                    if (x > lsi.maxX) lsi.maxX = x;
                    foundNonBlack = true;
                }
                c = ((Bitmap)b).GetPixel(x, 3 * b.Height / 4);
                if (c.B != 0 || c.R != 0 || c.G != 0)
                {
                    if (x < lsi.minXBot) lsi.minXBot = x;
                    if (x > lsi.maxXBot) lsi.maxXBot = x;
                }
            }

            if (lsi.minX >= lsi.maxX || lsi.minXBot >= lsi.maxXBot || !foundNonBlack)
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

            if (lsi.minY >= lsi.maxY)
            {
                loadScreenInfo = null;
                return;
            }
            

            lsi.scale = (lsi.maxY - lsi.minY) / 341.0;

            lsi.topChampionCount = (int)((lsi.maxX - lsi.minX) / (185 * lsi.scale - 5));
            lsi.botChampionCount = (int)((lsi.maxXBot - lsi.minXBot) / (185 * lsi.scale - 5));

            if (lsi.topChampionCount <= 0 || lsi.botChampionCount <= 0)
            {

                loadScreenInfo = null;
                return;
            }

            lsi.championWidth = (int)Math.Round(190 * lsi.scale);
            lsi.championHeight = (int)Math.Round(341 * lsi.scale);
            lsi.xpadding = (int)Math.Round(9 * lsi.scale);
            lsi.topPadding = (int)Math.Round(18 * lsi.scale);


            //Console.WriteLine("Top champions:" + lsi.topChampionCount);
            //Console.WriteLine("Bottom champions:" + lsi.botChampionCount);
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

            foreach (FileInfo fi in new DirectoryInfo(Preferences.leagueFolder + @"\air\assets\images\spells").GetFiles("*.png"))
            {
                if (fi.Name.Contains("Stifle")) continue;
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
            foreach (FileInfo fi in new DirectoryInfo(Preferences.leagueFolder + @"\air\assets\images\champions").GetFiles("*.jpg"))
            {
                if (fi.Name.ToLower().Contains("_square_") || fi.Name.ToLower().Contains("_splash_") || fi.Name.Count(c => c == '_') != 1) continue;
                Bitmap temp = (Bitmap)Bitmap.FromFile(fi.FullName);
                double rms = calcRMSDiff(champImg, temp, compareRect);
                if (rms < minChampionRMS)
                {
                    si.championCodeName = fi.Name.Split('_')[0];
                    minChampionRMS = rms;
                }
            }
            Console.WriteLine("champion is " + si.championCodeName);
            return si;
        }

    }
}
