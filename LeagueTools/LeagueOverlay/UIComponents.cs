using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

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

        //Rectangle functions

        [AttrLuaFunc("NewRectangle")]
        public int newRectangle()
        {
            int id = compNum++;

            activeComponents[id] = new Rectangle();
            Canvas.SetLeft(activeComponents[id], 0);
            Canvas.SetTop(activeComponents[id], 0);
            parent.mainCanvas.Children.Add(activeComponents[id]);
            return id;
        }

        [AttrLuaFunc("SetRectangleSize")]
        public bool setRectangleSize(int r, int w, int h)
        {
            if (activeComponents.ContainsKey(r) && activeComponents[r] is Rectangle)
            {
                ((Rectangle)activeComponents[r]).Width = w;
                ((Rectangle)activeComponents[r]).Height = h;
                return true;
            }
            return false;
        }

        [AttrLuaFunc("SetRectangleBgColor")]
        public bool setRectangleBgColor(int r, byte alpha, byte red, byte green, byte blue)
        {
            if (activeComponents.ContainsKey(r) && activeComponents[r] is Rectangle)
            {
                try
                {
                    ((Rectangle)activeComponents[r]).Fill = new SolidColorBrush(
                        Color.FromArgb(alpha, red, green, blue)
                        );
                }
                catch
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        [AttrLuaFunc("SetRectangleImage")]
        public bool setRectangleBgColor(int r, string path)
        {
            if (activeComponents.ContainsKey(r) && activeComponents[r] is Rectangle)
            {
                try
                {
                    ((Rectangle)activeComponents[r]).Fill = new ImageBrush(new BitmapImage(new Uri(path)));
                }
                catch
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        [AttrLuaFunc("SetRectangleClickEvent")]
        public bool setRectangleClickEvent(int r, string func)
        {
            if (activeComponents.ContainsKey(r) && activeComponents[r] is Rectangle)
            {
                ((Rectangle)activeComponents[r]).Tag = func;
                return true;
            }
            return false;
        }
    }
}
