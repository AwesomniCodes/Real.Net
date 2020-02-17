// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataObject.cs" project="AwesomniCodes.FlowRx" solution="AwesomniCodes.FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace AwesomniCodes.FlowRx.DataSystem
{
    using System.Reactive.Subjects;

    public abstract class DataObject : IDataObject
    {
        protected DataObject(object key) { Key = key; }

        public object Key { get; }

        public abstract ISubject<DataUpdateInfo> Link { get; }

        public abstract IDataObject Clone();
    }
}