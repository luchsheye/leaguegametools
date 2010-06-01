using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LeagueOverlay
{
    public class UIComponents
    {
       
        
        public Dictionary<int, UIElement> activeComponents;

        public int compNum = 0;

        MainWindow parent;
        public UIComponents(MainWindow parent)
        {
            activeComponents = new Dictionary<int, UIElement>();
            this.parent = parent;
        }

        //functions available to scripting
        //general component functions
        [AttrLuaFunc("SetComponentPos")]
        public bool setComponentPos(int c, int x, int y)
        {
            if (activeComponents.ContainsKey(c))
            {
                UIElement uie = activeComponents[c];
                Canvas.SetLeft(uie, x);
                Canvas.SetTop(uie, y);
                return true;
            }
            return false;
        }
        [AttrLuaFunc("GetComponentX")]
        public int getComponentX(int c)
        {
            if (activeComponents.ContainsKey(c))
            {
                return (int)Canvas.GetLeft(activeComponents[c]);
            }
            return -9999;
        }
        [AttrLuaFunc("GetComponentY")]
        public int getComponentY(int c)
        {
            if (activeComponents.ContainsKey(c))
            {
                return (int)Canvas.GetTop(activeComponents[c]);
            }
            return -9999;
        }
        [AttrLuaFunc("RemoveComponent")]
        public bool removeComponent(int c)
        {
            
            if (activeComponents.ContainsKey(c))
            {
                parent.mainCanvas.Children.Remove(activeComponents[c]);
                activeComponents.Remove(c);
                return true;
            }
            return false;
        }
        [AttrLuaFunc("SetComponentVisible")]
        public bool setComponentVisible(int c, bool visible)
        {
            if (activeComponents.ContainsKey(c))
            {
                activeComponents[c].Visibility = (visible) ? Visibility.Visible : Visibility.Hidden;
                return true;
            }
            return false;
        }

        //label functions
        [AttrLuaFunc("NewLabel")]
        public int newLabel()
        {
            int id = compNum++;
            Label l = new Label();
            
            
            activeComponents[id] = new Label();
            Canvas.SetLeft(activeComponents[id], 0);
            Canvas.SetTop(activeComponents[id], 0);
            parent.mainCanvas.Children.Add(activeComponents[id]);
            return id;
        }
        [AttrLuaFunc("SetLabelText")]
        public bool setLabelText(int l, string text)
        {
            if (activeComponents.ContainsKey(l) && activeComponents[l] is Label)
            {
                ((Label)activeComponents[l]).Content = text;
                return true;
            }
            return false;
        }
        [AttrLuaFunc("SetLabelFont")]
        public bool setLabelFont(int l, string font, int size)
        {
            if (activeComponents.ContainsKey(l) && activeComponents[l] is Label)
            {
                try
                {
                    ((Label)activeComponents[l]).FontFamily = new FontFamily(font);
                    ((Label)activeComponents[l]).FontSize = size;
                }
                catch
                {
                    ((Label)activeComponents[l]).FontFamily = new FontFamily("Comic Sans");
                    return false;
                }
                return true;
            }
            return false;
        }
        [AttrLuaFunc("SetLabelColor")]
        public bool setLabelColor(int l, byte a, byte r, byte g, byte b)
        {
            if (activeComponents.ContainsKey(l) && activeComponents[l] is Label)
            {
                try
                {
                    ((Label)activeComponents[l]).Foreground = new SolidColorBrush(
                        Color.FromArgb(a, r, g, b)
                        );
                }
                catch
                {
                    ((Label)activeComponents[l]).Foreground = Brushes.White;
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
