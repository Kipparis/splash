using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.UserActivityMonitor;
using System.Runtime.InteropServices;
using System.IO;


namespace backMusic {
    class ExSoundPlayer : IDisposable {
        [DllImport("winmm.dll")]
        private static extern int mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);
        [DllImport("winmm.dll")]
        public static extern int mciGetErrorString(int errCode, StringBuilder errMsg, int buflen);

        Random randomNumber = new Random();

        public string path;
        public string pathFolder;
        public string[] pathes;
        public string name; // 
        public string subName;
        public string fileName;
        public string ext;

        // Формат для того, что бы было легче ( впринципе незачем )
        const string FORMAT = "open {0} type mpegvideo alias {1}";

        public void NextSong() {
            mciSendString("close " + subName, null, 0, IntPtr.Zero);
            if (pathes == null) {
                // Если плейлист ещё не создан
                pathes = Directory.GetFiles(pathFolder);
            }
            int id = Array.IndexOf(pathes, path);
            path = pathes[(id + 1)%pathes.Length];
            FindName(); // Заного просчитываются все элементы класса
        }

        //mciSendString(@"open C:\Users\Aleksey\Music\wedidit.mp3 type mpegvideo alias wedidit", null, 0, IntPtr.Zero);
        //mciSendString(@"play wedidit", null, 0, IntPtr.Zero);

        public string returnString = "";
        
        public ExSoundPlayer(string path) { // Сразу определяем плейлист
            this.path = path;
            FindName();

            //mciSendString("open \"" + path + "\" type mpegvideo alias " + subName, null, 0, IntPtr.Zero);
            // Находим все мп3 в папке и добавляем в список
        }

        public ExSoundPlayer() { }

        public void Play() {
            //string command = "play " + subName + " notify";
            //mciSendString(command, null, 0, IntPtr.Zero);
        }

        public void Play(string path) {
            FindName(path);
            mciSendString("open \"" + path + "\" type mpegvideo alias " + subName, null, 0, IntPtr.Zero);
            mciSendString("play " + name.Split()[0], null, 0, IntPtr.Zero);
        }

        public string ReturnPlay() {
            if (path == null) {
                ChooseRandom();
            }
            mciSendString("open \"" + path + "\" type mpegvideo alias " + subName, null, 0, IntPtr.Zero);
            string command = "play " + subName + " notify";
            return(command);
        }

        public void FindName(string path) {
            name = path.Substring(path.LastIndexOf('\\') + 1).Split('.')[0];
            ext = path.Substring(path.LastIndexOf('\\') + 1).Split('.')[1];
            subName = name.Split()[0];
            if (pathFolder == null) {
                pathFolder = path.Substring(0, path.LastIndexOf('\\'));
            }
        }

        public void FindName() {
            FindName(this.path);
        }

        public void Stop() {
            string command = "stop " + subName;
            mciSendString(command, null, 0, IntPtr.Zero);
        }

        public void Dispose() {
            MessageBox.Show("Song ended");
        }

        public void ChooseRandom() {
            if (pathes == null) {
                // Если плейлист ещё не создан
                if (pathFolder == null) {
                    pathes = Directory.GetFiles(Form1.path);
                }
            }
            path = pathes[randomNumber.Next(0, pathes.Length)];
            FindName();
        }
    }
}
