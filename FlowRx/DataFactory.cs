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
        public IDataObject FromChange(IChange change)
        {
            var changeType = change.GetType().GetTypeIfImplemented(typeof(IChange<>));
            if (changeType != null)
            {
                var genericParams = changeType.GetGenericArguments();
                return Object(genericParams.Single());
            }

            throw new ArgumentException("The change must also implement the generic change interface", nameof(change));
        }

        public TDataObject FromChange<TDataObject>(IChange<TDataObject> change) where TDataObject : class, IDataObject
            => Object<TDataObject>();


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

                //TODO
                //if (genericDefinition == typeof(DataList<>) || genericDefinition == typeof(IDataList<>))
                //{
                //    return List(genericParams[0], genericParams[1]);
                //}

                if (genericDefinition == typeof(DataDictionary<,>) || genericDefinition == typeof(IDataDictionary<,>))
                {
                    return Dictionary(genericParams[0], genericParams[1]);
                }

                if (genericDefinition == typeof(DataObservable<>) /*|| genericDefinition == typeof(IDataObservable<>)*/)
                {
                    return Observable(genericParams.Single());
                }

                if (genericDefinition == typeof(DataItem<>) || genericDefinition == typeof(IDataItem<>))
                {
                    return Item(genericParams.Single(), constructorArgs.FirstOrDefault());
                }
            }

            //TODO
            //if (objectType == typeof(DataDirectory) || objectType == typeof(IDataDirectory))
            //{
            //    return Directory();
            //}

            throw new ArgumentException("The type is unknown", nameof(objectType));
        }

        public IDataDictionary Dictionary(Type keyType, Type dataObjectType)
            => (IDataDictionary)Activator.CreateInstance(typeof(DataDictionary<,>).MakeGenericType(keyType, dataObjectType), true);

        public IDataDictionary<TKey, TDataObject> Dictionary<TKey,TDataObject>() where TDataObject : class, IDataObject
            =>  new DataDictionary<TKey, TDataObject>();

        public IDataItem<TData> Item<TData>(TData initialValue = default)
            => new DataItem<TData>(initialValue);

        public IDataItem Item(Type type, object? initialValue = default)
            => (IDataItem)GetType()
            .GetMethod(nameof(Item), 1, new Type[] { Type.MakeGenericMethodParameter(0) })
            .MakeGenericMethod(type)
            .Invoke(this, new object?[] { initialValue });

        public DataObservable<TData> Observable<TData>(IObservable<TData> observable, TData initialValue = default)
            => new DataObservable<TData>(observable, initialValue);
        
        public IDataObject Observable(Type type)
            => (IDataObject)GetType()
            .GetMethod(nameof(Observable), 1, new Type[] { Type.MakeGenericMethodParameter(0) })
            .MakeGenericMethod(type)
            .Invoke(this, new object?[] { type.GetDefault() });
    }
}