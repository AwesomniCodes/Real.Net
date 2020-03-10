// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IDataDirectory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public interface IDataDirectory : IDataObject, IEnumerable<IDataObject>
    {
        IDataItem<TData> GetOrCreate<TData>(string key, TData value = default(TData));

        IDataDirectory GetOrCreateDirectory(string key);
        
        IDataDirectory GetDirectory(string key) => (IDataDirectory)Get(key);

        IDataItem<TData> Get<TData>(string key);

        IDataObject Get(string key);

        void Remove(string key);

        void Copy(string sourceKey, string destinationKey);

        void Move(string sourceKey, string destinationKey);
    }
}