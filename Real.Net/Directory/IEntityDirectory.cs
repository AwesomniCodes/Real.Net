﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2024" holder="Awesomni.Codes" author="Felix Keil" contact="felix.keil@awesomni.codes"
//    file="IEntityDirectory.cs" project="Real.Net" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.Real.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public interface IEntityDirectory<TKey> : IEntityDictionary<TKey, IEntity>
    {
        QEntity GetOrAdd<QEntity>(TKey key, Func<QEntity> creator) where QEntity : class, IEntity
        {
            QEntity CreateAndAdd()
            {
                var obj = creator();
                Add(key, obj);
                return obj;
            }
            return Get<QEntity>(key) ?? CreateAndAdd();
        }

        QEntity? Get<QEntity>(TKey key) where QEntity : class, IEntity
            => Get(key) as QEntity;
    }
}