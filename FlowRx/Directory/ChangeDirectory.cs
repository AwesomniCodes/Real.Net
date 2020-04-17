// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="ChangeDirectory.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------


namespace Awesomni.Codes.FlowRx
{
    using System;
    using System.Collections.Generic;

    public interface IChangeDirectory : IChangeDictionary<string, IDataObject> { }

    public class ChangeDirectory : ChangeDictionary<string, IDataObject>, IChangeDirectory
    {
        public static new IChangeDirectory Create(string key, IEnumerable<IChange<IDataObject>> changes)
            => new ChangeDirectory(key, changes);
        protected ChangeDirectory(string key, IEnumerable<IChange<IDataObject>> changes) : base(key, changes) { }
    }
}