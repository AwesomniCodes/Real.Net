// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="InvokeCommandAction.cs" project="Keil.Mvvm" solution="KeilUtility" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Keil.Mvvm.Mvvm.Commands
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interactivity;

    public sealed class InvokeCommandAction : TriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter",
            typeof(object),
            typeof(InvokeCommandAction),
            null);

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(InvokeCommandAction),
            null);

        public static readonly DependencyProperty InvokeParameterProperty = DependencyProperty.Register(
            "InvokeParameter",
            typeof(object),
            typeof(InvokeCommandAction),
            null);

        private string _commandName;

        public ICommand Command { get => (ICommand) this.GetValue(CommandProperty); set => this.SetValue(CommandProperty, value); }

        public string CommandName
        {
            get => this._commandName;
            set
            {
                if (this.CommandName != value)
                {
                    this._commandName = value;
                }
            }
        }

        public object CommandParameter { get => this.GetValue(CommandParameterProperty); set => this.SetValue(CommandParameterProperty, value); }

        public object InvokeParameter { get => this.GetValue(InvokeParameterProperty); set => this.SetValue(InvokeParameterProperty, value); }

        protected override void Invoke(object parameter)
        {
            this.InvokeParameter = parameter;

            if (this.AssociatedObject != null)
            {
                ICommand command = this.ResolveCommand();
                if ((command != null) && command.CanExecute(this.CommandParameter))
                {
                    command.Execute(this.CommandParameter);
                }
            }
        }

        private ICommand ResolveCommand()
        {
            ICommand command = null;
            if (this.Command != null)
            {
                return this.Command;
            }

            var frameworkElement = this.AssociatedObject as FrameworkElement;
            if (frameworkElement != null)
            {
                object dataContext = frameworkElement.DataContext;
                if (dataContext != null)
                {
                    PropertyInfo commandPropertyInfo = dataContext.GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(
                            p => typeof(ICommand).IsAssignableFrom(p.PropertyType) && string.Equals(
                                     p.Name,
                                     this.CommandName,
                                     StringComparison.Ordinal));

                    if (commandPropertyInfo != null)
                    {
                        command = (ICommand) commandPropertyInfo.GetValue(dataContext, null);
                    }
                }
            }

            return command;
        }
    }
}