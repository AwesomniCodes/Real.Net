// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IDataDirectory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public interface IDataDirectory : IDataDictionary<string, IDataObject>
    {
        QDataObject GetOrAdd<QDataObject>(string key, Func<QDataObject> creator) where QDataObject : class, IDataObject
        {
            QDataObject CreateAndAdd()
            {
                var obj = creator();
                Add(key, obj);
                return obj;
            }
            return Get<QDataObject>(key) ?? CreateAndAdd();
        }

        QDataObject? Get<QDataObject>(string key) where QDataObject : class, IDataObject
            => Get(key) as QDataObject;
    }
}