// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityObservableDynamicActor.cs" project="Real.Net.Dynamic" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

using Awesomni.Codes.Real.Net;
using Awesomni.Codes.Real.Net.Utility;
using ImpromptuInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Awesomni.Codes.Real.Net.Dynamic.Actors
{
    internal class EntityObservableDynamicActor<TValue> : EntityDynamicActor, IEntityDynamicActor
    {
        internal EntityObservableDynamicActor(IEntityObservable<TValue> observable, SyntaxOptions syntaxOptions) : base(observable, syntaxOptions)
        {
            //TODO iterate over T properties and fill Expando property and Directory
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
