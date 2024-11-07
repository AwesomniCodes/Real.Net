// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="Entity.cs" project="Real.Net" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.Real.Net
{
    using Awesomni.Codes.Real.Net.Utility;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;

    public abstract class Entity : IEntity
    {
        private ISubject<IEnumerable<IChange>>? _changes;
        public ISubject<IEnumerable<IChange>> Changes => _changes ?? (_changes = CreateChangesSubject());

        protected virtual ISubject<IEnumerable<IChange>> CreateChangesSubject()
            => Subject.Create<IEnumerable<IChange>>(
                    CreateObserverForChangesSubject(),
                    CreateObservableForChangesSubject());

        protected abstract IObserver<IEnumerable<IChange>> CreateObserverForChangesSubject();

        protected abstract IObservable<IEnumerable<IChange>> CreateObservableForChangesSubject();

        public static IDictionary<Type, Type> InterfaceToClassTypeMap { get; } 
            = new Dictionary<Type, Type> {
                {typeof(IEntityDirectory<>), typeof(EntityDirectory<>) },
                {typeof(IEntityDictionary<,>), typeof(EntityDictionary<,>) },
                {typeof(IEntityList<>), typeof(EntityList<>) },
                {typeof(IEntitySubject<>), typeof(EntitySubject<>) },
                {typeof(IEntityObservable<>), typeof(EntityObservable<>) },
            };

        private static IEntity InvokeGenericCreation(Type entityGenericDefinition, Type[] genericSubtypes, params object?[] arguments)
        => (IEntity)entityGenericDefinition
            .MakeGenericType(genericSubtypes)
            .GetMethod(nameof(Create), BindingFlags.Static | BindingFlags.Public)
            .Invoke(null, arguments);

        public static IEntity Create(Type objectType, params object?[] constructorArgs)
        {
            var genericArguments = objectType.GetGenericArguments();
            var genericDefinition = objectType.GetGenericTypeDefinition();
            
            return typeof(IEntity).IsAssignableFrom(genericArguments[0]) && typeof(IEntitySubject<>).IsAssignableFrom(genericDefinition)
                    ? Create(genericArguments[0], new object[] { } )
                    : InvokeGenericCreation(InterfaceToClassTypeMap[objectType.GetGenericTypeDefinition()], objectType.GetGenericArguments(), constructorArgs)
                        ?? throw new ArgumentException("The type is unknown", nameof(objectType));
        }

    }
}