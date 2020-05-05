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

    public interface IDataList : IDataObject, IEnumerable, ICollection, IList
    {
        IDataObject GetOrConnect(int key, Func<IDataObject> creator)
        {
            IDataObject CreateAndAdd()
            {
                var obj = creator();
                Add(key, obj);
                return obj;
            }
            return Get(key) ?? CreateAndAdd();
        }

        IDataObject? Get(int key);

        void Add(int key, IDataObject dataObject);

        void Remove(int key);

        new IDataObject this[int index] { get; set; }
    }

    public interface IDataList<TDataObject> : IDataList, IEnumerable<TDataObject>, ICollection<TDataObject>, IList<TDataObject>, IReadOnlyCollection<TDataObject> where TDataObject : class, IDataObject
    {
        QDataObject GetOrAdd<QDataObject>(int key, Func<QDataObject> creator) where QDataObject : class, TDataObject
        {
            QDataObject CreateAndAdd()
            {
                var obj = creator();
                Add(key, obj);
                return obj;
            }
            return Get<QDataObject>(key) ?? CreateAndAdd();
        }

        QDataObject? Get<QDataObject>(int key) where QDataObject : class, TDataObject;

        void Add(int key, TDataObject dataObject);

        new TDataObject this[int index] { get; set; }
    }
}