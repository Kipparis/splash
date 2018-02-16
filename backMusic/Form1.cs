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


// Сделать открываемую в трее менюшку, для выбора корневой директории
// для класса ExSoundPlayer
namespace backMusic {
    public enum mainState {
        idle,
        awake,
        choose
    }

    public partial class Form1 : Form {
        [DllImport("winmm.dll")]
        static extern Int32 mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

        private const int MM_MCINOTIFY = 0x03b9;
        private const int MCI_NOTIFY_SUCCESS = 0x01;
        private const int MCI_NOTIFY_SUPERSEDED = 0x02;
        private const int MCI_NOTIFY_ABORTED = 0x04;
        private const int MCI_NOTIFY_FAILURE = 0x08;

        public static string path = @"C:\sht\music\";

        public Label songName;
        public Label tips;
        public Label currentSongPosition;
        public Label currentSongLength;

        public string tipsS;

        System.Media.SoundPlayer pew;
        System.Media.SoundPlayer pupa;
        System.Media.SoundPlayer za;
        System.Media.SoundPlayer lupa;
        System.Media.SoundPlayer zhivi;

        ExSoundPlayer mPlayer;

        PictureBox pb1;
        PictureBox pb2;
        PictureBox musicBar;
        
        static public Form firstScreen;
        static public Form secondScreen;

        mainState state;

        protected override void WndProc(ref Message m) {
            if (m.Msg == MM_MCINOTIFY) {
                switch (m.WParam.ToInt32()) {
                    case MCI_NOTIFY_SUCCESS:
                        // success handling
                        //MessageBox.Show("success handling");
                        // Играет следующую песню, в зависимости от внутреннего значения
                        // state в классе
                        mPlayer.NextSong();
                        PlaySong();
                        break;
                    case MCI_NOTIFY_SUPERSEDED:
                        // superseded handling
                        //MessageBox.Show("Superseded handling");
                        break;
                    case MCI_NOTIFY_ABORTED:
                        // abort handling
                        //MessageBox.Show("Abort handling");
                        break;
                    case MCI_NOTIFY_FAILURE:
                        // failure! handling
                        //MessageBox.Show("Failure! Handling");
                        break;
                    default:
                        // haha
                        //MessageBox.Show("Haha");
                        break;
                }
            }
            base.WndProc(ref m);
        }

        public Form1() {
            InitializeComponent();
            firstScreen = new Form() {
                Text = "First Screen",
                BackColor = Color.FromArgb(255, 232, 232)
            };
            firstScreen.Show();
            secondScreen = new Form() {
                Text = "Second Screen",
                BackColor = Color.FromArgb(255, 232, 232)
            };
            secondScreen.Show();

            firstScreen.Location = new Point(0, 0);
            secondScreen.Location = new Point(1913, 0);

            BeginSession();
            state = mainState.awake;

            //pew.Play();

        }

        void BeginSession() {
            timer.Interval = 10000;
            timer.Start();

            frameTimer.Interval = 10;
            frameTimer.Start();

            mPlayer = new ExSoundPlayer();

            pew = new System.Media.SoundPlayer() {
                SoundLocation = "piu.wav"
            };

            zhivi = new System.Media.SoundPlayer() {
                SoundLocation = "zhivi.wav"
            };

            pupa = new System.Media.SoundPlayer() {
                SoundLocation = "pupa.wav"
            };

            lupa = new System.Media.SoundPlayer() {
                SoundLocation = "lupa.wav"
            };

            za = new System.Media.SoundPlayer() {
                SoundLocation = "za.wav"
            };

            pb1 = new PictureBox() {
                Location = new Point(0,0),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(300, 300),
                ImageLocation = "1.gif",
                Visible = false
            };

            pb2 = new PictureBox() {
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(300, 300),
                ImageLocation = "4.png",
                Visible = false
            };

            musicBar = new PictureBox() {
                Location = new Point(0, 0),
                ImageLocation = "bar.png",
                Visible = false,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            songName = new Label() {
                Text = "None",
                TextAlign = ContentAlignment.TopRight,
                Height = 40,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(100, 0, 0, 0),
                ForeColor = Color.White,
                Font = new Font("Arial", 24, FontStyle.Bold),
                AutoSize = true
            };

            tips = new Label() {
                Text = "O - first song / next song \nS - stop \nR - random \nT - repeat \nY - play in order ",
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 40,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(100, 0, 0, 0),
                ForeColor = Color.White,
                Font = new Font("Arial", 18, FontStyle.Bold),
                AutoSize = true
            };
            currentSongPosition = new Label() {
                Text = "0",
                Height = 40,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(100, 0, 0, 0),
                ForeColor = Color.White,
                Font = new Font("Arial", 18, FontStyle.Bold),
                AutoSize = true
            };
            currentSongLength = new Label() {
                Text = "0",
                Height = 40,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(100, 0, 0, 0),
                ForeColor = Color.White,
                Font = new Font("Arial", 18, FontStyle.Bold),
                AutoSize = true
            };
            pb1.Controls.Add(songName);
            pb1.Controls.Add(tips);
            pb1.Controls.Add(currentSongPosition);
            pb1.Controls.Add(currentSongLength);

            pb1.Controls.Add(musicBar);

            firstScreen.Controls.Add(pb1);
            secondScreen.Controls.Add(pb2);

            // Добавление низкоуровневых хуков
            HookManager.MouseClick += HookManager_MouseClick;
            HookManager.KeyPress += HookManager_PressedKey;
            HookManager.MouseMove += HookManager_MouseMove;

            // Делаем всё всё с окнами, а потом просто скрываем
            SetUpScreens();
        }

        private void HookManager_PressedKey( object sender, KeyPressEventArgs e) {
            if (state != mainState.idle) return; // Если комп в рабочем состоянии, то мы даже не проверяем клаву
            if (e.KeyChar == 'o') {
                mPlayer.NextSong();
                PlaySong();
            } else
            if (e.KeyChar == 'q') {
                Awake();
            } else
            if (e.KeyChar == 'p') {
                if (mPlayer != null) {
                    mciSendString(mPlayer.ReturnPlay(), null, 0, this.Handle);
                }
            } else
            if (e.KeyChar == 's') {
                if (mPlayer != null) {
                    mPlayer.Stop();
                }
            } else
            if (e.KeyChar == 'l') {
                mciSendString(@"open C:\Users\Aleksey\Music\wedidit.mp3 type mpegvideo alias wedidit", null, 0, IntPtr.Zero);
                mciSendString(@"play wedidit", null, 0, IntPtr.Zero);
            } else
            if (e.KeyChar == 'w') {
                mciSendString(@"stop wedidit", null, 0, IntPtr.Zero);
            } else
            if (e.KeyChar == 'c') {
                ChooseDirectory();
                state = mainState.idle;
            } else
            if (e.KeyChar == 'd') {
                mPlayer.NextSong();
                //MessageBox.Show(mPlayer.currentPos.ToString());
            } else
             if (e.KeyChar == 'r') {
                mPlayer.state = playerState.random;
            } else
            if (e.KeyChar == 't') {
                mPlayer.state = playerState.repeat;
            } else
            if (e.KeyChar == 'y') {
                mPlayer.state = playerState.order;
            } else
            if (e.KeyChar == 'j') {
                GetSongPosition();
            } else
            if (e.KeyChar == 'm') {
                Help();
            }
        }

        private void Help() {
            // Показ расписания, планов, добавление во всякие плейлисты, их создание, удаление песен
            throw new NotImplementedException();
        }

        // Когда нажата кнопка мыши
        private void HookManager_MouseClick(object sender, MouseEventArgs e) {
            //pew.Play();
        }

        public void HideAll() {
            this.Hide();
            firstScreen.Hide();
            secondScreen.Hide();

            pb1.Visible = false;
            pb2.Visible = false;
        }

        // Когда передвигается мышь
        private void HookManager_MouseMove(object sender, MouseEventArgs e) {
            // Если таймер !включён!
            if (state == mainState.choose) return;
            if (timer.Enabled) ResetTimer();  // Обновляем таймер
            if (state == mainState.awake) return;   // Значит сейчас фаза сна
        }

        void SetUpScreens() {
            /////////Для первого монитора\\\\\\\\\\\\
            firstScreen.FormBorderStyle = FormBorderStyle.None;
            firstScreen.WindowState = FormWindowState.Maximized;
            firstScreen.TopMost = true;
            // Увеличение размера, добавление картинки
            pb1.Size = firstScreen.Size;

            musicBar.Location = new Point(0, 0);
            musicBar.Size = new Size(firstScreen.Width, 16);
            musicBar.Visible = true;

            //songName.Size = firstScreen.Size;
            songName.AutoSize = true;
            songName.SetBounds(firstScreen.Width - songName.Width, firstScreen.Height / 8, 
                songName.Width, songName.Height);

            tips.Location = new Point(0, firstScreen.Height * 26 / 30);

            currentSongPosition.Location = new Point(0, firstScreen.Height / 2);
            currentSongLength.Location = new Point(0, firstScreen.Height * 1 / 4);

            currentSongLength.Visible = false;
            currentSongPosition.Visible = false;

            if (Screen.AllScreens.Length < 2) return;
            // Картинка добавлена на раннем этапе
            /////////Для второго монитора            
            secondScreen.FormBorderStyle = FormBorderStyle.None;
            secondScreen.WindowState = FormWindowState.Maximized;
            secondScreen.TopMost = true;

            // Увеличение размера, добавление картинки
            pb2.Size = secondScreen.Size;
        }

        void ResetTimer() {
            timer.Stop();
            timer.Start();
        }
        void Idle() {   // Когда ничего не нажато долгое время
            firstScreen.Visible = true; // Включаем основную форму, чтоб показать картинку
            secondScreen.Visible = true; // Включаем основную форму, чтоб показать картинку
            pb1.Visible = true;
            pb2.Visible = true;

            state = mainState.idle;
        }

        void Awake() {
            state = mainState.awake;
            Cursor.Show();

            HideAll();
        }

        private void Pew(object sender, KeyEventArgs e) {
            pew.Play();
        }

        private void Pew(object sender, MouseEventArgs e) {
            pew.Play();
        }

        // Когда таймер срабатывает
        private void timer_Tick(object sender, EventArgs e) {
            Idle();
        }

        private void Form1_Load(object sender, EventArgs e) {
        }

        private void Form1_Shown(object sender, EventArgs e) {
            HideAll();
        }

        public void PlaySong() {
            songName.Text = mPlayer.name;
            songName.SetBounds(firstScreen.Width - songName.Width, firstScreen.Height / 8, 
                songName.Width, songName.Height);
            // Рассчитываем ширину в зависимости от имя
            //songName.Width = songName.Text.Split().Length * 25;
            mciSendString(mPlayer.ReturnPlay(), null, 0, this.Handle);
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            MessageBox.Show("Clicked trayIcon");
        }

        // Выбирает стандартную папку
        private void ChooseDirectory_Click(object sender, EventArgs e) {
            ChooseDirectory();
            state = mainState.idle;
        }

        public void ChooseDirectory() {
            state = mainState.choose;
            using (FolderBrowserDialog dlgOpen = new FolderBrowserDialog()) {
                //dlgOpen.Filter = "Mp3 File|*.mp3";
                dlgOpen.SelectedPath = @"C:\sht\";

                if (dlgOpen.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    Form1.path = dlgOpen.SelectedPath;
                }
            }
        }

        // Если таймер уже запущен -> выключает
        // Таймер выключен -> включает
        private void StopWatch_Click(object sender, EventArgs e) {
            if (timer.Enabled)  { timer.Enabled = false; } 
            else                { timer.Enabled = true; }
        }

        public void GetSongPosition() {
            currentSongPosition.Text = mPlayer.GetSongPosition().ToString();
        }

        public void GetSongLength() {
            currentSongLength.Text = mPlayer.GetSongLength().ToString();
        }

        public int songPosition {
            get { return (int.Parse(currentSongPosition.Text)); }
            set { currentSongPosition.Text = value.ToString(); }
        }

        private void frameTimer_Tick(object sender, EventArgs e) {
            if (mPlayer.subName == null) return;
            GetSongLength();
            GetSongPosition();
            if (mPlayer.songLength == 0 || mPlayer.songPosition == 0) return;
            // Сначала найти отношение в float, потом  умножить его и перевести в int
            int newWidth = Convert.ToInt32((double)firstScreen.Width * (double)mPlayer.songPosition / (double)mPlayer.songLength);
            musicBar.Size = new Size(newWidth, 12);
        }
    }
}
