﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2024" holder="Awesomni.Codes" author="Felix Keil" contact="felix.keil@awesomni.codes"
//    file="ChangeSubject.cs" project="Real.Net" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.Real.Net
{
    using System;
    using System.Collections.Generic;

    public interface IChangeSubject<TValue> : IChange<IEntitySubject<TValue>>
    {
        ChangeType ChangeType { get; }
        TValue Value { get; }
    }

    public abstract class ChangeSubject : IChangeSubject<object?>
    {
        protected ChangeSubject(ChangeType changeType, object? value = null)
        {
            ChangeType = changeType;
            Value = value;
        }

        public ChangeType ChangeType { get; }

        public object? Value { get; }
    }

    public class ChangeSubject<TValue> : ChangeSubject, IChangeSubject<TValue>
    {
        public static IChangeSubject<TValue> Create(ChangeType changeType, TValue value = default)
            => new ChangeSubject<TValue>(changeType, value);

        protected ChangeSubject(ChangeType changeType, TValue value = default) : base(changeType, value) { }

        public new TValue Value => base.Value is TValue tValue ? tValue : default!;

    }
}