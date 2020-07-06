// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityValueDynamicActor.cs" project="FlowRx.Dynamic" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Utility;
using ImpromptuInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Awesomni.Codes.FlowRx.Dynamic.Actors
{
    internal class EntityValueDynamicActor<TValue> : EntityDynamicActor, IEntityDynamicActor
    {
        private static readonly IDictionary<string, PropertyInfo> _properties;
        private static readonly IDictionary<string, MethodInfo> _methods;
        private readonly IEntityValue<TValue> _value;

        internal EntityValueDynamicActor(IEntityValue<TValue> value, SyntaxOptions syntaxOptions) : base(value, syntaxOptions)
        {
            _value = value;
            //TODO iterate over T properties and fill Expando property and Directory
        }

        static EntityValueDynamicActor()
        {
            _properties = typeof(IEntityValue<>).GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(property => property.Name);
            _methods = typeof(IEntityValue<>).GetMethods(BindingFlags.Instance | BindingFlags.Public).ToDictionary(method => method.Name);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            throw new NotImplementedException();
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            throw new NotImplementedException();
        }
    }
}
