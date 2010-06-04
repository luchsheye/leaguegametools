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

        public static bool Load()
        {
            if (!File.Exists("preferences.xml"))
            {
                //create everything
                
                return false;
            }
            //load anything that is needed
            XmlDocument pref = new XmlDocument();
            pref.Load("preferences.xml");
            if (pref["Preferences"]["LeagueFolder"] != null)
            {
                leagueFolder = pref["Preferences"]["LeagueFolder"].InnerText;
            }
            if (pref["Preferences"]["DisclaimerAccepted"] != null)
            {
                disclaimerAccepted = Convert.ToBoolean(pref["Preferences"]["DisclaimerAccepted"].InnerText);
            }
            return true;
        }

        public static void Save()
        {
            XmlDocument xd = new XmlDocument();

            xd.LoadXml("<Preferences><LeagueFolder>" + leagueFolder + "</LeagueFolder><DisclaimerAccepted>" + disclaimerAccepted + "</DisclaimerAccepted></Preferences>");
            xd.Save("preferences.xml");
        }

    }
}
