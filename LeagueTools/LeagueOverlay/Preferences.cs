using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace LeagueOverlay
{
    public class Preferences
    {

        public static string leagueFolder = "C:\\Riot Games\\League of Legends";
        public static bool disclaimerAccepted = false;
        public static bool hideLeagueBorders = true;
        public static int processingTimer = 500;
        public static int UITimer = 32;

        public static bool Load()
        {
            if (!File.Exists("preferences.xml"))
            {
                //create everything


                Save();
                return false;
            }
            //load anything that is needed
            XmlDocument pref = new XmlDocument();
            int optionCount = 0;
            pref.Load("preferences.xml");
            if (pref["Preferences"]["LeagueFolder"] != null)
            {
                leagueFolder = pref["Preferences"]["LeagueFolder"].InnerText;
                optionCount++;
            }
            if (pref["Preferences"]["DisclaimerAccepted"] != null)
            {
                disclaimerAccepted = Convert.ToBoolean(pref["Preferences"]["DisclaimerAccepted"].InnerText);
                optionCount++;
            }
            if (pref["Preferences"]["HideLeagueBorders"] != null)
            {
                hideLeagueBorders = Convert.ToBoolean(pref["Preferences"]["HideLeagueBorders"].InnerText);
                optionCount++;
            }
            if (pref["Preferences"]["ProcessingTimer"] != null)
            {
                processingTimer = Convert.ToInt32(pref["Preferences"]["ProcessingTimer"].InnerText);
                optionCount++;
            }
            if (pref["Preferences"]["UITimer"] != null)
            {
                UITimer = Convert.ToInt32(pref["Preferences"]["UITimer"].InnerText);
                optionCount++;
            }
            if (optionCount != 5)
            {
                Save();
            }
            return true;
        }

        public static void Save()
        {
            XmlDocument xd = new XmlDocument();

            xd.LoadXml("<Preferences><LeagueFolder>" + leagueFolder + 
                "</LeagueFolder><DisclaimerAccepted>" + disclaimerAccepted + 
                "</DisclaimerAccepted><HideLeagueBorders>" + hideLeagueBorders + 
                "</HideLeagueBorders><ProcessingTimer>" + processingTimer + 
                "</ProcessingTimer><UITimer>" + UITimer +
                "</UITimer></Preferences>");
            xd.Save("preferences.xml");
        }

    }
}
