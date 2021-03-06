﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Windows.Threading;
using System.IO;

using Keys = System.Windows.Forms.Keys;
using Bitmap = System.Drawing.Bitmap;

namespace LeagueOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IntPtr windowHandle;
        IntPtr windowBitmapHandle;
        //public byte[] windowBytes;
        public Bitmap windowImage = new Bitmap(1, 1);
        public LeagueInfo leagueInfo;
        public ScriptControl scriptControl;
        public UIComponents uicomponents;
        public KeyboardManager keyboardManager;

        DispatcherTimer processingTimer, UILogicTimer;

        public MainWindow()
        {
            InitializeComponent();
            LeagueUI.loadLevelBitmaps();
        }
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //Set the window style to noactivate.
            WindowInteropHelper helper = new WindowInteropHelper(this);
            WIN32_API.SetWindowLong(helper.Handle, -20,
                WIN32_API.GetWindowLong(helper.Handle, -20) | 0x08000000 | 0x00000020);
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Preferences.Load();

            leagueInfo = new LeagueInfo(this);
            scriptControl = new ScriptControl(this);
            uicomponents = new UIComponents(this);
            keyboardManager = new KeyboardManager(this);
            LeagueUI.setMainWindow(this);

            scriptControl.registerLuaFunctions(leagueInfo);
            scriptControl.registerLuaFunctions(uicomponents);
            scriptControl.registerLuaFunctions(keyboardManager);
            scriptControl.registerLuaFunctions(new LeagueUI());
            scriptControl.LoadScriptFiles();

            Width = 0;
            Height = 0;
            
            //create the timers
            processingTimer = new DispatcherTimer();
            processingTimer.Interval = TimeSpan.FromMilliseconds(Preferences.processingTimer);
            processingTimer.Tick += new EventHandler(processingTimer_Tick);
            processingTimer.Start();

            UILogicTimer = new DispatcherTimer();
            UILogicTimer.Interval = TimeSpan.FromMilliseconds(Preferences.UITimer);
            UILogicTimer.Tick += new EventHandler(UILogicTimer_Tick);
            UILogicTimer.Start();

        }

        void UILogicTimer_Tick(object sender, EventArgs e)
        {
            //do logic such as updating timers or UI elements
            //DateTime start = DateTime.Now;
            DisplayTimers.updateTimers();
            keyboardManager.update();
            scriptControl.update();
            //UITimeLabel.Content = "UI Time:" + (int)(DateTime.Now - start).TotalMilliseconds + "ms";
        }

        void processingTimer_Tick(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder(256);
            IntPtr tempHandle = WIN32_API.GetForegroundWindow();
            WIN32_API.GetWindowText(tempHandle, sb, (IntPtr)sb.MaxCapacity);

            if (windowHandle != IntPtr.Zero)
            {
                uint windowStyle = (uint)WIN32_API.GetWindowLong(windowHandle, WIN32_API.GWL_STYLE);
                if (windowStyle == 0)
                {
                    scriptControl.log("Closing...League of legends window no longer detected");
                    System.Environment.Exit(1);
                    return;
                }
                else if ((windowStyle & 0x80000) != 0x80000)
                {
                    scriptControl.log("Error: League was in fullscreen mode when the overlay started");
                    this.Close();
                    return;
                }
            }
            if (sb.ToString().ToLower().Contains("league of legends (tm) client"))
            {
                this.Visibility = Visibility.Visible;
                DateTime start = DateTime.Now;

                //grab the current screen image from league of legends
                windowHandle = tempHandle;
                Bitmap temp = GetClientWindowImage();
                
                if (temp != null)
                {
                    //windowImage.Dispose();
                    windowImage = temp;
                }
                else
                {
                    return;
                }
                if (windowImage.Width < 10 || windowImage.Height < 10) return;

                if (Preferences.hideLeagueBorders == false)
                {
                    //adjust the window position and show the UI
                    WIN32_API.RECT leagueLoc = new WIN32_API.RECT();
                    WIN32_API.RECT clientDim = new WIN32_API.RECT();
                    WIN32_API.GetWindowRect(windowHandle, ref leagueLoc);
                    WIN32_API.GetClientRect(windowHandle, ref clientDim);

                    //the numbers used may depend on what kind of window the person is using
                    //probably gives wrong alignments on windows xp...
                    //45 = 3d width
                    //5 = border width
                    //31 = titlebar height
                    this.Left = leagueLoc.x + (int)WIN32_API.GetSystemMetrics((IntPtr)45) + (int)WIN32_API.GetSystemMetrics((IntPtr)5);
                    this.Top = leagueLoc.y + (int)WIN32_API.GetSystemMetrics((IntPtr)45) + (int)WIN32_API.GetSystemMetrics((IntPtr)5) + (int)WIN32_API.GetSystemMetrics((IntPtr)31);
                    this.Width = clientDim.width;
                    this.Height = clientDim.height;
                    mainCanvas.Width = this.Width;
                    mainCanvas.Height = this.Height;
                }

                //TODO: Add image processing here
                leagueInfo.update();
                scriptControl.raiseEvent("processingFinished", "");
                this.procTimeLabel.Content = "Procesing Time:" + (int)(Math.Round((DateTime.Now - start).TotalMilliseconds)) + "ms";
               
            }
            else
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        Bitmap rawWindowImage;
        IntPtr rawWindowImageBits;

        public Bitmap GetClientWindowImage()
        {
            
            IntPtr hDC = WIN32_API.GetDC(windowHandle);
            IntPtr hMemDC = WIN32_API.CreateCompatibleDC(hDC);
                
            if (hDC == IntPtr.Zero || hMemDC == IntPtr.Zero) return null;

            WIN32_API.RECT windowSize = new WIN32_API.RECT();
            WIN32_API.GetClientRect(windowHandle, ref windowSize);

            if ((int)windowSize.width <= 10 || (int)windowSize.height <= 10) return null;

            if (windowBitmapHandle == IntPtr.Zero || (int)windowSize.width != windowImage.Width || (int)windowSize.height != windowImage.Height)
            {

                Console.WriteLine("got a new bitmap handle");
                scriptControl.log("New Window Resolution:" + (int)windowSize.width + " x " + (int)windowSize.height);
                if (Preferences.hideLeagueBorders)
                {
                    //adjust the window position and show the UI
                    WIN32_API.SetWindowLong(windowHandle, WIN32_API.GWL_STYLE,
                        WIN32_API.GetWindowLong(windowHandle, WIN32_API.GWL_STYLE) & ~WIN32_API.WS_CAPTION);
                    int XPadding = (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - windowSize.width) / 2;
                    int YPadding = (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - windowSize.height) / 2;
                    WIN32_API.SetWindowPos(windowHandle, 0, XPadding, YPadding, windowSize.width, windowSize.height, 0);


                    this.Left = XPadding;
                    this.Top = YPadding;
                    this.Width = windowSize.width;
                    this.Height = windowSize.height;
                    mainCanvas.Width = this.Width;
                    mainCanvas.Height = this.Height;
                }


                //init the UI rectangle locations and notify scripts
                LeagueUI.init(windowSize.width, windowSize.height);
                //windowBitmapHandle = WIN32_API.CreateCompatibleBitmap(hDC, (IntPtr)windowSize.width, (IntPtr)windowSize.height);

                WIN32_API.BITMAPINFO bmi = new WIN32_API.BITMAPINFO();
                bmi.biSize = 40;  // the size of the BITMAPHEADERINFO struct
                bmi.biWidth = windowSize.width;
                bmi.biHeight = windowSize.height;
                bmi.biPlanes = 1; // "planes" are confusing. We always use just 1. Read MSDN for more info.
                bmi.biBitCount = 24; // ie. 1bpp or 8bpp
                bmi.biCompression = WIN32_API.BI_RGB; // ie. the pixels in our RGBQUAD table are stored as RGBs, not palette indexes
                bmi.biSizeImage = 0;
                bmi.biXPelsPerMeter = 1000000; // not really important
                bmi.biYPelsPerMeter = 1000000; // not really important
                bmi.biClrUsed = 0;
                bmi.biClrImportant = 0;

                rawWindowImage = new Bitmap(windowSize.width, windowSize.height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                windowBitmapHandle = WIN32_API.CreateDIBSection(hDC, ref bmi, WIN32_API.DIB_RGB_COLORS, out rawWindowImageBits, IntPtr.Zero, 0);
                
            }
             
            if (windowBitmapHandle != IntPtr.Zero)
            {
                IntPtr hOld = (IntPtr)WIN32_API.SelectObject(hMemDC, windowBitmapHandle);
                if(!leagueInfo.outOfLoadScreen)
                    WIN32_API.BitBlt(hMemDC, 0, 0, windowSize.width, windowSize.height, hDC, 0, 0, WIN32_API.SRCCOPY);
                else
                    WIN32_API.BitBlt(hMemDC, 0, 3 * windowSize.height / 4, windowSize.width/4, windowSize.height / 4, hDC, 0, 3 * windowSize.height / 4, WIN32_API.SRCCOPY);
                WIN32_API.SelectObject(hMemDC, hOld);
                WIN32_API.DeleteDC(hMemDC);
                WIN32_API.ReleaseDC(windowHandle, hDC);

                var bInfo = rawWindowImage.LockBits(new System.Drawing.Rectangle(0, 0, rawWindowImage.Width, rawWindowImage.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
               
                //copy the data from the windows bitmap to a c# bitmap...would be nice to not have to do this
                unsafe
                {
                    int* s = (int*)bInfo.Scan0;
                    int intWidth = bInfo.Stride / 4;
                    int len = rawWindowImage.Height * intWidth;
                    int* screenImg = (int*)rawWindowImageBits + len;
                    int* t;


                    int yCount = rawWindowImage.Height;
                    int lineLen = intWidth;
                    int bitSkip = 0;
                    if (leagueInfo.outOfLoadScreen)
                    {
                        int startY = 3 * windowSize.height / 4;
                        yCount = rawWindowImage.Height / 4;
                        s += startY * intWidth;
                        screenImg -= startY * intWidth;
                        lineLen /= 4;
                        bitSkip = intWidth - lineLen;
                    }

                    for (int y = 0; y < yCount; y++)
                    {
                        screenImg -= intWidth;
                        t = screenImg;
                        for (int x = 0; x < lineLen; x++)
                        {
                            *(s++) = *(t++);
                        }
                        s += bitSkip;
                    }
                   
                } 
                rawWindowImage.UnlockBits(bInfo);

                //Bitmap bm = System.Drawing.Image.FromHbitmap(windowBitmapHandle);

                return rawWindowImage;
            }
            WIN32_API.DeleteDC(hMemDC);
            WIN32_API.ReleaseDC(windowHandle, hDC);
            return null;
        }

        public bool hasRed(Rect r)
        {
            int red = 0, green = 0, blue = 0;
            Color temp = new Color();

            for (int i = 0; i < r.Width; i++)
            {
                for (int j = 0; j < r.Height; j++)
                {
                    var c = windowImage.GetPixel((int)(r.X + i), (int)(r.Y - j));
                    temp.A = c.A;
                    temp.R = c.R;
                    temp.G = c.G;
                    temp.B = c.B;
                    // Console.WriteLine(temp.R);
                    if (temp.R > 200 && temp.B > 200 && temp.G > 200) continue;
                    red += temp.R;
                    green += temp.G;
                    blue += temp.B;
                }
            }

            int total = red + green + blue;

            double percent = (double)red / (double)total;

            if (red > (blue + 2500) && red > (green + 2500)) return true;
            return false;
        }
    }
}
