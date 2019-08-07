namespace Xilium.CefGlue.Samples.WpfOsr
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    internal enum INPUT_TYPE : uint
    {
        INPUT_MOUSE = 0,
        INPUT_KEYBOARD = 1,
        INPUT_HARDWARE = 2
    }

    public partial class MainWindow : Window
    {
        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        private bool _isStopRecord = true;

        private int _pathStep = 0;
        private List<Point> _step1MousePathCollection = new List<Point>();


        private DispatcherTimer movementTimer = new DispatcherTimer();
        private DispatcherTimer step1Timer = new DispatcherTimer();
        private Point endPosition;
        private int mouseMoveSteps = 10;
        private int count;

        private int step = 0;

        [DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT
        {
            [FieldOffset(0)]
            public int type;

            [FieldOffset(4)]
            public KEYBDINPUT ki;

            [FieldOffset(4)]
            public MOUSEINPUT mi;

            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }

        public struct MOUSEINPUT
        {
            public int dx;

            public int dy;

            public int mouseData;

            public int dwFlags;

            public int time;

            public IntPtr dwExtraInfo;
        }

        public struct KEYBDINPUT
        {
            public short wVk;

            public short wScan;

            public int dwFlags;

            public int time;

            public IntPtr dwExtraInfo;
        }

        public struct HARDWAREINPUT
        {
            public int uMsg;

            public short wParamL;

            public short wParamH;
        }

        const uint MAPVK_VK_TO_VSC = 0x00;
        const uint MAPVK_VSC_TO_VK = 0x01;
        const uint MAPVK_VK_TO_CHAR = 0x02;
        const uint MAPVK_VSC_TO_VK_EX = 0x03;
        const uint MAPVK_VK_TO_VSC_EX = 0x04;

        [DllImport("user32.dll")]
        static extern uint MapVirtualKeyW(uint uCode, uint uMapType);


        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);


        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern short VkKeyScan(char ch);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern short GetKeyState(int nVirtKey);


        public MainWindow()
        {
            movementTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            movementTimer.Tick += OnMouseTimerTick;

            InitializeComponent();
        }


        private Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        private void OnMouseTimerTick(object sender, EventArgs e)
        {
            Point mousePosition = GetMousePosition();

            int stepx = (int)(endPosition.X - mousePosition.X) / count;
            int stepy = (int)(endPosition.Y - mousePosition.Y) / count;
            count--;
            if (count == 0)
            {
                movementTimer.Stop();
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                //OnMouseMoveEnd();
            }
            mouse_event(MOUSEEVENTF_MOVE, stepx, stepy, 0, 0);
        }

        private void MoveMouse()
        {
            ((Action)(() =>
            {
                foreach (var item in _step1MousePathCollection)
                {
                    double stepx = item.X;
                    double stepy = item.Y;
                    _pathStep++;
                    mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, (int)(stepx / 1920 * 65535), (int)(stepy / 1080 * 65535), 0, 0);

                    if (_pathStep == _step1MousePathCollection.Count - 1)
                    {
                        movementTimer.Stop();
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    }
                    Thread.Sleep(20);
                }
                
                SimulateInputString("yidong fangwu ");
                Thread.Sleep(100);
                SimulateInputString("xiangfang jingzhuangxiu ");
                OnInputSearchStringEnd();
            })).BeginInvoke(null, null);
        }

        protected override void OnClosed(EventArgs e)
        {
            browser.Dispose();
            base.OnClosed(e);
        }

        private void addressTextBox_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                browser.NavigateTo(addressTextBox.Text);
            }
        }

        private void OnInputSearchStringEnd()
        {
            step1Timer.Interval = new TimeSpan(0, 0, 4);
            step1Timer.Tick += ((object sender, EventArgs e) =>
            {
                step = 1;
                step1Timer.Stop();
                browser.getElementPositionByInnerText("移动房屋箱房精装修", (x, y) => {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        var sreenPoint = PointToScreen(new Point(x + 5, y + 25 + 5)); // 25是地址栏的高度
                        MoveMouseToPoint((int)sreenPoint.X, (int)sreenPoint.Y);
                    }), null);
                });
            });
            step1Timer.Start();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.I && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                browser.ShowDevTools();
            }
            if (e.Key == Key.Q && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                MoveMouse();
            }

            if (e.Key == Key.D1 && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                _step1MousePathCollection.Clear();
                _isStopRecord = false;
            }

            if (e.Key == Key.S && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                string path1String = JsonConvert.SerializeObject(_step1MousePathCollection);
                File.WriteAllText("1.txt", path1String);

                _isStopRecord = true;
            }
        }

        private void MoveMouseToPoint(int x, int y)
        {
            count = mouseMoveSteps;
            endPosition.X = x;
            endPosition.Y = y;

            movementTimer.Start();
        }

        public void SimulateInputString(string sText)
        {
            char[] cText = sText.ToCharArray();
            foreach (char c in cText)
            {
                INPUT[] input = new INPUT[2];
                if (c >= 0 && c < 256)//a-z A-Z
                {
                    short num = VkKeyScan(c);//获取虚拟键码值
                    if (num != -1)
                    {
                        bool shift = (num >> 8 & 1) != 0;//num >>8表示 高位字节上当状态，如果为1则按下Shift，否则没有按下Shift，即大写键CapsLk没有开启时，是否需要按下Shift。
                        if ((GetKeyState(20) & 1) != 0 && ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))//GetKeyState(20)获取CapsLk大写键状态
                        {
                            shift = !shift;
                        }
                        if (shift)
                        {
                            input[0].type = 1;//模拟键盘
                            input[0].ki.wVk = 16;//Shift键
                            input[0].ki.dwFlags = 0;//按下
                            SendInput(1u, input, Marshal.SizeOf((object)default(INPUT)));
                        }
                        
                        input[0].type = 1;
                        input[0].ki.wScan = (short)MapVirtualKeyW(c, MAPVK_VK_TO_VSC);
                        input[0].ki.wVk = (short)(num & 0xFF);
                        input[1].type = 1;
                        input[1].ki.wVk = (short)(num & 0xFF);
                        input[1].ki.dwFlags = 2;
                        SendInput(2u, input, Marshal.SizeOf((object)default(INPUT)));
                        if (shift)
                        {
                            input[0].type = 1;
                            input[0].ki.wVk = 16;
                            input[0].ki.dwFlags = 2;//抬起
                            SendInput(1u, input, Marshal.SizeOf((object)default(INPUT)));
                        }
                        continue;
                    }
                }
                input[0].type = 1;
                input[0].ki.wVk = 0;//dwFlags 为KEYEVENTF_UNICODE 即4时，wVk必须为0
                input[0].ki.wScan = (short)c;
                input[0].ki.dwFlags = 4;//输入UNICODE字符
                input[0].ki.time = 0;
                input[0].ki.dwExtraInfo = IntPtr.Zero;
                input[1].type = 1;
                input[1].ki.wVk = 0;
                input[1].ki.wScan = (short)c;
                input[1].ki.dwFlags = 6;
                input[1].ki.time = 0;
                input[1].ki.dwExtraInfo = IntPtr.Zero;
                SendInput(2u, input, Marshal.SizeOf((object)default(INPUT)));
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isStopRecord)
            {
                _step1MousePathCollection.Add(PointToScreen(e.GetPosition(this)));
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            string tmpPathContent = File.ReadAllText("1.txt");
            _step1MousePathCollection = JsonConvert.DeserializeObject<List<Point>>(tmpPathContent);
        }
    }
}
