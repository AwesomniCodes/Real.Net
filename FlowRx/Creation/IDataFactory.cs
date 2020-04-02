// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataFactory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Awesomni.Codes.FlowRx
{
    public interface IDataFactory
    {
        IDataDictionary Dictionary(Type keyType, Type dataObjectType);
        IDataDictionary<TKey, TDataObject> Dictionary<TKey, TDataObject>() where TDataObject : class, IDataObject;
        IDataDirectory Directory();
        IDataItem Item(Type type, object? initialValue = null);
        IDataItem<TData> Item<TData>(TData initialValue = default);
        IDataList List(Type dataObjectType);
        IDataList<TDataObject> List<TDataObject>() where TDataObject : class, IDataObject;
        IDataObject Object(Type objectType, params object?[] constructorArgs);
        TDataObject Object<TDataObject>() where TDataObject : class, IDataObject;
        IDataObject Observable(Type type);
        IDataObservable<TData> Observable<TData>(IObservable<TData> observable, TData initialValue = default);

        void AddGenericCreation(ObjectGenericCreation<IDataObject> creation);

        void AddCreation(ObjectCreation<IDataObject> creation);
    }

    public delegate TUngeneric? ObjectCreation<TUngeneric>(Type concreteType, object?[] constructionArguments) where TUngeneric : class;

    public delegate TUngeneric? ObjectGenericCreation<TUngeneric>(Type concreteType, Type genericDefinition, Type[] genericArguments, object?[] constructionArguments) where TUngeneric : class;

}