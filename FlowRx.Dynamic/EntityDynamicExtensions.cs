using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Dynamic.Actors;
using ImpromptuInterface;
using System;
using System.Dynamic;

namespace Awesomni.Codes.FlowRx.Dynamic
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
            => new EntityValueDynamicActor<TValue>(subject, syntaxOptions);

        public static dynamic AsDynamic<TValue>(this IEntityObservable<TValue> observable, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess)
            => new EntityObservableDynamicActor<TValue>(observable, syntaxOptions);
    }
}
