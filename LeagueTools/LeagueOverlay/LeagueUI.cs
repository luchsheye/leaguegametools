using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Rect = System.Drawing.Rectangle;
using LuaInterface;
using System.IO;

namespace LeagueOverlay
{
    public class LeagueUI
    {
        public static Rect
            mapR,

            cLevel,
            cHealth,
            cMana,

            cAbility1R,
            cAbility2R,
            cAbility3R,
            cAbility4R,

            sAbility1R,
            sAbility2R,

            ally1R,
            ally2R,
            ally3R,
            ally4R,

            ab1Plus,
            ab2Plus,
            ab3Plus,
            ab4Plus,

            playerAvatar;
        
        public static MainWindow parent;
        public static int xResolution, yResolution,levelBitSize=0;
        public static byte[][] levelBmBytes = new byte[18][];
        public static Dictionary<int, Rect> levelRects = new Dictionary<int, Rect>();
        public static void setMainWindow(MainWindow mw)
        {
            parent = mw;
            
        }
        public static void loadLevelBitmaps()
        {
            
            Bitmap[] levelBitmaps = new Bitmap[18];
            System.Drawing.Imaging.BitmapData bd;
            IntPtr ip;
            int index;
            foreach (FileInfo f in (new DirectoryInfo("levelImages")).GetFiles())
            {
                string[] split = f.Name.Split("_.".ToCharArray());
                index = Convert.ToInt32(split[1])-1;
                levelBitmaps[index]= new Bitmap(new Bitmap(f.FullName).Clone(new Rect(3, 3, (12), (8)), System.Drawing.Imaging.PixelFormat.Undefined));
                bd = levelBitmaps[index].LockBits(new System.Drawing.Rectangle(0, 0,  12,  8), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                ip = bd.Scan0;
                levelBitSize = bd.Stride * levelBitmaps[index].Height;
                levelBmBytes[index] = new byte[levelBitSize];
                System.Runtime.InteropServices.Marshal.Copy(ip, levelBmBytes[index], 0, levelBitSize);
                levelBitmaps[index].UnlockBits(bd);
                
            }

                
                
        }
        //calculate all of the locations for screen elements
        public static void init(int xRes, int yRes)
        {
            levelRects[1024] = new Rect(60, 768 - 743, 10, 6);
            levelRects[1152] = new Rect(68, 864 - 836, 11, 7);
            levelRects[1280] = new Rect(76, 768 - 737, 12, 8);

            xResolution = xRes;
            yResolution = yRes;
         
            //92,747
            double widthScale = xRes / 1280.0;
            double heightScale = yRes / 758.0;

             
            cLevel = new Rect(levelRects[xRes].X, yRes - levelRects[xRes].Y, levelRects[xRes].Width, levelRects[xRes].Height);


            ////

            int plusSize = (int)Math.Round(16 * widthScale);
            int abilitySize = (int)Math.Round(42 * widthScale);
            int firstXPos = (int)Math.Round(507 * widthScale);
            int abilityHeight = (int)Math.Round(yRes - 105 * widthScale);

            int avatarSize = (int)Math.Round(102 * widthScale);
            int abPlusHeight = (int)Math.Round(yRes - 142 * widthScale) -(int)(Math.Ceiling(1.0/widthScale));
            int clevelHeight = (int)Math.Floor (yRes - (31) * widthScale)-1;
            int playerAvatarHeight = (int)Math.Round(yRes - (117) * widthScale);
            //3 3 - 14 10
            cAbility1R = new Rect((int)Math.Round((507) * widthScale), abilityHeight, abilitySize, abilitySize);
            cAbility2R = new Rect((int)Math.Round((507 + 42) * widthScale), abilityHeight, abilitySize, abilitySize);
            cAbility3R = new Rect((int)Math.Round((593) * widthScale), abilityHeight, abilitySize, abilitySize);
            cAbility4R = new Rect((int)Math.Round((593 + 42) * widthScale), abilityHeight, abilitySize, abilitySize);

            sAbility1R = new Rect((int)Math.Round((691) * widthScale), abilityHeight, abilitySize, abilitySize);
            sAbility2R = new Rect((int)Math.Round((691 + 42) * widthScale), abilityHeight, abilitySize, abilitySize);

            ab1Plus = new Rect((int)Math.Round(522 * widthScale), abPlusHeight, plusSize, plusSize);
            ab2Plus = new Rect((int)Math.Round(562 * widthScale), abPlusHeight, plusSize, plusSize);
            ab3Plus = new Rect((int)Math.Round(602 * widthScale), abPlusHeight, plusSize, plusSize);
            ab4Plus = new Rect((int)Math.Round(642 * widthScale), abPlusHeight, plusSize, plusSize);

            cHealth = new Rect(
                (int)Math.Round((450) * widthScale),
                (int)Math.Round(yRes - 50 * widthScale),
                (int)Math.Round(381 * widthScale),
                (int)Math.Round(14 * widthScale));
            cMana = new Rect(
                (int)Math.Round((450) * widthScale),
                (int)Math.Round(yRes - 30 * widthScale),
                (int)Math.Round(381 * widthScale),
                (int)Math.Round(14 * widthScale));

            playerAvatar = new Rect((int)Math.Round(9 * widthScale), playerAvatarHeight, avatarSize, avatarSize);
           // cLevel = new Rect((int)Math.Floor(76 * widthScale), clevelHeight, cLevelWidth, cLevelHeight0);

            if(parent.leagueInfo.outOfLoadScreen)
                parent.scriptControl.raiseEvent("interfaceInit", "");
        }

        
        //functions for getting UI element positions in LUA
        [AttrLuaFunc("GetResolutionX")]
        public int getResolutionX() { return xResolution; }
        [AttrLuaFunc("GetResolutionY")]
        public int getResolutionY() { return yResolution; }

        [AttrLuaFunc("GetUIRect_Map")]
        public void LUA_getmapR(LuaTable table) { parent.scriptControl.storeRectInTable(mapR, table); }
        [AttrLuaFunc("GetUIRect_ChampionLevel")]
        public void LUA_getcLevel(LuaTable table) { parent.scriptControl.storeRectInTable(cLevel, table); }
        [AttrLuaFunc("GetUIRect_ChampionHealth")]
        public void LUA_getcHealth(LuaTable table) { parent.scriptControl.storeRectInTable(cHealth, table); }
        [AttrLuaFunc("GetUIRect_ChampionMana")]
        public void LUA_getcMana(LuaTable table) { parent.scriptControl.storeRectInTable(cMana, table); }
        [AttrLuaFunc("GetUIRect_ChampionAbility1")]
        public void LUA_getcAbility1R(LuaTable table) { parent.scriptControl.storeRectInTable(cAbility1R, table); }
        [AttrLuaFunc("GetUIRect_ChampionAbility2")]
        public void LUA_getcAbility2R(LuaTable table) { parent.scriptControl.storeRectInTable(cAbility2R, table); }
        [AttrLuaFunc("GetUIRect_ChampionAbility3")]
        public void LUA_getcAbility3R(LuaTable table) { parent.scriptControl.storeRectInTable(cAbility3R, table); }
        [AttrLuaFunc("GetUIRect_ChampionAbility4")]
        public void LUA_getcAbility4R(LuaTable table) { parent.scriptControl.storeRectInTable(cAbility4R, table); }
        [AttrLuaFunc("GetUIRect_SummonerAbility1")]
        public void LUA_getsAbility1R(LuaTable table) { parent.scriptControl.storeRectInTable(sAbility1R, table); }
        [AttrLuaFunc("GetUIRect_SummonerAbility2")]
        public void LUA_getsAbility2R(LuaTable table) { parent.scriptControl.storeRectInTable(sAbility2R, table); }
        [AttrLuaFunc("GetUIRect_PlayerAvatar")]
        public void LUA_getplayerAvatar(LuaTable table) { parent.scriptControl.storeRectInTable(playerAvatar, table); }
        [AttrLuaFunc("GetUIRect_AbilityByNum")]
        public LuaTable LUA_getAbilityByNum(int num, LuaTable table)
        {
            switch (num)
            {
                case 0:
                    return parent.scriptControl.storeRectInTable(cAbility1R, table);
                case 1:
                    return parent.scriptControl.storeRectInTable(cAbility2R, table);
                case 2:
                    return parent.scriptControl.storeRectInTable(cAbility3R, table);
                case 3:
                    return parent.scriptControl.storeRectInTable(cAbility4R, table);
                case 4:
                    return parent.scriptControl.storeRectInTable(sAbility1R, table);
                case 5:
                    return parent.scriptControl.storeRectInTable(sAbility2R, table);
            }
            return parent.scriptControl.storeRectInTable(new Rect(), table);
        }

    }
}
