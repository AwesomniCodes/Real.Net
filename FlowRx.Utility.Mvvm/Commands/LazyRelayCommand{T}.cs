// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="LazyRelayCommand{T}.cs" project="Keil.Mvvm" solution="KeilUtility" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Keil.Mvvm.Mvvm.Commands
{
    using System;
    using System.Windows.Input;

    public class LazyRelayCommand<T> : ICommand
    {
        private readonly Lazy<RelayCommand<T>> _innerCommand;

        public LazyRelayCommand(Action<T> execute, Predicate<T> canExecute = null) => _innerCommand = new Lazy<RelayCommand<T>>(() => new RelayCommand<T>(execute, canExecute));

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _innerCommand.Value.CanExecute(parameter);

        public void Execute(object parameter) => _innerCommand.Value.Execute(parameter);
    }
}