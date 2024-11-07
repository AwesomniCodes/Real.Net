// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IEntityDynamicActor.cs" project="FlowRx.Dynamic" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx.Dynamic.Actors
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Reactive.Subjects;

    public interface IEntityDynamicActor : IDynamicMetaObjectProvider, IEntity
    {

    }

    public interface IEntityDynamicActor<T> : IEntityDynamicActor where T : class
    {
        
    }
}