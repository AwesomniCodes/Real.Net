// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityDirectoryDynamicActor.cs" project="FlowRx.Dynamic" solution="FlowRx" />
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
    internal class EntityDirectoryDynamicActor<TKey> : DynamicObject, IEntityDynamicActor
    {
        private readonly IEntityDirectory<TKey> _directory;
        private readonly SyntaxOptions _syntaxOptions;

        internal EntityDirectoryDynamicActor(IEntityDirectory<TKey> directory, SyntaxOptions syntaxOptions)
        {
            _directory = directory;
            _syntaxOptions = syntaxOptions;
        }


        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            var name = binder.Name.Convert<TKey>();
            var type = binder.ReturnType;

            result = (_syntaxOptions.HasFlag(SyntaxOptions.AutoCreate)
                ? typeof(IEntity).IsAssignableFrom(type)
                    ? _directory.GetOrAdd(name, () => Entity.Create(type))
                    : _directory.GetOrAdd(name, () => Entity.Create(typeof(IEntityValue<>).MakeGenericType(type)))
                : _directory.Get(name))
                ?.AsDynamic();
            return result != null;
        }


        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var key = binder.Name.Convert<TKey>();
            if (value is IEntity entity)
            {
                _directory.Remove(key);
                _directory.Add(key, entity);
                return true;
            }
            else
            {
                var maybeEntity = _directory.Get(key);
                if (maybeEntity == null)
                {
                    _directory.Add(key, Entity.Create(typeof(IEntityValue<>).MakeGenericType(value.GetType()), value));
                    return true;
                }
                else if (maybeEntity is IEntityValue<object> entityValue)
                {
                    entityValue.OnNext(value);
                    return true;
                }
            }

            return false;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (typeof(IEntityDirectory<TKey>).IsAssignableFrom(binder.Type))
            {
                result = _directory;
                return true;
            }

            //simple version
            result = _directory;
            return true;
            //return base.TryConvert(binder, out result);
        }
    }
}
