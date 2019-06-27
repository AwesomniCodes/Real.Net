// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EventToCommandBehavior.cs" project="Keil.Mvvm" solution="KeilUtility" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Keil.Mvvm.Mvvm.Commands
{
    using System;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interactivity;

    /// <summary>
    /// Behavior that will connect an UI event to a viewmodel Command,
    /// allowing the event arguments to be passed as the CommandParameter.
    /// </summary>
    public class EventToCommandBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(null));

        public static readonly DependencyProperty EventProperty = DependencyProperty.Register(
            "Event",
            typeof(string),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(null, OnEventChanged));

        public static readonly DependencyProperty PassArgumentsProperty = DependencyProperty.Register(
            "PassArguments",
            typeof(bool),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(false));

        private Delegate _handler;

        private EventInfo _oldEvent;

        // Command
        public ICommand Command { get => (ICommand) GetValue(CommandProperty); set => SetValue(CommandProperty, value); }

        // Event
        public string Event { get => (string) GetValue(EventProperty); set => SetValue(EventProperty, value); }

        // PassArguments (default: false)
        public bool PassArguments { get => (bool) GetValue(PassArgumentsProperty); set => SetValue(PassArgumentsProperty, value); }

        protected override void OnAttached() => AttachHandler(this.Event); // initial set

        private static void OnEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var beh = (EventToCommandBehavior) d;

            // is not yet attached at initial load
            if (beh.AssociatedObject != null)
            {
                beh.AttachHandler((string) e.NewValue);
            }
        }

        /// <summary>
        /// Attaches the handler to the event
        /// </summary>
        /// <param name="eventName">The event name</param>
        private void AttachHandler(string eventName)
        {
            // detach old event
            if (_oldEvent != null)
            {
                _oldEvent.RemoveEventHandler(AssociatedObject, _handler);
            }

            // attach new event
            if (!string.IsNullOrEmpty(eventName))
            {
                EventInfo ei = AssociatedObject.GetType().GetEvent(eventName);
                if (ei != null)
                {
                    MethodInfo mi = GetType().GetMethod(
                        "ExecuteCommand",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    _handler = Delegate.CreateDelegate(ei.EventHandlerType, this, mi);
                    ei.AddEventHandler(AssociatedObject, _handler);
                    _oldEvent = ei; // store to detach in case the Event property changes
                }
                else
                {
                    throw new ArgumentException(
                        string.Format(
                            "The event '{0}' was not found on type '{1}'",
                            eventName,
                            AssociatedObject.GetType().Name));
                }
            }
        }

        /// <summary>
        /// Executes the Command
        /// </summary>
        private void ExecuteCommand(object sender, EventArgs e)
        {
            object parameter = PassArguments ? e : null;
            if (Command != null)
            {
                if (Command.CanExecute(parameter))
                {
                    Command.Execute(parameter);
                }
            }
        }
    }
}