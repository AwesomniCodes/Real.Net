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
        public TDataObject Object<TDataObject>() where TDataObject : class, IDataObject
            => (TDataObject)Object(typeof(TDataObject));

        public IDataObject Object(Type objectType, params object?[] constructorArgs)
        {
            if (objectType.IsGenericType)
            {
                var genericParams = objectType.GetGenericArguments();
                var genericDefinition = objectType.GetGenericTypeDefinition();

                if (typeof(IDataObject).IsAssignableFrom(genericParams[0]))
                {
                    return Object(genericParams[0], constructorArgs);
                }

                if (typeof(IDataList<>).IsAssignableFrom(genericDefinition))
                {
                    return List(genericParams.Single());
                }

                if (typeof(IDataDictionary<,>).IsAssignableFrom(genericDefinition))
                {
                    return Dictionary(genericParams[0], genericParams[1]);
                }

                if (typeof(IDataObservable<>).IsAssignableFrom(genericDefinition))
                {
                    return Observable(genericParams.Single());
                }

                if (typeof(IDataItem<>).IsAssignableFrom(genericDefinition))
                {
                    return Item(genericParams.Single(), constructorArgs.FirstOrDefault());
                }
            }

            if (typeof(IDataDirectory).IsAssignableFrom(objectType))
            {
                return Directory();
            }

            throw new ArgumentException("The type is unknown", nameof(objectType));
        }

        public IDataDictionary Dictionary(Type keyType, Type dataObjectType)
            => (IDataDictionary)Activator.CreateInstance(typeof(DataDictionary<,>).MakeGenericType(keyType, dataObjectType), true);

        public IDataDictionary<TKey, TDataObject> Dictionary<TKey,TDataObject>() where TDataObject : class, IDataObject
            =>  new DataDictionary<TKey, TDataObject>();

        public IDataDirectory Directory() => new DataDirectory();

        public IDataItem<TData> Item<TData>(TData initialValue = default)
            => new DataItem<TData>(initialValue);

        public IDataItem Item(Type type, object? initialValue = default)
            => (IDataItem)GetType()
            .GetMethod(nameof(Item), 1, new Type[] { Type.MakeGenericMethodParameter(0) })
            .MakeGenericMethod(type)
            .Invoke(this, new object?[] { initialValue });

        public IDataList List(Type dataObjectType)
            => (IDataList)Activator.CreateInstance(typeof(DataList<>).MakeGenericType(dataObjectType), true);

        public IDataList<TDataObject> List<TDataObject>() where TDataObject : class, IDataObject
            => new DataList<TDataObject>();

        public IDataObservable<TData> Observable<TData>(IObservable<TData> observable, TData initialValue = default)
            => new DataObservable<TData>(observable, initialValue);
        
        public IDataObject Observable(Type type)
            => (IDataObject)GetType()
            .GetMethod(nameof(Observable), 1, new Type[] { Type.MakeGenericMethodParameter(0) })
            .MakeGenericMethod(type)
            .Invoke(this, new object?[] { type.GetDefault() });
    }
}