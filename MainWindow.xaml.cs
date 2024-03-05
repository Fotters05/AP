using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioPlaer
{
    public partial class MainWindow : Window
    {
        public DispatcherTimer songTimer;
        public MainWindow()
        {
            InitializeComponent();
            VolumeSlider.Minimum = 0;
            VolumeSlider.Maximum = 1;
            VolumeSlider.Value = 1;
            songList.DisplayMemberPath = "songName";

            songTimer = new DispatcherTimer();
            songTimer.Interval = TimeSpan.FromSeconds(1); // Интервал таймера - 1 секунда
            songTimer.Tick += SongTimer_Tick; // Обработчик события Tick
            songTimer.Start();
        }



        private void SongTimer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.Source != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                TimeSpan currentTime = mediaElement.Position;
                TimeSpan totalTime = mediaElement.NaturalDuration.TimeSpan;

                SongTime.Text = $"{currentTime.ToString(@"mm\:ss")} / {totalTime.ToString(@"mm\:ss")}";
            }
        }

        int fileIndex = 0;
        Random rand = new Random();
        List<Song> fileList = new List<Song>();
        List<Song> randomList = new List<Song>();
        bool pause = false;
        bool loop = false;
        bool random = false;


        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryWindow historyWindow = new HistoryWindow();

            historyWindow.Owner = this;

            bool? result = historyWindow.ShowDialog();

            if (result == true)
            {
            }
            else
            {
            }
        }

        private async void FolderOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            if (random) Random_Button_Click(sender, e);
            if (pause) Pause_Button_Click(sender, e);
            mediaElement.Stop();

            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            dialog.Title = "Выбери папку с музыкой и нажми выбрать";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string[] files = Directory.GetFiles(dialog.FileName);

                fileList.Clear();
                foreach (string file in files)
                {
                    if (file.EndsWith(".mp3") || file.EndsWith(".wav") || file.EndsWith(".m4a"))
                    {
                        fileList.Add(new Song(file));
                    }
                }

                songList.ItemsSource = fileList;
                songList.SelectedIndex = 0;

                Play_Button_Click(sender, e);
            }
        }

        private void Play_Button_Click(object sender, RoutedEventArgs e)
        {
            if (fileList.Count == 0) return;

            mediaElement.Stop();
            SongPositionSlider.Value = 0;

            List<Song> selectedList = random ? randomList : fileList;
            int selectedIndex = songList.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= selectedList.Count) return;

            CurrentSongName.Text = selectedList[selectedIndex].songName;
            CurrentSongImage.Source = selectedList[selectedIndex].songImage;
            mediaElement.Source = new Uri(selectedList[selectedIndex].path);
            mediaElement.Play();

        }

        private void Pause_Button_Click(object sender, RoutedEventArgs e)
        {
            if (pause)
            {
                mediaElement.Play();
                pause = false;
            }
            else
            {
                mediaElement.Pause();
                pause = true;
            }

            Pause_Button.Foreground = pause ? Brushes.Black : Brushes.Black;
        }

        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
        }

        private void SongList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Play_Button_Click(sender, e);
        }

        private void Loop_Button_Click(object sender, RoutedEventArgs e)
        {
            switch (loop)
            {
                case false:
                    Loop_Button.Foreground = Brushes.Black;
                    loop = true;
                    break;
                case true:
                    Loop_Button.Foreground = Brushes.Black;
                    loop = false;
                    break;
            }
        }

        private void Random_Button_Click(object sender, RoutedEventArgs e)
        {
            RandomList();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = (double)VolumeSlider.Value;
        }

        private void SongPositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Position = new TimeSpan(Convert.ToInt64(SongPositionSlider.Value));

        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);

            timer.Start();
            SongPositionSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.Ticks;
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Song? currentSong = songList.SelectedItem as Song;

            if (currentSong != null)
            {
                CurrentSongName.Text = currentSong.songName;

                if (Application.Current.Windows.OfType<HistoryWindow>().FirstOrDefault() is HistoryWindow historyWindow)
                {
                    historyWindow.AddTrackToHistory(CurrentSongName.Text);
                }
            }

            if (!loop)
            {
                NextSong_Button_Click(sender, e);
            }
            else
            {
                mediaElement.Position = TimeSpan.Zero;
                mediaElement.Play();
            }
        }

        private void PreviousSong_Button_Click(object sender, RoutedEventArgs e)
        {
            songList.SelectedIndex = songList.SelectedIndex == 0 || songList.SelectedIndex == -1
                ? songList.Items.Count - 1
                : songList.SelectedIndex - 1;
        }

        private void NextSong_Button_Click(object sender, RoutedEventArgs e)
        {
            if (songList.SelectedIndex + 1 == songList.Items.Count)
            {
                songList.SelectedIndex = 0;
            }
            else
            {
                songList.SelectedIndex++;
            }
        }

        private void RandomList()
        {
            if (!random)
            {
                Random_Button.Foreground = Brushes.Black;
                random = true;

                fileIndex = songList.SelectedIndex;

                randomList.Clear();
                randomList.AddRange(fileList);

                for (int i = randomList.Count - 1; i >= 1; i--)
                {
                    int j = rand.Next(i + 1);

                    var temp = randomList[j];
                    randomList[j] = randomList[i];
                    randomList[i] = temp;
                }

                songList.ItemsSource = randomList;
                songList.SelectedIndex = 0;
            }
            else
            {
                Random_Button.Foreground = Brushes.Black;
                random = false;

                songList.ItemsSource = fileList;
                songList.SelectedIndex = fileIndex;
            }
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            SongPositionSlider.Value = mediaElement.Position.Ticks;
        }
    }
}