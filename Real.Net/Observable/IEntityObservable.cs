﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IEntityObservable.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public interface IEntityObservable<TValue> : IEntity, IObservable<TValue>
    {
    }
}