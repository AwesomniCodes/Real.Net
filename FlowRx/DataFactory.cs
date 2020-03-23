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
    using System.Reactive.Subjects;
    using System.Reflection;

    public class DataFactory : IDataFactory
    {
        public IDataObject FromChange(IChange change)
        {
            var changeType = change.GetType().GetTypeIfImplemented(typeof(IChange<>));
            if (changeType != null)
            {
            }

            throw new ArgumentException("The change must also implement the generic change interface", nameof(change));
        }

        public TDataObject FromChange<TDataObject>(IChange<TDataObject> change) where TDataObject : class, IDataObject
            => (TDataObject)Object(typeof(TDataObject));

        public IDataObject Object(Type objectType) => throw new ArgumentException("The type is not a valid DataObject type");

        public IDataDictionary Dictionary(Type keyType, Type dataObjectType)
            => (IDataDictionary)Activator.CreateInstance(typeof(DataDictionary<,>).MakeGenericType(keyType, dataObjectType), true);

        public IDataDictionary<TKey, TDataObject> Dictionary<TKey,TDataObject>() where TDataObject : class, IDataObject
            =>  new DataDictionary<TKey, TDataObject>();

        public IDataItem<TData> Item<TData>(TData initialValue = default)
            => new DataItem<TData>(initialValue);

        public IDataItem Item(object initialValue)
            => (IDataItem) GetType()
            .GetMethod(nameof(Item), 1, new Type[] { Type.MakeGenericMethodParameter(0) })
            .MakeGenericMethod(initialValue.GetType())
            .Invoke(this, new object[] { initialValue });

        public IDataItem Item(Type type)
            => (IDataItem)GetType()
            .GetMethod(nameof(Item), 1, new Type[] { Type.MakeGenericMethodParameter(0) })
            .MakeGenericMethod(type)
            .Invoke(this, new object?[] { type.GetDefault() });

        public DataObservable<TData> Observable<TData>(IObservable<TData> observable, TData initialValue = default)
            => new DataObservable<TData>(observable, initialValue);
    }
}