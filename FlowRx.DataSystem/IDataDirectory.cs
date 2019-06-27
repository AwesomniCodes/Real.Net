// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IDataDirectory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace FlowRx.DataSystem
{
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public interface IDataDirectory : IDataObject, IEnumerable<DataObject>
    {
        DataItem<TData> GetOrCreate<TData>(string key, TData value = default(TData));

        DataDirectory GetOrCreateDirectory(string key);

        DataItem<TData> Get<TData>(string key);

        DataObject Get(string key);

        void Delete(string key);

        void Copy(string sourceKey, string destinationKey);

        void Move(string sourceKey, string destinationKey);
    }
}