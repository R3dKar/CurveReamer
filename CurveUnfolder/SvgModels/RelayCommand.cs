using System;
using System.Windows.Input;

namespace CurveUnfolder
{
    public class RelayCommand : ICommand
    {
        private readonly Action action;
        private readonly Func<bool> canExecute;

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public void TriggerCanExecuteChanged()
        {
            CanExecuteChanged.Invoke(this, new EventArgs());
        }


        public RelayCommand(Action action, Func<bool> canExecute = null)
        {
            this.action = action;
            this.canExecute = canExecute ?? new Func<bool>(() => true);
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute();
        }

        public void Execute(object parameter)
        {
            this.action();
        }
    }
}
