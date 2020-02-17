// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataUpdateInfo{TData}.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx.DataSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Flags]
    public enum DataUpdateType : int
    {
        Created = 1,
        Connected = 2,
        Modify = 4,
        Remove = 8,
        Request = 16,
        Sync = 32,
    }

    public abstract class DataUpdateInfo
    {
        private List<object> _keyChain = new List<object>();

        protected DataUpdateInfo(DataUpdateType updateType, object key, object value = null) : this(updateType, new List<object> {key}, value)
        {
            UpdateType = updateType;
            ForwardUp(key);
            Value = value;
        }

        protected DataUpdateInfo(DataUpdateType updateType, IEnumerable<object> keyChain, object value = null)
        {
            UpdateType = updateType;
            _keyChain = keyChain.ToList();
            Value = value;
        }

        public DataUpdateType UpdateType { get; }
        public IReadOnlyList<object> KeyChain => _keyChain;
        public object Value { get; }

        public DataUpdateInfo ForwardUp(object key) { return CreateWithSameType(UpdateType, _keyChain.Prepend(key), Value); }

        public DataUpdateInfo ForwardDown(object key)
        {
            if (EqualityComparer<object>.Default.Equals(key, _keyChain[0]))
            {
                return CreateWithSameType(UpdateType, _keyChain.Skip(1), Value);
            }

            throw new InvalidOperationException();
        }

        public abstract DataUpdateInfo CreateWithSameType(DataUpdateType updateType, IEnumerable<object> keyChain, object value);
    }

    public class DataUpdateInfo<TData> : DataUpdateInfo
    {
        internal DataUpdateInfo(DataUpdateType updateType, object key, TData value = default(TData)) : this(updateType, new List<object> {key}, value) { }

        internal DataUpdateInfo(DataUpdateType updateType, IEnumerable<object> keyChain, TData value = default(TData)) : base(updateType, keyChain, value) { }

        public new TData Value => (TData) base.Value;

        public override DataUpdateInfo CreateWithSameType(DataUpdateType updateType, IEnumerable<object> keyChain, object value)
        {
            return new DataUpdateInfo<TData>(updateType, keyChain, value is TData genValue ? genValue : default(TData));
        }
    }
}