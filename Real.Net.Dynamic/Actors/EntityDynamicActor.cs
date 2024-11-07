// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityDynamicActors.cs" project="Real.Net.Dynamic" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

using Awesomni.Codes.Real.Net;
using Awesomni.Codes.Real.Net.Utility;
using ImpromptuInterface;
using ImpromptuInterface.Optimization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Reflection;

namespace Awesomni.Codes.Real.Net.Dynamic.Actors
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

        public ISubject<IEnumerable<IChange>> Changes => _entity.Changes;

        //Try invocation on actual object as default strategy
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[]? args, out object? result)
        {
            if(binder.Name == "Entity")
            {
                result = _entity;
                return true;
            }

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
