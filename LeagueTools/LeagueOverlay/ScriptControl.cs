using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaInterface;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace LeagueOverlay
{
    public class ScriptControl
    {
        Lua LuaVM;

        Dictionary<string, List<string>> eventTable = new Dictionary<string, List<string>>();

        MainWindow parent;

        DateTime ProgramStartTime = DateTime.Now;
        DateTime lastUpdate;
        public ScriptControl(MainWindow parent)
        {
            this.parent = parent;
            eventTable["update"] = new List<string>();
            eventTable["interfaceInit"] = new List<string>();
            eventTable["processingFinished"] = new List<string>();
            eventTable["levelUp"] = new List<string>();
            LuaVM = new Lua();
            registerLuaFunctions(this);

            //LuaVM.RegisterFunction("RegisterEvent", this, parent.leagueInfo.GetType().GetMethod("registerEvent"));
        }
        public LuaTable storeRectInTable(System.Drawing.Rectangle r, LuaTable table)
        {
            table["x"] = r.X;
            table["y"] = r.Y;
            table["width"] = r.Width;
            table["height"] = r.Height;
            return table;
        }

        public void registerLuaFunctions(Object o)
        {
            Type pTrgType = o.GetType();
            foreach (MethodInfo mInfo in pTrgType.GetMethods())
            {
                foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                {
                    if (attr.GetType() == typeof(AttrLuaFunc))
                    {
                        AttrLuaFunc pAttr = (AttrLuaFunc)attr;
                        LuaVM.RegisterFunction(
                            pAttr.getFuncName(),
                            o,
                            mInfo);

                    }
                }
            }
        }
        
        public void LoadScriptFiles()
        {
            foreach (FileInfo f in (new DirectoryInfo("scripts")).GetFiles())
            {
                if (f.Name.Contains(".lua"))
                {
                    LuaVM.DoFile(f.FullName);
                    
                    /*
                    try
                    {

                        
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show(e.Message,"Lua Error: " + f.Name);
                        Console.WriteLine(e.StackTrace);
                    }
                     * */
                }
            }
        }

        public void doMouseClick()
        {
            int mouseX = (int)(System.Windows.Forms.Cursor.Position.X - parent.Left);
            int mouseY = (int)(System.Windows.Forms.Cursor.Position.Y - parent.Top);

            foreach (UIElement uie in parent.uicomponents.activeComponents.Values)
            {
                Rectangle r = uie as Rectangle;
                if (r != null && r.Tag != null && r.Visibility == Visibility.Visible)
                {
                    
                    int leftPos = (int)Canvas.GetLeft(r);
                    int topPos = (int)Canvas.GetTop(r);
                    if (mouseX >= leftPos && mouseX <= leftPos + r.Width && mouseY >= topPos && mouseY <= topPos + r.Height)
                    {                       
                        LuaVM.DoString((string)r.Tag);
                    }
                }
            }
        }

        [AttrLuaFunc("RegisterEvent")]
        public void registerEvent(string eventName, string functionName)
        {
            if (eventTable.ContainsKey(eventName))
            {
                eventTable[eventName].Add(functionName);
            }
        }

        public void raiseEvent(string eventName,string arguments)
        {
            if (eventTable.ContainsKey(eventName))
            {
                foreach (string s in eventTable[eventName])
                {
                    LuaVM.DoString(s + "(" + arguments + ")");
                }
            }
        }
        public void update()
        {
            if (lastUpdate == null) lastUpdate = DateTime.Now;
            int elapsedTime = (int)Math.Round((DateTime.Now - lastUpdate).TotalMilliseconds);
            foreach (string s in eventTable["update"])
            {
                LuaVM.DoString(s + "(" + elapsedTime + ");");
            }
            lastUpdate = DateTime.Now;
        }
        public void levelUp()
        {
            foreach (string s in eventTable["levelUp"])
            {
                
                LuaVM.DoString(s + "(" + LeagueInfo.currentLevel + ");");
            }
        }

        //some basic functions that dont really fit anywhere
        [AttrLuaFunc("PrintMsg")]
        public void printMsg(string msg)
        {
            Console.WriteLine(msg);
        }

        [AttrLuaFunc("GetmsTime")]
        public int getmsTime()
        {
            return (int)Math.Round((DateTime.Now - ProgramStartTime).TotalMilliseconds);
        }
        [AttrLuaFunc("GetLeagueDir")]
        public string getLeagueDir()
        {
            return Preferences.leagueFolder + "\\";
        }
    }
}
