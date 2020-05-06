using Awesomni.Codes.FlowRx;
using ImpromptuInterface;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace FlowRx.Dynamic
{
    public class DataDynamicAccessOnlyExisting : DynamicObject
    {
        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _properties.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _properties[binder.Name] = value;
            return true;
        }
    }
}
