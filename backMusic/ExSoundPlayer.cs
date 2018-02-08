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
    public enum playerState {
        repeat,
        random,
        order
    }

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

        public playerState state = playerState.order;  // В зависимости от значения будет проигрывать 
        // следующую песню

        // Формат для того, что бы было легче ( впринципе незачем )
        const string FORMAT = "open {0} type mpegvideo alias {1}";

        // Для того чтобы работал, нужно внешней функцией присвоить значение коревой папки
        // Присваивает значения класса, что бы можно было вызвать PlaySong() из главной формы
        public void NextSong() {
            if(subName != null) mciSendString("close " + subName, null, 0, IntPtr.Zero);
            // Находим все пути к песням и корневую папку
            FindPath(); 
            switch (state) {
                case playerState.repeat:
                    // Ничего не делаем, т.к. потом заново откроем этот же путь
                    break;
                case playerState.random:
                    // Присваивает значения пути и т.д. к рандомной песне
                    ChooseRandom();
                    break;
                case playerState.order:
                    // Находим следующую песню в списке
                    if (path == null) {
                        path = pathes[0];
                    } else {
                        int id = Array.IndexOf(pathes, path);
                        path = pathes[(id + 1) % pathes.Length];
                    }
                    break;
                default:
                    break;
            }
            FindName(); // Находим контрольные именна для выбранной песни
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

        // Выбирает и сразу присваивает все значения для рандомной песни
        public void ChooseRandom() {
            path = pathes[randomNumber.Next(0, pathes.Length)];
        }

        public void FindPath() {
            if (pathFolder == null) {
                pathFolder = Form1.path;
                // Если плейлист ещё не создан
                if (pathes == null) {
                    pathes = Directory.GetFiles(pathFolder);
                }
            }
        }
    }
}
