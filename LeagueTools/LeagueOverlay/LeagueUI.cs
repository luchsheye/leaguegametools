using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rect = System.Drawing.Rectangle;
using LuaInterface;

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
        public static void setMainWindow(MainWindow mw)
        {
            parent = mw;
        }
        //calculate all of the locations for screen elements
        public static void init(int xRes, int yRes)
        {
            

         
            //92,747
            double widthScale = xRes / 1280.0;
            double heightScale = yRes / 758.0;

            ///////
            int tempx = (int)Math.Round(73 * widthScale);
            int tempy = (int)Math.Round(yRes - (768 - 734) * widthScale);
            int wid = (int)Math.Round(19 * widthScale);
            int hi = (int)Math.Round(13 * widthScale);

            
            int cLevelWidth = (int)Math.Round(12 * widthScale);
            int cLevelHeight0 = (int)Math.Round(8 * widthScale);

            cLevel = new Rect((int)Math.Round(tempx+ wid / 2.0 - cLevelWidth/2.0), (int)Math.Round(tempy + hi / 2.0 - cLevelHeight0/2.0), cLevelWidth, cLevelHeight0);


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

            parent.scriptControl.raiseEvent("interfaceInit", "");
        }

        
        //functions for getting UI element positions in LUA
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
