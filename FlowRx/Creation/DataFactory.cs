// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataFactory.cs" project="FlowRx" solution="FlowRx" />
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


    public class DataFactory : IDataFactory
    {
        private readonly IList<ObjectCreation<IDataObject>> _typeCreations = new List<ObjectCreation<IDataObject>>();
        public DataFactory()
        {
            AddCreation((type, _) => typeof(IDataDirectory).IsAssignableFrom(type) ? Directory() : null);

            AddGenericCreation((concreteType, genericDefinition, genericArguments, constructionArguments)
                => typeof(IDataList<>).IsAssignableFrom(genericDefinition) ? List(genericArguments.Single()) : null );

            AddGenericCreation((concreteType, genericDefinition, genericArguments, constructionArguments)
                => typeof(IDataObject).IsAssignableFrom(genericArguments[0]) ? Object(genericArguments.Single()) : null);

            AddGenericCreation((concreteType, genericDefinition, genericArguments, constructionArguments)
                => typeof(IDataDictionary<,>).IsAssignableFrom(genericDefinition) ? Dictionary(genericArguments[0], genericArguments[1]) : null);

            AddGenericCreation((concreteType, genericDefinition, genericArguments, constructionArguments)
                           => typeof(IDataObservable<>).IsAssignableFrom(genericDefinition) ? Observable(genericArguments.Single()) : null);
            
            AddGenericCreation((concreteType, genericDefinition, genericArguments, constructionArguments)
                           => typeof(IDataItem<>).IsAssignableFrom(genericDefinition) ? Item(genericArguments.Single(), constructionArguments.FirstOrDefault()) : null);

        }

        public void AddGenericCreation(ObjectGenericCreation<IDataObject> creation)
        {
            AddCreation((type, arguments) => type.IsGenericType ? creation(type, type.GetGenericTypeDefinition(), type.GetGenericArguments(), arguments) : null );
        }

        public void AddCreation(ObjectCreation<IDataObject> creation)
        {
            _typeCreations.Add(creation);
        }

        public TDataObject Object<TDataObject>() where TDataObject : class, IDataObject
            => (TDataObject)Object(typeof(TDataObject));

        public IDataObject Object(Type objectType, params object?[] constructorArgs)
            => _typeCreations.Select(x => x(objectType, constructorArgs)).FirstOrDefault(o => o != null)
                ?? throw new ArgumentException("The type is unknown", nameof(objectType));

        public IDataDictionary Dictionary(Type keyType, Type dataObjectType)
            => (IDataDictionary)Activator.CreateInstance(typeof(DataDictionary<,>).MakeGenericType(keyType, dataObjectType), true);

        public IDataDictionary<TKey, TDataObject> Dictionary<TKey, TDataObject>() where TDataObject : class, IDataObject
            => DataDictionary<TKey, TDataObject>.Create();

        public IDataDirectory Directory() => DataDirectory.Create();

        public IDataItem<TData> Item<TData>(TData initialValue = default)
            => DataItem<TData>.Create(initialValue);

        public IDataItem<object?> Item(Type type, object? initialValue = default)
            => (IDataItem<object?>)GetType()
            .GetMethod(nameof(Item), 1, new Type[] { Type.MakeGenericMethodParameter(0) })
            .MakeGenericMethod(type)
            .Invoke(this, new object?[] { initialValue });

        public IDataList List(Type dataObjectType)
            => (IDataList)Activator.CreateInstance(typeof(DataList<>).MakeGenericType(dataObjectType), true);

        public IDataList<TDataObject> List<TDataObject>() where TDataObject : class, IDataObject
            => DataList<TDataObject>.Create();

        public IDataObservable<TData> Observable<TData>(IObservable<TData> observable, TData initialValue = default)
            => DataObservable<TData>.Create(observable, initialValue);

        public IDataObject Observable(Type type)
            => (IDataObject)GetType()
            .GetMethod(nameof(Observable), 1, new Type[] { Type.MakeGenericMethodParameter(0) })
            .MakeGenericMethod(type)
            .Invoke(this, new object?[] { type.GetDefault() });
    }
}