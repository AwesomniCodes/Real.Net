// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataObject.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using Awesomni.Codes.FlowRx.Utility;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;

    public delegate TUngeneric? ObjectCreation<TUngeneric>(Type concreteType, object?[] constructionArguments) where TUngeneric : class;

    public delegate TUngeneric? ObjectGenericCreation<TUngeneric>(Type concreteType, Type genericDefinition, Type[] genericArguments, object?[] constructionArguments) where TUngeneric : class;

    public abstract class DataObject : IDataObject
    {
        public abstract ISubject<IEnumerable<IChange>> Changes { get; }

        private static readonly IList<ObjectCreation<IDataObject>> _typeCreations = new List<ObjectCreation<IDataObject>>();

        static DataObject()
        {
            AddCreation((type, _) => typeof(IDataDirectory).IsAssignableFrom(type) ? Directory() : null);

            AddGenericCreation((concreteType, genericDefinition, genericArguments, constructionArguments)
                => typeof(IDataList<>).IsAssignableFrom(genericDefinition) ? List(genericArguments.Single()) : null);

            AddGenericCreation((concreteType, genericDefinition, genericArguments, constructionArguments)
                => typeof(IDataObject).IsAssignableFrom(genericArguments[0]) ? Create(genericArguments.Single()) : null);

            AddGenericCreation((concreteType, genericDefinition, genericArguments, constructionArguments)
                => typeof(IDataDictionary<,>).IsAssignableFrom(genericDefinition) ? Dictionary(genericArguments[0], genericArguments[1]) : null);

            AddGenericCreation((concreteType, genericDefinition, genericArguments, constructionArguments)
                           => typeof(IDataObservable<>).IsAssignableFrom(genericDefinition) ? Observable(genericArguments.Single()) : null);

            AddGenericCreation((concreteType, genericDefinition, genericArguments, constructionArguments)
                           => typeof(IDataItem<>).IsAssignableFrom(genericDefinition) ? Item(genericArguments.Single(), constructionArguments.FirstOrDefault()) : null);

        }

        public static void AddGenericCreation(ObjectGenericCreation<IDataObject> creation)
        {
            AddCreation((type, arguments) => type.IsGenericType ? creation(type, type.GetGenericTypeDefinition(), type.GetGenericArguments(), arguments) : null);
        }

        public static void AddCreation(ObjectCreation<IDataObject> creation)
        {
            _typeCreations.Add(creation);
        }

        public static TDataObject Create<TDataObject>() where TDataObject : class, IDataObject
            => (TDataObject)Create(typeof(TDataObject));

        public static IDataObject Create(Type objectType, params object?[] constructorArgs)
            => _typeCreations.Select(x => x(objectType, constructorArgs)).FirstOrDefault(o => o != null)
                ?? throw new ArgumentException("The type is unknown", nameof(objectType));

        private static IDataDictionary<object?, IDataObject> Dictionary(Type keyType, Type dataObjectType)
            => (IDataDictionary<object?, IDataObject>)Activator.CreateInstance(typeof(DataDictionary<,>).MakeGenericType(keyType, dataObjectType), true);

        private static IDataDictionary<TKey, TDataObject> Dictionary<TKey, TDataObject>() where TDataObject : class, IDataObject
            => DataDictionary<TKey, TDataObject>.Create();

        private static IDataDirectory Directory() => DataDirectory.Create();

        private static IDataItem<TData> Item<TData>(TData initialValue = default)
            => DataItem<TData>.Create(initialValue);

        private static IDataItem<object?> Item(Type type, object? initialValue = default)
            => (IDataItem<object?>)typeof(DataObject)
            .GetMethod(nameof(Item), 1, BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new Type[] { Type.MakeGenericMethodParameter(0) }, new ParameterModifier[] { })
            .MakeGenericMethod(type)
            .Invoke(null, new object?[] { initialValue });

        private static IDataList List(Type dataObjectType)
            => (IDataList)Activator.CreateInstance(typeof(DataList<>).MakeGenericType(dataObjectType), true);

        private static IDataList<TDataObject> List<TDataObject>() where TDataObject : class, IDataObject
            => DataList<TDataObject>.Create();

        private static IDataObservable<TData> Observable<TData>(IObservable<TData> observable, TData initialValue = default)
            => DataObservable<TData>.Create(observable, initialValue);

        private static IDataObject Observable(Type type)
            => (IDataObject)typeof(DataObject)
            .GetMethod(nameof(Observable), 1, new Type[] { Type.MakeGenericMethodParameter(0) })
            .MakeGenericMethod(type)
            .Invoke(null, new object?[] { type.GetDefault() });
    }
}