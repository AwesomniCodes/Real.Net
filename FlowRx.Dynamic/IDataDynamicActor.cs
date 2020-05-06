// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IDataDynamicActor.cs" project="FlowRx.Dynamic" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Reactive.Subjects;

    public interface IDataDynamicActor : IDynamicMetaObjectProvider
    {

    }

    public interface IDataDynamicActor<T> : IDataDynamicActor where T : class
    {
        
    }
}