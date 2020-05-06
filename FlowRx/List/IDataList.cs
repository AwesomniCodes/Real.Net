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

    public interface IDataList<TDataObject> : IDataObject, IEnumerable, ICollection, IList, IEnumerable<TDataObject>, ICollection<TDataObject>, IList<TDataObject>, IReadOnlyCollection<TDataObject> where TDataObject : class, IDataObject
    {
        new TDataObject this[int index] { get; set; }
    }
}