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

        StringBuilder sb = new StringBuilder(128);
        public ulong songPosition;
        public ulong songLength;

        public bool IsPlaying() {
            mciSendString("status " + subName + " mode", sb, 128, IntPtr.Zero);
            if (sb.Length == 7 &&
                      sb.ToString().Substring(0, 7) == "playing")
                return true;
            else
                return false;
        }

        public ulong GetSongPosition() {
            if (subName == null) return(0);
            mciSendString("status " + subName + " position", sb, 128, IntPtr.Zero);
            if (sb == null) return (0);
            songPosition = Convert.ToUInt64(sb.ToString());
            return (songPosition);
        }

        public ulong GetSongLength() {
            if (subName == null) return (0);
            mciSendString("status " + subName + " length", sb, 128, IntPtr.Zero);
            if (sb == null) return (0);
            songLength = Convert.ToUInt64(sb.ToString());
            return (songLength);
        }

        Random randomNumber = new Random();

        public string path;
        public string pathFolder;
        public string[] pathes;
        public string name; // 
        public string subName;
        public string fileName;
        public string ext;

        // В зависимости от значения будет проигрывать 
        // следующую песню
        public playerState state = playerState.order;

        // Формат для того, что бы было легче ( впринципе незачем )
        const string FORMAT = "open {0} type mpegvideo alias {1}";

        // Для того чтобы работал, нужно внешней функцией присвоить значение коревой папки
        // Присваивает значения класса, что бы можно было вызвать PlaySong() из главной формы
        public void NextSong() {
            if (subName != null) mciSendString("close " + subName, null, 0, IntPtr.Zero);
            // Находим все пути к песням и корневую папку
            FindPath();
            switch (state) {
                case playerState.repeat:
                    // Открываем этот же путь
                    break;
                case playerState.random:
                    // Выбираем рандом
                    ChooseRandom();
                    break;
                case playerState.order:
                    // След. песня
                    if (path == null) { path = pathes[0]; } else {
                        int id = Array.IndexOf(pathes, path);
                        path = pathes[(id + 1) % pathes.Length];
                    }
                    break;
                default:
                    break;
            }
            FindName(); // Находим контрольные именна для выбранной песни
        }

        public ExSoundPlayer() {

        }

        public void Play(string path) {
            FindName(path);
            mciSendString("open \"" + path + "\" type mpegvideo alias " + subName, null, 0, IntPtr.Zero);
            mciSendString("play " + name.Split()[0], null, 0, IntPtr.Zero);
        }

        public string ReturnPlay() {
            //currentSong = name;
            mciSendString("open \"" + path + "\" type mpegvideo alias " + subName, null, 0, IntPtr.Zero);
            string command = "play " + subName + " notify";
            return (command);
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
                    pathes = Directory.GetFiles(pathFolder, "*.mp3");
                }
            }
        }
    }
}
