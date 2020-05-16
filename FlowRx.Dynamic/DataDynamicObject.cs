using Awesomni.Codes.FlowRx;
using ImpromptuInterface;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reactive.Subjects;

namespace FlowRx.Dynamic
{
    public abstract class DataDynamicObjectBase : DataDirectory<string>, IDataDynamicObject<object>
    {
        public abstract object Value { get; }
    }
    public class DataDynamicObject<T> : DataDynamicObjectBase, IDataDynamicObject<T> where T : class
    {
        private IDataDynamicObject<T> @this => this;
        private readonly T _value;
        public static new IDataDynamicObject<T> Create()
        {
            return new DataDynamicObject<T>();
        }

        public DataDynamicObject()
        {
            _value = Impromptu.ActLike<T>(this.AsDynamic());
        }

        public override object Value => _value;
        T IDataDynamicObject<T>.Value => _value;
    }
}
