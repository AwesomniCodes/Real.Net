// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2024" holder="Awesomni.Codes" author="Felix Keil" contact="felix.keil@awesomni.codes"
//    file="IEntityDynamicActor.cs" project="Real.Net.Dynamic" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.Real.Net.Dynamic.Actors
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