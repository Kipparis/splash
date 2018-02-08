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
using AxWMPLib;

namespace backMusic {
    public enum mainState {
        idle,
        awake
    }

    public partial class Form1 : Form {
        [DllImport("winmm.dll")]
        static extern Int32 mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

        private const int MM_MCINOTIFY = 0x03b9;
        private const int MCI_NOTIFY_SUCCESS = 0x01;
        private const int MCI_NOTIFY_SUPERSEDED = 0x02;
        private const int MCI_NOTIFY_ABORTED = 0x04;
        private const int MCI_NOTIFY_FAILURE = 0x08;

        public static string path = @"C:\sht\music";

        System.Media.SoundPlayer pew;
        System.Media.SoundPlayer pupa;
        System.Media.SoundPlayer za;
        System.Media.SoundPlayer lupa;
        System.Media.SoundPlayer zhivi;

        ExSoundPlayer mPlayer;

        NotifyIcon trayIcon;
        PictureBox pb1;
        PictureBox pb2;
        
        Form firstScreen;
        Form secondScreen;

        AxWindowsMediaPlayer player;

        mainState state;

        protected override void WndProc(ref Message m) {
            if (m.Msg == MM_MCINOTIFY) {
                switch (m.WParam.ToInt32()) {
                    case MCI_NOTIFY_SUCCESS:
                        // success handling
                        //MessageBox.Show("success handling");
                        mPlayer.NextSong();
                        PlaySong();
                        break;
                    case MCI_NOTIFY_SUPERSEDED:
                        // superseded handling
                        MessageBox.Show("Superseded handling");
                        break;
                    case MCI_NOTIFY_ABORTED:
                        // abort handling
                        MessageBox.Show("Abort handling");
                        break;
                    case MCI_NOTIFY_FAILURE:
                        // failure! handling
                        MessageBox.Show("Failure! Handling");
                        break;
                    default:
                        // haha
                        MessageBox.Show("Haha");
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

            trayIcon = new NotifyIcon() {
                Text = "My app",
                Icon = new System.Drawing.Icon("face.ico"),
                Visible = true
            };

            firstScreen.Controls.Add(pb1);
            secondScreen.Controls.Add(pb2);

            // Добавление низкоуровневых хуков
            HookManager.MouseClick += HookManager_MouseClick;
            HookManager.KeyPress += HookManager_PressedKey;
            HookManager.MouseMove += HookManager_MouseMove;

            SetUpScreens();
        }

        private void HookManager_PressedKey( object sender, KeyPressEventArgs e) {
            if (state == mainState.awake) return; // Если комп в рабочем состоянии, то мы даже не проверяем клаву
            if (e.KeyChar == 'o') {
                //mPlayer.Play("C:\\sht\\music\\soul.mp3");
                using (OpenFileDialog dlgOpen = new OpenFileDialog()) {
                    dlgOpen.Filter = "Mp3 File|*.mp3";
                    dlgOpen.InitialDirectory = @"C:\sht\music\";

                    if (dlgOpen.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                        //mPlayer = new ExSoundPlayer(dlgOpen.FileName);
                        //mPlayer.repeat = true;
                        //mPlayer = new ExSoundPlayer(dlgOpen.FileName);
                        //mPlayer.Play(dlgOpen.FileName);
                    }
                }
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
            if (e.KeyChar == 'd') {
                mPlayer.NextSong();
                //MessageBox.Show(mPlayer.currentPos.ToString());
            } else
             if (e.KeyChar == 'r') {
                mPlayer.ChooseRandom();
                PlaySong();
            } else
            if (e.KeyChar == 'f') {
                Idle();
            } else 
            if (e.KeyChar == 'g') {
                Awake();
            } 
        }

        // Когда нажата кнопка мыши
        private void HookManager_MouseClick(object sender, MouseEventArgs e) {
            pew.Play();
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
            ResetTimer();
            if (state == mainState.awake) {
                return;
            }// Значит сейчас фаза сна
            Awake();
        }

        void SetUpScreens() {
            /////////Для первого монитора\\\\\\\\\\\\
            firstScreen.FormBorderStyle = FormBorderStyle.None;
            firstScreen.WindowState = FormWindowState.Maximized;
            firstScreen.TopMost = true;
            // Увеличение размера, добавление картинки
            pb1.Size = firstScreen.Size;
            
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
            //firstScreen.Visible = false;

            //firstScreen.FormBorderStyle = FormBorderStyle.Sizable;
            //firstScreen.WindowState = FormWindowState.Normal;
            //firstScreen.TopMost = false;

            //secondScreen.FormBorderStyle = FormBorderStyle.Sizable;
            //secondScreen.WindowState = FormWindowState.Normal;
            //secondScreen.TopMost = false;

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
            mciSendString(mPlayer.ReturnPlay(), null, 0, this.Handle);
        }
    }
}
