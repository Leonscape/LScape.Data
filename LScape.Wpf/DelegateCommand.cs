using System;
using System.Windows.Input;

namespace LScape.Wpf
{
    /// <summary>
    /// Simple Delegate Command to provide ICommand support and a function to execute
    /// </summary>
    /// <typeparam name="T">The type of parameter for the executing command</typeparam>
    public class DelegateCommand<T> : ICommand
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;

        /// <summary>
        /// Event raised when the execute function is changed 
        /// </summary>
        public event EventHandler CanExecuteChanged;
        
        /// <summary>
        /// Constructs the command
        /// </summary>
        /// <param name="execute">The function to execute for the command</param>
        /// <param name="canExecute">The function to call to decide if the command is executeable</param>
        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Whether this command is ewxecutable
        /// </summary>
        /// <param name="parameter">The parameter to supply</param>
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            return _canExecute((T) parameter);
        }

        /// <summary>
        /// ACtually executes the command
        /// </summary>
        /// <param name="parameter">The parameter to supply</param>
        public void Execute(object parameter)
        {
            _execute((T) parameter);
        }

        /// <summary>
        /// Called when execute is changed
        /// </summary>
        protected void OnExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Simple Delegate command to provide ICommand Support and an action to execute
    /// </summary>
    public class DelegateCommand : DelegateCommand<object>
    {
        /// <summary>
        /// Constructs a command 
        /// </summary>
        /// <param name="execute">The action to execute</param>
        public DelegateCommand(Action execute) : base((o) => execute())
        {}

        /// <summary>
        /// Constructs a command 
        /// </summary>
        /// <param name="execute">The action to execute</param>
        /// <param name="canExecute">The function to call to decide if the command is executeable</param>
        public DelegateCommand(Action execute, Func<bool> canExecute) : base((o) => execute(), (o) => canExecute())
        {}
    }
}
