using System;
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

        public Bitmap windowImage = new Bitmap(1, 1);
        public LeagueInfo leagueInfo;
        public ScriptControl scriptControl;
        public UIComponents uicomponents;
        public KeyboardManager keyboardManager;

        DispatcherTimer processingTimer, UILogicTimer;

        public MainWindow()
        {
            InitializeComponent();
            
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
            leagueInfo = new LeagueInfo(this);
            scriptControl = new ScriptControl(this);
            uicomponents = new UIComponents(this);
            keyboardManager = new KeyboardManager();
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
            processingTimer.Interval = TimeSpan.FromMilliseconds(500);
            processingTimer.Tick += new EventHandler(processingTimer_Tick);
            processingTimer.Start();

            UILogicTimer = new DispatcherTimer();
            UILogicTimer.Interval = TimeSpan.FromMilliseconds(100);
            UILogicTimer.Tick += new EventHandler(UILogicTimer_Tick);
            UILogicTimer.Start();
        }

        void UILogicTimer_Tick(object sender, EventArgs e)
        {
            //do logic such as updating timers or UI elements
            DisplayTimers.updateTimers();
            keyboardManager.update();
            scriptControl.update();
            if (keyboardManager.wasKeyPressed((int)Key.D9))
            {
                keyboardManager.sendKeyDown(KeyboardManager.DIK_LCONTROL);
                keyboardManager.sendKeyPress(KeyboardManager.DIK_W);
                keyboardManager.sendKeyUp(KeyboardManager.DIK_LCONTROL);
            }
            else if (keyboardManager.wasKeyPressed((int)Key.Up))
            {
                DisplayTimers.createTimer("testing", new TimeSpan(0, 0, 10));
            }
        }

        void processingTimer_Tick(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder(256);
            IntPtr tempHandle = WIN32_API.GetForegroundWindow();
            WIN32_API.GetWindowText(tempHandle, sb, (IntPtr)sb.MaxCapacity);

            if (sb.ToString().ToLower().Contains("league of legends (tm) client"))
            {
                DateTime start = DateTime.Now;
                //grab the current screen image from league of legends
                windowHandle = tempHandle;
                Bitmap temp = windowImage;
                windowImage = GetClientWindowImage();
                temp.Dispose();

                //TODO: Add image processing here
                leagueInfo.update();

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
                
                this.procTimeLabel.Content = "Procesing Time:" + (int)(Math.Round((DateTime.Now - start).TotalMilliseconds)) + "ms";
                scriptControl.raiseEvent("processingFinished", "");
                //this.Show();
                //WIN32_API.SetFocus(windowHandle);
            }
            else
            {
                //this.Hide();
                Width = 0;
                Height = 0;
            }
        }

        public Bitmap GetClientWindowImage()
        {
            IntPtr hDC = WIN32_API.GetDC(windowHandle);
            IntPtr hMemDC = WIN32_API.CreateCompatibleDC(hDC);

            WIN32_API.RECT windowSize = new WIN32_API.RECT();
            WIN32_API.GetClientRect(windowHandle, ref windowSize);

            if (windowBitmapHandle == IntPtr.Zero || (int)windowSize.width != windowImage.Width || (int)windowSize.height != windowImage.Height)
            {
                Console.WriteLine("got a new bitmap handle");
                LeagueUI.init(windowSize.width, windowSize.height);
                windowBitmapHandle = WIN32_API.CreateCompatibleBitmap(hDC, (IntPtr)windowSize.width, (IntPtr)windowSize.height);
            }


            if (windowBitmapHandle != IntPtr.Zero)
            {
                IntPtr hOld = (IntPtr)WIN32_API.SelectObject(hMemDC, windowBitmapHandle);
                WIN32_API.BitBlt(hMemDC, 0, 0, windowSize.width, windowSize.height, hDC, 0, 0, WIN32_API.SRCCOPY);
                WIN32_API.SelectObject(hMemDC, hOld);
                WIN32_API.DeleteDC(hMemDC);
                WIN32_API.ReleaseDC(windowHandle, hDC);
                return System.Drawing.Image.FromHbitmap(windowBitmapHandle);
            }
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
