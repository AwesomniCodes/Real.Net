// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="ViewModelBase.cs" project="FlowRx.Utility.Mvvm" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace FlowRx.Utility.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using DynamicData;
    using DynamicProperties;
    using Utility.Extensions;

    /// <summary>
    /// A Base class that implements INotifyPropertyChanged and makes the creation of properties easier by providing special get and set methods with automated backing field
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        private Subject<(string Name, object Value)> _propertyChangedTrigger = new Subject<(string Name, object Value)>();

        private List<IDisposable> _dependentSubscriptions = new List<IDisposable>();

        private SourceCache<PropertyFeatures, string> _properties = new SourceCache<PropertyFeatures, string>(pf => pf.Name);
        private ReadOnlyObservableCollection<PropertyFeatures> _propertiesBind;

        private DynamicPropertyManager<ViewModelBase> _propertyManager;

        public ViewModelBase()
        {
            _propertyManager = new DynamicPropertyManager<ViewModelBase>(this);
            _properties.Connect().Bind(out _propertiesBind);


            foreach (var prop in TypeDescriptor.GetProperties(this).OfType<PropertyDescriptor>().Where(p => !p.Attributes.Contains(new IgnoreDataMemberAttribute())))
            {
                var propertySubject = new BehaviorSubject<object>(prop.GetType().GetDefault());
                var propFeature = new PropertyFeatures
                {
                    Name = prop.Name,
                    Set = propertySubject,
                    Get = propertySubject,
                    IsDynamic = false,
                };

                _properties.AddOrUpdate(propFeature);
            }

            WhenPropertyChanged = _properties.Connect().MergeMany(pf => pf.Get.DistinctUntilChanged().Select(o => (Name: pf.Name, Value: o))).Merge(_propertyChangedTrigger);
            WhenPropertyChanged.Subscribe(
                (changed) =>
                {

                    _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(changed.Name));

                });
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add { _propertyChanged += value; } remove { _propertyChanged -= value; } }

        // ReSharper disable once InconsistentNaming
        private event PropertyChangedEventHandler _propertyChanged;

        [IgnoreDataMember] public IObservable<(string Name, object Value)> WhenPropertyChanged { get; }

        
        public void AddObservable<T>(string name, IObservable<T> property, T startValue = default(T))
        {
            if (property == null)
            {
                throw new ArgumentNullException();
            }

            var firstAsyncAwaiter = property.FirstAsync().GetAwaiter();
            var initialValue = !EqualityComparer<T>.Default.Equals(startValue, default(T)) ? startValue :
                firstAsyncAwaiter.IsCompleted ? firstAsyncAwaiter.GetResult() : default(T);

            var propFeature = new PropertyFeatures
            {
                Name = name,
                LastValue = initialValue,
                Get = property.Select(t => (object) t).Publish().RefCount(),
                Set = null,
                IsDynamic = true
            };

            propFeature.Get.Subscribe(o => propFeature.LastValue = o);
            _properties.AddOrUpdate(propFeature);
            
            _propertyManager.Properties.Add(
                DynamicPropertyManager<ViewModelBase>.CreateProperty<ViewModelBase, T>(
                    name,
                    t => Get<T>(name),
                    null));
        }

        public void AddSubject<T>(string name, ISubject<T> property, T startValue = default(T))
        {
            if (property == null)
            {
                throw new ArgumentNullException();
            }

            var firstAsyncAwaiter = property.FirstAsync().GetAwaiter();
            var initialValue = !EqualityComparer<T>.Default.Equals(startValue, default(T)) ? startValue :
                firstAsyncAwaiter.IsCompleted ? firstAsyncAwaiter.GetResult() : default(T);

            var propFeature = new PropertyFeatures
            {
                Name = name,
                Get = property.Select(t => (object) t).Publish().RefCount(),
                Set = property.Transform<object, T>(t => t is T value ? value : default(T)),
                LastValue = initialValue,
                IsDynamic = true
            };

            propFeature.Get.Subscribe(o => propFeature.LastValue = o);
            _properties.AddOrUpdate(propFeature);
            
            _propertyManager.Properties.Add(
                DynamicPropertyManager<ViewModelBase>.CreateProperty<ViewModelBase, T>(
                    name,
                    t => Get<T>(name),
                    (t, y) => Set(y, name),
                    null));
        }

        public void Add<T>(string name, T startValue = default(T))
        {
            var propertySubject = new BehaviorSubject<object>(startValue);

            var propFeature = new PropertyFeatures
            {
                Name = name,
                Get = propertySubject,
                Set = propertySubject,
                LastValue = startValue,
                LogOnUserSet = false,
                IsDynamic = true
            };

            propFeature.Get.Subscribe(o => propFeature.LastValue = o);
            _properties.AddOrUpdate(propFeature);
            
            _propertyManager.Properties.Add(
                DynamicPropertyManager<ViewModelBase>.CreateProperty<ViewModelBase, T>(
                    name,
                    t => Get<T>(name),
                    (t, y) => Set(y, name),
                    null));
        }

        public IObserver<T> GetObserver<T>(string name) => _properties.Items.FirstOrDefault(pf => pf.Name == name)?.Set?.Transform<T,object>(t => t);

        public IObservable<T> GetObservable<T>(string name) => _properties.Items.FirstOrDefault(pf => pf.Name == name)?.Get.Cast<T>();

        public ISubject<T> GetSubject<T>(string name) => Subject.Create<T>(GetObserver<T>(name), GetObservable<T>(name));

        /// <summary>
        /// Gets the value of a property
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="name">The name of the property</param>
        /// <returns>The value of the property</returns>
        public T Get<T>([CallerMemberName] string name = null)
        {
            Debug.Assert(name != null, "name != null");
            var propFeature = _properties.Items.FirstOrDefault(pf => pf.Name == name);
            if (propFeature != null)
            {
                return propFeature?.LastValue is T value ? value : default(T);
            }

            return default(T);
        }

        /// <summary>
        /// Sets the value of a property
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="value">The value of the property</param>
        /// <param name="name">The name of the property</param>
        /// <remarks>Use this overload when implicitly naming the property</remarks>
        public void Set<T>(T value, [CallerMemberName] string name = null)
        {
            Debug.Assert(name != null, "name != null");
            if (Equals(value, Get<T>(name)))
            {
                return;
            }

            var propFeature = _properties.Items.FirstOrDefault(pf => pf.Name == name);

            if (propFeature?.Set != null)
            {
                propFeature.LastValue = value;
                propFeature.Set?.OnNext(value);
            }
        }

        protected void TriggerPropertyChanged(string name) { _propertyChangedTrigger.OnNext((name, _properties.Items.FirstOrDefault(pf => pf.Name == name)?.LastValue)); }

    }
}