using Awesomni.Codes.Real.Net;
using Awesomni.Codes.Real.Net.Dynamic.Actors;
using Awesomni.Codes.Real.Net.Utility;
using ImpromptuInterface;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reactive.Subjects;

namespace Awesomni.Codes.Real.Net.Dynamic
{
    public static class EntityDynamicExtensions
    {

        public static dynamic AsDynamic<TEntity>(this TEntity entity, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess) where TEntity : IEntity =>
            entity switch
            {
                IEntityDynamic<object> dynamicEntity => dynamicEntity.AsDynamic(syntaxOptions),
                IEntityDirectory<object> directory => directory.AsDynamic(syntaxOptions),
                IEntityDictionary<object, IEntity> dictionary => dictionary.AsDynamic(syntaxOptions),
                IEntityList<IEntity> list => list.AsDynamic(syntaxOptions),
                IEntitySubject<object?> subject => subject.AsDynamic(syntaxOptions),
                IEntityObservable<object> observable => observable.AsDynamic(syntaxOptions),
                _ => throw new ArgumentException($"Unknown {nameof(IEntity)} provided")
            };

        public static dynamic AsDynamic<TInterface>(this IEntityDynamic<TInterface> dynamicEntity, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess) where TInterface : class
            => new EntityDynamicDynamicActor<TInterface>(dynamicEntity, syntaxOptions);
        public static dynamic AsDynamic<TKey>(this IEntityDirectory<TKey> directory, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess)
            => new EntityDirectoryDynamicActor<TKey>(directory, syntaxOptions);

        public static dynamic AsDynamic<TEntity>(this IEntityList<TEntity> list, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess) where TEntity : class, IEntity
            => new EntityListDynamicActor<TEntity>(list, syntaxOptions);

        public static dynamic AsDynamic<TKey, TEntity>(this IEntityDictionary<TKey, TEntity> dictionary, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess) where TEntity : class, IEntity
            => new EntityDictionaryDynamicActor<TKey, TEntity>(dictionary, syntaxOptions);

        public static dynamic AsDynamic<TValue>(this IEntitySubject<TValue> subject, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess)
            => new EntitySubjectDynamicActor<TValue>(subject, syntaxOptions);

        public static dynamic AsDynamic<TValue>(this IEntityObservable<TValue> observable, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess)
            => new EntityObservableDynamicActor<TValue>(observable, syntaxOptions);


        public static object? TryCreateEntityImplementation(this Type type)
        {
            return type.TryCreate() ?? (type.IsInterface ? type.GetImplementation(type.GetEntityBackend()) : null);
        }


        public static object GetImplementation(this Type type, IEntity entity)
        {
            var genType = type.IsGenericType
                ? (Definition: type.GetGenericTypeDefinition(), Arguments: type.GetGenericArguments())
                : (Definition: type, Arguments: new Type[] { });

            if (typeof(IEntity).IsAssignableFrom(type)
                || genType.Definition.IsAssignableFrom(typeof(ISubject<>)) 
                || genType.Definition.IsAssignableFrom(typeof(IObservable<>))
                )
            {
                return entity;
            }

            if (type.IsGenericType)
            {

                if (genType.Definition.IsAssignableFrom(typeof(IList<>)))
                {
                    return typeof(EntityListExtension)
                        .GetMethod(nameof(EntityListExtension.AsValueList))
                        .MakeGenericMethod(genType.Arguments)
                        .Invoke(null, new[] { entity });
                }

                if (genType.Definition.IsAssignableFrom(typeof(IDictionary<,>)))
                {
                    return typeof(EntityDictionaryExtension)
                        .GetMethod(nameof(EntityDictionaryExtension.AsValueDictionary))
                        .MakeGenericMethod(genType.Arguments)
                        .Invoke(null, new[] { entity });
                }
            }
            if (type.IsInterface)
            {
                return ((IEntityDynamic<object>)entity).Value;
            }

            return ((IEntitySubject<object>)entity).Value;
        }

        public static IEntity GetEntityBackend(this Type type)
        {
            var genType = type.IsGenericType
                ? (Definition: type.GetGenericTypeDefinition(), Arguments: type.GetGenericArguments())
                : (Definition: type, Arguments: new Type[] { });

            if (typeof(IEntity).IsAssignableFrom(type))
            {
                return genType.Definition.IsAssignableFrom(typeof(IEntitySubject<>))
                    ? Entity.Create(type, TryCreateEntityImplementation(genType.Arguments[0]))
                    : Entity.Create(type);
            }

            if (type.IsGenericType)
            {
                if (genType.Definition.IsAssignableFrom(typeof(ISubject<>)))
                {
                    return Entity.Create(typeof(IEntitySubject<>).MakeGenericType(genType.Arguments), TryCreateEntityImplementation(genType.Arguments[0]));
                }
                else if (genType.Definition.IsAssignableFrom(typeof(IObservable<>)))
                {
                    return Entity.Create(typeof(IEntityObservable<>).MakeGenericType(genType.Arguments), TryCreateEntityImplementation(genType.Arguments[0]));
                }
                else if (genType.Definition.IsAssignableFrom(typeof(IDictionary<,>)))
                {
                    return Entity.Create(typeof(IEntityDictionary<,>).MakeGenericType(genType.Arguments[0], typeof(IEntitySubject<>).MakeGenericType(genType.Arguments[1])));
                }
                else if (genType.Definition.IsAssignableFrom(typeof(IList<>)))
                {
                    return Entity.Create(typeof(IEntityList<>).MakeGenericType(typeof(IEntitySubject<>).MakeGenericType(genType.Arguments)));
                }
            }
            else if (type.IsInterface)
            {
                return Entity.Create(typeof(IEntityDynamic<>).MakeGenericType(type));
            }

            //throw new InvalidOperationException($"The type {type} cannot be backed by an entity");

            //Common base value gets added as a EntitySubject
            return Entity.Create(typeof(IEntitySubject<>).MakeGenericType(type), type.TryCreate());
        }
    }
}
