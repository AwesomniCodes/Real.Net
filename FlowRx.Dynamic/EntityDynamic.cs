using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Dynamic.Actors;
using Awesomni.Codes.FlowRx.Utility;
using ImpromptuInterface;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Awesomni.Codes.FlowRx.Dynamic
{
    public abstract class EntityDynamic : EntityDirectory<string>, IEntityDynamic<object>
    {
        static EntityDynamic() => Entity.InterfaceToClassTypeMap[typeof(IEntityDynamic<>)] = typeof(EntityDynamic<>);
        public abstract object Value { get; }
    }
    public class EntityDynamic<TInterface> : EntityDynamic, IEntityDynamic<TInterface> where TInterface : class
    {
        private readonly IDictionary<string, object> _delegates = new Dictionary<string, object>();
        private IEntityDynamic<TInterface> @this => this;
        private readonly TInterface _value;
        public static new IEntityDynamic<TInterface> Create() => new EntityDynamic<TInterface>();


        public EntityDynamic()
        {
            if (!typeof(TInterface).IsInterface) throw new ArgumentException("Type needs to be an interface to be dynamically implemented");

            PopulateDictionaryAccordingToInterfaceContract();

            _value = Impromptu.ActLike<TInterface>(@this.AsDynamic());
        }

        private void PopulateDictionaryAccordingToInterfaceContract()
        {
            typeof(TInterface)
                .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                .Select(propertyInfo => (PropertyInfo: propertyInfo, T: GetTypeEntityAndDelegate(propertyInfo.PropertyType)))
                .ForEach(t =>
                {
                    Add(t.PropertyInfo.Name, t.T.Entity);
                    _delegates.Add(t.PropertyInfo.Name, t.T.Delegate);
                });

            
        }

        private (object Delegate, IEntity Entity) GetTypeEntityAndDelegate(Type type, bool insideSubject = false)
        {
            var genericArguments = type.IsGenericType ? type.GetGenericArguments() : new Type[] { };
            var genericDefinition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            Func<(object Delegate, IEntity Entity)> valueResolver = () => GetTypeEntityAndDelegate(genericArguments[0], true);
            (object Delegate, IEntity Entity) t = default;

            if (typeof(IEntity).IsAssignableFrom(genericDefinition))
            {
                if (genericDefinition.IsAssignableFrom(typeof(IEntitySubject<>)))
                {
                    t.Entity = Entity.Create(type, valueResolver().Delegate);
                }
                else
                {
                    t.Entity = Entity.Create(type);
                }
                t.Delegate = t.Entity;
            }
            else if (type.IsGenericType)
            {
                if (genericDefinition.IsAssignableFrom(typeof(ISubject<>)))
                {
                    t.Entity = Entity.Create(typeof(IEntitySubject<>).MakeGenericType(genericArguments), valueResolver().Delegate);
                    t.Delegate = t.Entity;
                }
                else if (genericDefinition.IsAssignableFrom(typeof(IObservable<>)))
                {
                    t.Entity = Entity.Create(typeof(IEntityObservable<>).MakeGenericType(genericArguments), valueResolver().Delegate);
                    t.Delegate = t.Entity;
                }
                else if (genericDefinition.IsAssignableFrom(typeof(IDictionary<,>)))
                {
                    t.Entity = Entity.Create(typeof(IEntityDictionary<,>).MakeGenericType(genericArguments[0], typeof(IEntitySubject<>).MakeGenericType(genericArguments[1])));
                    t.Delegate = t.Entity.AsDynamic();
                }
                else if (genericDefinition.IsAssignableFrom(typeof(IList<>)))
                {
                    t.Entity = Entity.Create(typeof(IEntityList<>).MakeGenericType(typeof(IEntitySubject<>).MakeGenericType(genericArguments)));
                    t.Delegate = t.Entity.AsDynamic();
                }
                else
                {
                    throw new InvalidOperationException($"The type {type} cannot be");
                }
            }
            else if (type.IsInterface)
            {
                t.Entity = Entity.Create(typeof(IEntityDynamic<>).MakeGenericType(type));
                t.Delegate = t.Entity.AsDynamic();
            }

            if(t == default)
            {
                throw new InvalidOperationException($"The type {type} cannot be");
            }

            //Common base value gets added as a EntitySubject
            return insideSubject ? null : Entity.Create(typeof(IEntitySubject<>).MakeGenericType(type), type.GetDefault());
        }
        public override object Value => _value;
        TInterface IEntityDynamic<TInterface>.Value => _value;
    }
}
