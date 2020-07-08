// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityDynamicDynamicActor.cs" project="FlowRx.Dynamic" solution="FlowRx" />
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
    internal class EntityDynamicDynamicActor<TInterface> : EntityDynamicActor, IEntityDynamicActor where TInterface : class
    {
        private readonly IDictionary<string, PropertyInfo> _properties;
        private readonly IEntityDynamic<TInterface> _dynamic;

        internal EntityDynamicDynamicActor(IEntityDynamic<TInterface> entityDynamic, SyntaxOptions syntaxOptions) : base(entityDynamic, syntaxOptions)
        {
            _dynamic = entityDynamic;
            _properties = entityDynamic.GetType().GetGenericArguments().First().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(property => property.Name);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = null;

            var name = binder.Name;
            var propertyInfo = _properties[name];

            var entity = _dynamic.Get(binder.Name);

            if(entity == null)
            {
                return false;
            }

            if (propertyInfo.PropertyType.IsGenericType)
            {

                if (propertyInfo.PropertyType.IsAssignableFrom(entity.GetType()))
                {
                    result = entity;
                    return true;
                }
            }

            if (entity is IEntitySubject<object> entityValue)
            {
                if(entityValue.GetType().GetGenericArguments().First() == propertyInfo.PropertyType)
                {
                    result = entityValue.Value;
                    return true;
                }
            }

            result = propertyInfo.PropertyType.IsInterface ? entity.AsDynamic() : null;
            return result != null;
        }


        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var key = binder.Name;
            if (value is IEntity entity)
            {
                _dynamic.Remove(key);
                _dynamic.Add(key, entity);
                return true;
            }
            else
            {
                var maybeEntity = _dynamic.Get(key);
                if (maybeEntity == null)
                {
                    _dynamic.Add(key, Entity.Create(typeof(IEntitySubject<>).MakeGenericType(value.GetType()), value));
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
    }
}
