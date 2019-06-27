// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="RelayCommand{T}.cs" project="Keil.Mvvm" solution="KeilUtility" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Keil.Mvvm.Mvvm.Commands
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Class for the Relay Command, has ICommand as base.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecutePredicate;

        private readonly Action<T> _executeAction;

        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">The predicate to check if the action can be executed.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            _executeAction = execute;
            _canExecutePredicate = canExecute ?? (p => true);
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// True if this command can be executed; otherwise, false.
        /// </returns>
        public bool CanExecute(object parameter)
        {
            if (_canExecutePredicate == null)
            {
                return true;
            }

            if (parameter == null && typeof(T).IsValueType)
            {
                return _canExecutePredicate(default(T));
            }

            if (parameter == null || parameter is T)
            {
                return _canExecutePredicate((T) parameter);
            }

            return false;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(object parameter)
        {
            var val = parameter;

            if (parameter != null && parameter.GetType() != typeof(T))
            {
                if (parameter is IConvertible)
                {
                    val = Convert.ChangeType(parameter, typeof(T), null);
                }
            }

            if (CanExecute(val) && _executeAction != null)
            {
                if (val == null)
                {
                    if (typeof(T).IsValueType)
                    {
                        _executeAction(default(T));
                    }
                    else
                    {
                        _executeAction((T) val);
                    }
                }
                else
                {
                    _executeAction((T) val);
                }
            }
        }
    }
}