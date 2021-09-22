using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace CurveReamer
{
    public class GCodeSettingsWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Window Owner { get; set; }

        private string startingGCode;
        public string StartingGCode
        {
            get => startingGCode;
            set
            {
                if (startingGCode == value)
                    return;
                startingGCode = value;
                OnPropertyChanged(nameof(StartingGCode));
            }
        }

        private string endingGCode;
        public string EndingGCode
        {
            get => endingGCode;
            set
            {
                if (endingGCode == value)
                    return;
                endingGCode = value;
                OnPropertyChanged(nameof(EndingGCode));
            }
        }

        private RelayCommand closeCommand;
        public RelayCommand CloseCommand
        {
            get => closeCommand;
            set
            {
                if (closeCommand == value)
                    return;
                closeCommand = value;
                OnPropertyChanged(nameof(CloseCommand));
            }
        }

        private RelayCommand saveAndCloseCommand;
        public RelayCommand SaveAndCloseCommand
        {
            get => saveAndCloseCommand;
            set
            {
                if (saveAndCloseCommand == value)
                    return;
                saveAndCloseCommand = value;
                OnPropertyChanged(nameof(SaveAndCloseCommand));
            }
        }

        public GCodeSettingsWindowViewModel()
        {
            StartingGCode = Properties.Settings.Default.StartingGCode;
            EndingGCode = Properties.Settings.Default.EndingGCode;

            SaveAndCloseCommand = new RelayCommand(SaveAndClose);
            CloseCommand = new RelayCommand(Close);
        }

        private void Close()
        {
            Owner.Close();
        }

        private void SaveAndClose()
        {
            Properties.Settings.Default.StartingGCode = StartingGCode;
            Properties.Settings.Default.EndingGCode = EndingGCode;
            Properties.Settings.Default.Save();
            Close();
        }
    }
}
