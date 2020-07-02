using Awesomni.Codes.FlowRx;
using ImpromptuInterface;
using System;
using System.Dynamic;

namespace FlowRx.Dynamic
{
    public static class EntityDynamicExtensions
    {

        public static dynamic AsDynamic<TEntity>(this TEntity entity, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess) where TEntity : IEntity
        {
            switch (entity)
            {
                case IEntityDirectory<object> directory:
                    return directory.AsDynamic(syntaxOptions);
                case IEntityValue<object?> value:
                    return value.AsDynamic(syntaxOptions);
                case IEntityList<IEntity> list:
                    return list.AsDynamic(syntaxOptions);
                case IEntityDictionary<object, IEntity> dictionary:
                    return dictionary.AsDynamic(syntaxOptions);
                case IEntityObservable<object> observable:
                    return observable.AsDynamic(syntaxOptions);
                default:
                    throw new ArgumentException($"Unknown {nameof(IEntity)} provided");
            }
        }

        public static dynamic AsDynamic<TKey>(this IEntityDirectory<TKey> directory, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess)
            => new EntityDirectoryDynamicActor<TKey>(directory, syntaxOptions);

        public static dynamic AsDynamic<TEntity>(this IEntityList<TEntity> list, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess) where TEntity : class, IEntity
            => new EntityListDynamicActor<TEntity>(list, syntaxOptions);

        public static dynamic AsDictionary<TKey, TEntity>(this IEntityDictionary<TKey, TEntity> dictionary, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess) where TEntity : class, IEntity
            => new EntityDictionaryDynamicActor<TKey, TEntity>(dictionary, syntaxOptions);

        public static dynamic AsDynamic<TValue>(this IEntityValue<TValue> value, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess)
            => new EntityValueDynamicActor<TValue>(value, syntaxOptions);

        public static dynamic AsDynamic<TValue>(this IEntityObservable<TValue> observable, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess)
            => new EntityObservableDynamicActor<TValue>(observable, syntaxOptions);
    }
}
