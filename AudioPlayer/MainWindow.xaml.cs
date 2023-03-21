using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace AudioPlayer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            btnPlay.IsEnabled = false;
            btnNext.IsEnabled = false;
            btnBack.IsEnabled = false;
            btnRepeat.IsEnabled = false;
            btnRandom.IsEnabled = false;
        }
        private DispatcherTimer timer;
        private string path;
        private bool isPlaying = false;
        private bool isRepeat = false;
        private bool isRandom = false;
        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += Timer_Tick;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            AudioSlider.Value = MediaEl.Position.TotalSeconds;
            MediaEl.Position = TimeSpan.FromSeconds(AudioSlider.Value);
        }
        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                btnPlay.IsEnabled = true;
                btnNext.IsEnabled = true;
                btnBack.IsEnabled = true;
                btnRepeat.IsEnabled = true;
                btnRandom.IsEnabled = true;
                SongList.Items.Clear();
                path = Path.GetFullPath(dialog.FileName);
                string[] files = Directory.GetFiles(dialog.FileName);
                foreach (string file in files)
                {
                    string trackName = Path.GetFileName(file);
                    string ext = Path.GetExtension(file);
                    if (ext == ".mp3" || ext == ".wav" || ext == ".ogg" || ext == ".flac" || ext == ".m4a")
                    {
                        SongList.Items.Add(trackName);
                    }
                }
                MediaEl.Source = new Uri(files.First());
                isPlaying = true;
                SongList.SelectedIndex = 0;
                UpdateTime();
                MediaEl.Play();
                timer.Start();
            }
        }
        private void MediaEl_MediaOpened(object sender, RoutedEventArgs e)
        {
            AudioSlider.Maximum = MediaEl.NaturalDuration.TimeSpan.TotalSeconds;
        }
        private void AudioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaEl.Position = TimeSpan.FromSeconds(e.NewValue);
        }
        private void NextBackSong()
        {
            string trackName = SongList.SelectedItem.ToString();
            string trackPath = path + "\\" + trackName;
            MediaEl.Source = new Uri(trackPath);
            MediaEl.Play();
            timer.Start();
            isPlaying = true;
        }
        private void PlaySong()
        {
            if (SongList.SelectedItem != null)
            {
                if (isPlaying)
                {
                    MediaEl.Pause();
                    timer.Stop();
                    isPlaying = false;
                }
                else
                {
                    MediaEl.Play();
                    timer.Start();
                    isPlaying = true;
                }
            }
        }
        private void RandomTrack()
        {
            Random rand = new Random();
            int track = rand.Next(SongList.Items.Count);
            SongList.SelectedIndex = track;
        }
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            PlaySong();
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (SongList.Items.Count > 0)
            {
                if (SongList.SelectedIndex == 0)
                {
                    SongList.SelectedIndex = SongList.Items.Count - 1;
                }
                else
                {
                    SongList.SelectedIndex--;
                }
                NextBackSong();
            }
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (SongList.Items.Count > 0)
            {
                if (SongList.SelectedIndex == SongList.Items.Count - 1)
                {
                    SongList.SelectedIndex = 0;
                }
                else
                {
                    SongList.SelectedIndex++;
                }
                NextBackSong();
            }
        }
        private void SongList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SongList.SelectedItem != null)
            {
                string trackName = SongList.SelectedItem.ToString();
                string trackPath = path + "\\" + trackName;
                MediaEl.Source = new Uri(trackPath);
                isPlaying = true;
                MediaEl.Play();
                timer.Start();
            }
        }
        private void UpdateTime()
        {
            if (MediaEl.Source != null)
            {
                Thread thread = new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(1000);
                        this.Dispatcher.Invoke(() =>
                        {
                            TotalTime.Text = String.Format("{0:mm\\:ss}", MediaEl.NaturalDuration.TimeSpan.Subtract(TimeSpan.FromSeconds(AudioSlider.Value)));
                            CurrentTime.Text = String.Format("{0:D2}:{1:D2}", MediaEl.Position.Minutes, MediaEl.Position.Seconds);
                        });
                    }
                });
                thread.Start();
            }
            else
            {
                TotalTime.Text = "00:00";
                CurrentTime.Text = "00:00";
            }
        }

        private void MediaEl_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (isRepeat)
            {
                MediaEl.Position = TimeSpan.Zero;
                MediaEl.Play();
            }
            else if (isRandom)
            {
                RandomTrack();
            }
            else
            {
                AudioSlider.Value = 0;
                timer.Stop();
                SongList.SelectedIndex++;
            }
            TotalTime.Text = "00:00";
            CurrentTime.Text = "00:00";
        }
        private void btnRepeat_Click(object sender, RoutedEventArgs e)
        {
            if (isRepeat == false)
                isRepeat = true;
            else
                isRepeat = false;
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            if (isRandom == false)
                isRandom = true;
            else
                isRandom = false;
        }
    }
}
