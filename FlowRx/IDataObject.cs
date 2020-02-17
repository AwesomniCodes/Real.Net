// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IDataObject.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx.DataSystem
{
    using System.Reactive.Subjects;

    public interface IDataObject
    {
        object Key { get; }

        ISubject<DataUpdateInfo> Link { get; }
    }
}