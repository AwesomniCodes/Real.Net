// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityDynamicActors.cs" project="FlowRx.Dynamic" solution="FlowRx" />
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
    internal abstract class EntityDynamicActor : DynamicObject, IEntityDynamicActor
    {
        protected readonly IEntity _entity;
        protected readonly SyntaxOptions _syntaxOptions;

        internal EntityDynamicActor(IEntity entity, SyntaxOptions syntaxOptions)
        {
            _entity = entity;
            _syntaxOptions = syntaxOptions;
            //TODO iterate over T properties and fill Expando property and Directory
        }

        //Try invocation on actual object as default strategy
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[]? args, out object? result)
        {
            try
            {
                result = _entity.GetType().InvokeMember(binder.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, _entity, args);
                return true;
            }
            catch (MissingMethodException)
            {
                result = null;
                return false;
            }
        }
    }
}
