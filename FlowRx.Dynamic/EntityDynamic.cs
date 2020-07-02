using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Utility;
using ImpromptuInterface;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FlowRx.Dynamic
{
    public abstract class EntityDynamicBase : EntityDirectory<string>, IEntityDynamic<object>
    {
        public abstract object Value { get; }
    }
    public class EntityDynamic<T> : EntityDynamicBase, IEntityDynamic<T> where T : class
    {
        private IEntityDynamic<T> @this => this;
        private readonly T _value;
        public static new IEntityDynamic<T> Create() => new EntityDynamic<T>();
        static EntityDynamic() => Entity.InterfaceToClassTypeMap[typeof(IEntityDynamic<>)] = typeof(EntityDynamic<>);

        public EntityDynamic()
        {
            if (!typeof(T).IsInterface) throw new ArgumentException("Type needs to be an interface to be dynamically implemented");

            PopulateDictionaryAccordingToInterfaceContract();

            _value = Impromptu.ActLike<T>(this.AsDynamic());
        }

        private void PopulateDictionaryAccordingToInterfaceContract()
        {
            typeof(T)
                .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                .ForEach(propertyInfo =>
                {
                    IEntity propertyDelegate;
                    if (propertyInfo.PropertyType.IsGenericType)
                    {
                        var baseType = propertyInfo.PropertyType.GetGenericTypeDefinition();
                        var genericArguments = propertyInfo.PropertyType.GetGenericArguments();
                        //Check for Subject assignability and add as EntityValue
                        if (typeof(ISubject<>).IsAssignableFrom(baseType))
                        {
                            propertyDelegate = Entity.Create(typeof(IEntityValue<>).MakeGenericType(genericArguments[0]));
                        }
                        //Check for List assignability and add as EntityList
                        //Check for Dict assignability and add as EntityDictionary


                        //TODO: Decide for Generic Subtype needs again dynamic supplementation or can be directly added
                        propertyDelegate = Entity.Create(typeof(IEntityValue<>).MakeGenericType(genericArguments[0]));

                    }
                    else if(propertyInfo.PropertyType.IsInterface)
                    {
                        //Child itself should be a EntityDynamic
                        propertyDelegate = Entity.Create(typeof(IEntityDynamic<>).MakeGenericType(propertyInfo.PropertyType));
                    }
                    else
                    {
                        //Common base value gets added as a EntityValue
                        propertyDelegate = Entity.Create(typeof(IEntityValue<>).MakeGenericType(propertyInfo.PropertyType));
                    }

                    Add(propertyInfo.Name, propertyDelegate);
                });

            
        }


        public override object Value => _value;
        T IEntityDynamic<T>.Value => _value;
    }
}
