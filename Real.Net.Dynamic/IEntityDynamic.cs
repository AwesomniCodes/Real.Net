// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IEntityDynamic.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public interface IEntityDynamic<TInterface> : IEntityDirectory<string> where TInterface : class
    {
        TInterface Value { get; }
    }
}