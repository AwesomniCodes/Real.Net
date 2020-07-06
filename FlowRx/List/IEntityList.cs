// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="IEntityList.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public interface IEntityList<TEntityObject> : IEntity, IEnumerable<TEntityObject>, ICollection<TEntityObject>, IList<TEntityObject>, IReadOnlyCollection<TEntityObject> where TEntityObject : class, IEntity
    {
        new TEntityObject this[int index] { get; set; }
    }
}