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
        private IEntityDynamic<TInterface> @this => this;
        private readonly TInterface _value;
        public static new IEntityDynamic<TInterface> Create() => new EntityDynamic<TInterface>();


        public EntityDynamic()
        {
            if (!typeof(TInterface).IsInterface) throw new ArgumentException("Type needs to be an interface to be dynamically implemented");

            PopulateDictionaryAccordingToInterfaceContract();

            _value = Impromptu.ActLike<TInterface>(@this.AsDynamic(), new Type[] { typeof(IEntity) });
        }

        private void PopulateDictionaryAccordingToInterfaceContract()
        {
            var interfaceEntitiesAndDelegates = typeof(TInterface)
                .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                .Select(propertyInfo => (PropertyInfo: propertyInfo, Entity: propertyInfo.PropertyType.GetEntityBackend()))
                .ForEach(t =>
                {
                    Add(t.PropertyInfo.Name, t.Entity);
                });
        }
        
        public override object Value => _value;
        TInterface IEntityDynamic<TInterface>.Value => _value;
    }
}
