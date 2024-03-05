using System.Collections.ObjectModel;
using System.Windows;

namespace AudioPlaer
{
    public partial class HistoryWindow : Window
    {

        private ObservableCollection<string> historyList = new ObservableCollection<string>();

        public HistoryWindow()
        {
            InitializeComponent();
            historyListView.ItemsSource = historyList;
        }

        public void AddTrackToHistory(string trackName)
        {
            historyList.Add(trackName);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}