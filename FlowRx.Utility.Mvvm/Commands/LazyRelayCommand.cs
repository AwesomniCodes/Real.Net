// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="LazyRelayCommand.cs" project="Keil.Mvvm" solution="KeilUtility" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Keil.Mvvm.Mvvm.Commands
{
    using System;
    using System.Windows.Input;

    public class LazyRelayCommand : ICommand
    {
        private readonly Lazy<RelayCommand> _innerCommand;

        public LazyRelayCommand(Action<object> execute, Predicate<object> canExecute = null) => _innerCommand = new Lazy<RelayCommand>(() => new RelayCommand(execute, canExecute));

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _innerCommand.Value.CanExecute(parameter);

        public void Execute(object parameter) => _innerCommand.Value.Execute(parameter);
    }
}