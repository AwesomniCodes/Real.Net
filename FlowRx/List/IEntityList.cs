﻿// --------------------------------------------------------------------------------------------------------------------
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

    public interface IEntityList<TEntity> : IEntity, IEnumerable<TEntity>, ICollection<TEntity>, IList<TEntity>, IReadOnlyCollection<TEntity> where TEntity : class, IEntity
    {
        new TEntity this[int index] { get; set; }
    }
}