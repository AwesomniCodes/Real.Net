using Awesomni.Codes.FlowRx;
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

            _value = Impromptu.ActLike<TInterface>(@this.AsDynamic());
        }

        private void PopulateDictionaryAccordingToInterfaceContract()
        {
            typeof(TInterface)
                .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                .ForEach(propertyInfo =>
                {
                    IEntity propertyDelegate;
                    if (propertyInfo.PropertyType.IsGenericType)
                    {
                        var genericArguments = propertyInfo.PropertyType.GetGenericArguments();

                        var correspondingEntityValueType = typeof(IEntitySubject<>).MakeGenericType(genericArguments[0]);
                        //Check for Subject assignability and add as EntityValue
                        if (propertyInfo.PropertyType.IsAssignableFrom(correspondingEntityValueType))
                        {
                            propertyDelegate = Entity.Create(typeof(IEntitySubject<>).MakeGenericType(genericArguments[0]), genericArguments.First().GetDefault());
                        }
                        else
                        {
                            throw new Exception();
                        }
                        //Check for List assignability and add as EntityList
                        //Check for Dict assignability and add as EntityDictionary


                        //TODO: Decide for Generic Subtype needs again dynamic supplementation or can be directly added

                    }
                    else if(propertyInfo.PropertyType.IsInterface)
                    {
                        //Child itself should be a EntityDynamic
                        propertyDelegate = Entity.Create(typeof(IEntityDynamic<>).MakeGenericType(propertyInfo.PropertyType));
                    }
                    else
                    {
                        //Common base value gets added as a EntityValue
                        propertyDelegate = Entity.Create(typeof(IEntitySubject<>).MakeGenericType(propertyInfo.PropertyType), propertyInfo.PropertyType.GetDefault());
                    }

                    Add(propertyInfo.Name, propertyDelegate);
                });

            
        }


        public override object Value => _value;
        TInterface IEntityDynamic<TInterface>.Value => _value;
    }
}
