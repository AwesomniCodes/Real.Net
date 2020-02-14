// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IDataObject.cs" project="Keil.FlowRx" solution="Keil.FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Keil.FlowRx.DataSystem
{
    using System.Reactive.Subjects;

    public interface IDataObject
    {
        object Key { get; }

        ISubject<DataUpdateInfo> Link { get; }
    }
}