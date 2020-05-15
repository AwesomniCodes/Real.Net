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

    public delegate TUngeneric? ObjectGenericCreation<TUngeneric>(Type genericDefinition, Type[] genericArguments, object?[] constructionArguments) where TUngeneric : class;

    public abstract class DataObject : IDataObject
    {
        public abstract ISubject<IEnumerable<IChange>> Changes { get; }

        private static readonly IList<ObjectCreation<IDataObject>> _typeCreations = new List<ObjectCreation<IDataObject>>();

        static DataObject()
        {
            AddGenericCreation((genericDefinition, genericArguments, constructionArguments)
                => typeof(IDataDirectory<>).IsAssignableFrom(genericDefinition) ? Directory(genericArguments.Single()) : null);

            AddGenericCreation((genericDefinition, genericArguments, constructionArguments)
                => typeof(IDataList<>).IsAssignableFrom(genericDefinition) ? List(genericArguments.Single()) : null);

            AddGenericCreation((genericDefinition, genericArguments, constructionArguments)
                => typeof(IDataObject).IsAssignableFrom(genericArguments[0]) ? Create(genericArguments.Single()) : null);

            AddGenericCreation((genericDefinition, genericArguments, constructionArguments)
                => typeof(IDataDictionary<,>).IsAssignableFrom(genericDefinition) ? Dictionary(genericArguments[0], genericArguments[1]) : null);

            AddGenericCreation((genericDefinition, genericArguments, constructionArguments)
                           => typeof(IDataObservable<>).IsAssignableFrom(genericDefinition) ? Observable(genericArguments.Single()) : null);

            AddGenericCreation((genericDefinition, genericArguments, constructionArguments)
                           => typeof(IDataItem<>).IsAssignableFrom(genericDefinition) ? Item(genericArguments.Single(), constructionArguments.FirstOrDefault()) : null);
        }

        public static void AddGenericCreation(ObjectGenericCreation<IDataObject> creation)
        {
            AddCreation((type, arguments) => type.IsGenericType ? creation(type.GetGenericTypeDefinition(), type.GetGenericArguments(), arguments) : null);
        }

        public static void AddCreation(ObjectCreation<IDataObject> creation)
        {
            _typeCreations.Add(creation);
        }

        private static IDataObject InvokeGenericCreation(Type dataObjectGenericDefinition, Type[] genericSubtypes, params object?[] arguments)
            => (IDataObject)dataObjectGenericDefinition
            .MakeGenericType(genericSubtypes)
            .GetMethod(nameof(Create), BindingFlags.Static | BindingFlags.Public)
            .Invoke(null, arguments);

        public static IDataObject Create(Type objectType, params object?[] constructorArgs)
            => _typeCreations.Select(x => x(objectType, constructorArgs)).FirstOrDefault(o => o != null)
                ?? throw new ArgumentException("The type is unknown", nameof(objectType));

        private static IDataObject Dictionary(Type keyType, Type dataObjectType)
            => InvokeGenericCreation(typeof(DataDictionary<,>), new Type[] { keyType, dataObjectType });

        private static IDataObject Directory(Type type)
            => InvokeGenericCreation(typeof(DataDirectory<>), new Type[] { type });

        private static IDataObject Item(Type type, object? initialValue = default)
            => InvokeGenericCreation(typeof(DataItem<>), new Type[] { type }, initialValue);

        private static IDataObject List(Type type)
            => InvokeGenericCreation(typeof(DataList<>), new Type[] { type });

        private static IDataObject Observable(Type type)
            => InvokeGenericCreation(typeof(DataObservable<>), new Type[] { type });

    }
}