// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataItem{TData}.cs" project="Keil.FlowRx" solution="Keil.FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Keil.FlowRx.DataSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public interface IDataItem<TData> : IDataObject, ISubject<TData>, IDisposable
    {
        TData Value { get; }
    }
}