// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IDataList.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public interface IDataList : IDataObject, IEnumerable
    {
        IDataObject Create(int key, Func<IDataObject> creator);

        IDataObject GetOrCreate(int key, Func<IDataObject> creator)
            => Get(key) ?? Create(key, creator);

        IDataObject? Get(int key);

        void Connect(int key, IDataObject dataObject);

        void Disconnect(int key);

        void Copy(int sourceKey, int destinationKey);

        void Move(int sourceKey, int destinationKey);
    }

    public interface IDataList<TDataObject> : IDataList, IEnumerable<TDataObject> where TDataObject : class, IDataObject
    {
        QDataObject Create<QDataObject>(int key, Func<QDataObject> creator) where QDataObject : TDataObject;

        QDataObject GetOrCreate<QDataObject>(int key, Func<QDataObject> creator) where QDataObject : class, TDataObject
            => Get<QDataObject>(key) ?? Create(key, creator);

        QDataObject? Get<QDataObject>(int key) where QDataObject : class, TDataObject;


        void Connect(int key, TDataObject dataObject);
    }
}