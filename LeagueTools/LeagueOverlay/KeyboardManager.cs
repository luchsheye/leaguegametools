using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace LeagueOverlay
{
    public class KeyboardManager
    {
        //these key states use the regular key codes found in Keys.
        Dictionary<int,int> currentKeyState = new Dictionary<int,int>();
        Dictionary<int, int> previousKeyState = new Dictionary<int,int>();

        bool mouseLeft,previousMouseLeft;
        MainWindow parent;

        public KeyboardManager(MainWindow mw)
        {
            parent = mw;
        }
        public void update()
        {
            foreach(int k in currentKeyState.Keys)
            {
                previousKeyState[k] = currentKeyState[k];  
            }
            foreach (int k in previousKeyState.Keys)
            {
                currentKeyState[k] = WIN32_API.GetAsyncKeyState(k);
                if (currentKeyState[k] < 0 && previousKeyState[k] >= 0)
                    parent.scriptControl.raiseEvent("keypress", k.ToString());
            }
            previousMouseLeft = mouseLeft;
            mouseLeft = ((int)System.Windows.Forms.Control.MouseButtons & 1048576) == 1048576;
            if (!mouseLeft && previousMouseLeft) parent.scriptControl.doMouseClick();
        }

        [AttrLuaFunc("WasKeyPressed")]
        public bool wasKeyPressed(int key)
        {

            if (!currentKeyState.ContainsKey(key))
            {
                currentKeyState[key] = WIN32_API.GetAsyncKeyState(key);
                previousKeyState[key] = currentKeyState[key];
            }
            return  currentKeyState[key] < 0 && previousKeyState[key] >= 0;
        }

        [AttrLuaFunc("IsKeyDown")]
        public bool isKeyDown(int key)
        {
            return WIN32_API.GetAsyncKeyState(key) < 0;
        }

        [AttrLuaFunc("SendAllyChatMessage")]
        public void sendAllyChatMessage(string Message)
        {
            bool wasControlDown = WIN32_API.GetAsyncKeyState((int)System.Windows.Forms.Keys.LControlKey) < 0;
            if (wasControlDown)
                sendKeyUp(DIK_LCONTROL);
            sendKeyPress(DIK_RETURN);
            Thread.Sleep(20);
            System.Windows.Forms.SendKeys.SendWait(Message);
            Thread.Sleep(20);
            sendKeyPress(DIK_RETURN);
            //if (wasControlDown)
             //   sendKeyDown(DIK_LCONTROL);
        }
        [AttrLuaFunc("SendChatMessage")]
        public void sendChatMessage(string Message)
        {
            sendKeyDown(DIK_LSHIFT);
            sendKeyPress(DIK_RETURN);
            sendKeyUp(DIK_LSHIFT);
            //System.Windows.Forms.SendKeys.SendWait(Message);
            sendKeyPress(DIK_RETURN);
        }

        //the send key functions should use the SCAN key codes found below...
        [AttrLuaFunc("SendKeyDown")]
        public void sendKeyDown(ushort k)
        {
            WIN32_API.INPUT[] InputData = new WIN32_API.INPUT[1];

            InputData[0].type = (IntPtr)1; //INPUT_KEYBOARD
            InputData[0].wScan = k;
            InputData[0].dwFlags = (IntPtr)WIN32_API.SendInputFlags.KEYEVENTF_SCANCODE;

            if (WIN32_API.SendInput(1, InputData, Marshal.SizeOf(InputData[0])) == 0)
            {
                System.Diagnostics.Debug.WriteLine("SendInput failed with code: " +
                Marshal.GetLastWin32Error().ToString());
            }
        }

        [AttrLuaFunc("SendKeyUp")]
        public void sendKeyUp(ushort k)
        {
            WIN32_API.INPUT[] InputData = new WIN32_API.INPUT[1];

            InputData[0].type = (IntPtr)1; //INPUT_KEYBOARD
            InputData[0].wScan = k;
            InputData[0].dwFlags =(IntPtr) ((uint)WIN32_API.SendInputFlags.KEYEVENTF_SCANCODE | (uint)WIN32_API.SendInputFlags.KEYEVENTF_KEYUP);

            if (WIN32_API.SendInput(1, InputData, Marshal.SizeOf(InputData[0])) == 0)
            {
                System.Diagnostics.Debug.WriteLine("SendInput failed with code: " +
                Marshal.GetLastWin32Error().ToString());
            }
        }

        [AttrLuaFunc("SendKeyPress")]
        public void sendKeyPress(ushort k)
        {
            WIN32_API.INPUT[] InputData = new WIN32_API.INPUT[2];

            InputData[0].type = (IntPtr)1; //INPUT_KEYBOARD
            InputData[0].wScan = k;
            InputData[0].dwFlags = (IntPtr)WIN32_API.SendInputFlags.KEYEVENTF_SCANCODE;
            
            InputData[1].type = (IntPtr)1; //INPUT_KEYBOARD
            InputData[1].wScan = k;
            InputData[1].dwFlags = (IntPtr)((uint)WIN32_API.SendInputFlags.KEYEVENTF_SCANCODE | (uint)WIN32_API.SendInputFlags.KEYEVENTF_KEYUP);

            if (WIN32_API.SendInput(2, InputData, Marshal.SizeOf(InputData[0])) == 0)
            {
                System.Diagnostics.Debug.WriteLine("SendInput failed with code: " +
                Marshal.GetLastWin32Error().ToString());
            }
        }

        //scan code constants for sending
        public const int DIK_ESCAPE          =0x01;
        public const int DIK_1               =0x02;
        public const int DIK_2               =0x03;
        public const int DIK_3               =0x04;
        public const int DIK_4               =0x05;
        public const int DIK_5               =0x06;
        public const int DIK_6               =0x07;
        public const int DIK_7               =0x08;
        public const int DIK_8               =0x09;
        public const int DIK_9               =0x0A;
        public const int DIK_0               =0x0B;
        public const int DIK_MINUS           =0x0C;   /* - on main keyboard */
        public const int DIK_EQUALS          =0x0D;
        public const int DIK_BACK            =0x0E;    /* backspace */
        public const int DIK_TAB             =0x0F;
        public const int DIK_Q               =0x10;
        public const int DIK_W               =0x11;
        public const int DIK_E               =0x12;
        public const int DIK_R               =0x13;
        public const int DIK_T               =0x14;
        public const int DIK_Y               =0x15;
        public const int DIK_U               =0x16;
        public const int DIK_I               =0x17;
        public const int DIK_O               =0x18;
        public const int DIK_P               =0x19;
        public const int DIK_LBRACKET        =0x1A;
        public const int DIK_RBRACKET        =0x1B;
        public const int DIK_RETURN          =0x1C;  /* Enter on main keyboard */
        public const int DIK_LCONTROL        =0x1D;
        public const int DIK_A               =0x1E;
        public const int DIK_S               =0x1F;
        public const int DIK_D               =0x20;
        public const int DIK_F               =0x21;
        public const int DIK_G               =0x22;
        public const int DIK_H               =0x23;
        public const int DIK_J               =0x24;
        public const int DIK_K               =0x25;
        public const int DIK_L               =0x26;
        public const int DIK_SEMICOLON       =0x27;
        public const int DIK_APOSTROPHE      =0x28;
        public const int DIK_GRAVE           =0x29 ;   /* accent grave */
        public const int DIK_LSHIFT          =0x2A;
        public const int DIK_BACKSLASH       =0x2B;
        public const int DIK_Z               =0x2C;
        public const int DIK_X               =0x2D;
        public const int DIK_C               =0x2E;
        public const int DIK_V               =0x2F;
        public const int DIK_B               =0x30;
        public const int DIK_N               =0x31;
        public const int DIK_M               =0x32;
        public const int DIK_COMMA           =0x33;
        public const int DIK_PERIOD          =0x34;   /* . on main keyboard */
        public const int DIK_SLASH           =0x35;  /* / on main keyboard */
        public const int DIK_RSHIFT          =0x36;
        public const int DIK_MULTIPLY        =0x37;    /* * on numeric keypad */
        public const int DIK_LMENU           =0x38;    /* left Alt */
        public const int DIK_SPACE           =0x39;
        public const int DIK_CAPITAL         =0x3A;
        public const int DIK_F1              =0x3B;
        public const int DIK_F2              =0x3C;
        public const int DIK_F3              =0x3D;
        public const int DIK_F4              =0x3E;
        public const int DIK_F5              =0x3F;
        public const int DIK_F6              =0x40;
        public const int DIK_F7              =0x41;
        public const int DIK_F8              =0x42;
        public const int DIK_F9              =0x43;
        public const int DIK_F10             =0x44;
        public const int DIK_NUMLOCK         =0x45;
        public const int DIK_SCROLL          =0x46;   /* Scroll Lock */
        public const int DIK_NUMPAD7         =0x47;
        public const int DIK_NUMPAD8         =0x48;
        public const int DIK_NUMPAD9         =0x49;
        public const int DIK_SUBTRACT        =0x4A;   /* - on numeric keypad */
        public const int DIK_NUMPAD4         =0x4B;
        public const int DIK_NUMPAD5         =0x4C;
        public const int DIK_NUMPAD6         =0x4D;
        public const int DIK_ADD             =0x4E;    /* + on numeric keypad */
        public const int DIK_NUMPAD1         =0x4F;
        public const int DIK_NUMPAD2         =0x50;
        public const int DIK_NUMPAD3         =0x51;
        public const int DIK_NUMPAD0         =0x52;
        public const int DIK_DECIMAL         =0x53;    /* . on numeric keypad */
        public const int DIK_F11             =0x57;
        public const int DIK_F12             =0x58;
    }
}
