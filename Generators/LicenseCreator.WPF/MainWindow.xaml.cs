using MaterialDesignThemes.Wpf;

using System;
using System.Windows;

namespace LicenseCreator.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public void CombinedDialogOpenedEventHandler(object sender, DialogOpenedEventArgs eventArgs)
        {
            if(DataContext is not MainViewModel model)return;

            CombinedCalendar.SelectedDate = model.ExpirationDate?.Date;
            CombinedClock.Time = (DateTime)model.ExpirationDate;
        }

        public void CombinedDialogClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if (DataContext is not MainViewModel model) return;
            if (Equals(eventArgs.Parameter, "1") &&
                CombinedCalendar.SelectedDate is DateTime selectedDate)
            {
                var combined = selectedDate.AddSeconds(CombinedClock.Time.TimeOfDay.TotalSeconds);
                model.ExpirationDate = combined;
            }
        }
    }
}
