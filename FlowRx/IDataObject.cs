// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IDataObject.cs" project="AwesomniCodes.FlowRx" solution="AwesomniCodes.FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace AwesomniCodes.FlowRx.DataSystem
{
    using System.Reactive.Subjects;

    public interface IDataObject
    {
        object Key { get; }

        ISubject<DataUpdateInfo> Link { get; }
    }
}