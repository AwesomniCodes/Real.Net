// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataObject.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public abstract class DataObject : IDataObject
    {
        public abstract ISubject<IEnumerable<SomeChange>> Changes { get; }
    }
}