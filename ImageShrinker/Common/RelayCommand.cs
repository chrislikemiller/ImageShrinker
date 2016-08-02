using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ImageShrinker.Common
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other
    /// Ts by invoking delegates. The default return value for the
    /// CanExecute method is 'true'.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute == null)
                    return;
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute == null)
                    return;
                CommandManager.RequerySuggested -= value;
            }
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute((T)parameter);
            return true;
        }

        [DebuggerStepThrough]
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
        [DebuggerStepThrough]
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
        }

        [DebuggerStepThrough]
        public bool CanExecute()
        {
            return true;
        }
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        [DebuggerStepThrough]
        public void Execute()
        {
            _execute();
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return CanExecute();
        }
        [DebuggerStepThrough]
        public void Execute(object parameter)
        {
            Execute();
        }
    }
}
