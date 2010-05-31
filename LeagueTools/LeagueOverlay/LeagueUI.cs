using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rect = System.Drawing.Rectangle;

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


        //calculate all of the locations for screen elements
        public static void init(int xRes, int yRes)
        {
            //92,747
            double widthScale = xRes / 1280.0;
            double heightScale = yRes / 758.0;

            int plusSize = (int)Math.Round(16 * widthScale);
            int abilitySize = (int)Math.Round(42 * widthScale);
            int firstXPos = (int)Math.Round(507 * widthScale);
            int abilityHeight = (int)Math.Round(yRes - 105 * widthScale);

            int cLevelWidth = (int)Math.Round(19 * widthScale);
            int cLevelHeight0 = (int)Math.Round(13 * widthScale);
            int avatarSize = (int)Math.Round(102 * widthScale);
            int abPlusHeight = (int)Math.Round(yRes - 142 * widthScale);
            int clevelHeight = (int)Math.Floor (yRes - (768 - 735) * widthScale);
            int playerAvatarHeight = (int)Math.Round(yRes - (117) * widthScale);

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
            cLevel = new Rect((int)Math.Floor(73 * widthScale), clevelHeight, cLevelWidth, cLevelHeight0);

        }
    }
}
