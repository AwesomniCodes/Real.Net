// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityDictionaryDynamicActor.cs" project="FlowRx.Dynamic" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Utility;
using ImpromptuInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Awesomni.Codes.FlowRx.Dynamic.Actors
{
    internal class EntityDictionaryDynamicActor<TKey, TEntity> : EntityDynamicActor, IEntityDynamicActor where TEntity : class, IEntity
    {
        private readonly IEntityDictionary<TKey, TEntity> _dictionary;
        internal EntityDictionaryDynamicActor(IEntityDictionary<TKey, TEntity> dictionary, SyntaxOptions syntaxOptions) : base(dictionary, syntaxOptions)
        {
            _dictionary = dictionary;
            //TODO iterate over T properties and fill Expando property and Directory
        }


        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            var name = binder.Name.Convert<TKey>();
            var type = binder.ReturnType;

            result = (_syntaxOptions.HasFlag(SyntaxOptions.AutoCreate)
                ? typeof(TEntity).IsAssignableFrom(type)
                    ? _dictionary.GetOrAdd(name, () => (TEntity) Entity.Create(type))
                    : _dictionary.GetOrAdd(name, () => (TEntity) Entity.Create(typeof(IEntitySubject<>).MakeGenericType(type)))
                : _dictionary.Get(name))
                ?.AsDynamic();
            return result != null;
        }


        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var key = binder.Name.Convert<TKey>();
            if (value is TEntity entity)
            {
                _dictionary.Remove(key);
                _dictionary.Add(key, entity);
                return true;
            }
            else
            {
                var maybeEntity = _dictionary.Get(key);
                if (maybeEntity == null)
                {
                    _dictionary.Add(key, (TEntity) Entity.Create(typeof(IEntitySubject<>).MakeGenericType(value.GetType()), value));
                    return true;
                }
                else if (maybeEntity is IEntitySubject<object> entityValue)
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
                result = _dictionary;
                return true;
            }

            //simple version
            result = _dictionary;
            return true;
            //return base.TryConvert(binder, out result);
        }
    }
}
