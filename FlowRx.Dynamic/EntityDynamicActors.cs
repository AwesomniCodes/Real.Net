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
using System.Linq.Expressions;

namespace FlowRx.Dynamic
{

    internal class EntityListDynamicActor<TEntity> : DynamicObject, IEntityDynamicActor where TEntity : class, IEntity
    {
        internal EntityListDynamicActor(IEntityList<TEntity> list, SyntaxOptions syntaxOptions)
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

    internal class EntityValueDynamicActor<TValue> : DynamicObject, IEntityDynamicActor
    {
        internal EntityValueDynamicActor(IEntityValue<TValue> value, SyntaxOptions syntaxOptions)
        {
            //TODO iterate over T properties and fill Expando property and Directory
        }

    }

    internal class EntityDictionaryDynamicActor<TKey, TEntity> : DynamicObject, IEntityDynamicActor where TEntity : class, IEntity
    {
        internal EntityDictionaryDynamicActor(IEntityDictionary<TKey, TEntity> dictionary, SyntaxOptions syntaxOptions)
        {
            //TODO iterate over T properties and fill Expando property and Directory
        }

    }

    internal class EntityObservableDynamicActor<TValue> : DynamicObject, IEntityDynamicActor
    {
        internal EntityObservableDynamicActor(IEntityObservable<TValue> observable, SyntaxOptions syntaxOptions)
        {
            //TODO iterate over T properties and fill Expando property and Directory
        }

    }
}
