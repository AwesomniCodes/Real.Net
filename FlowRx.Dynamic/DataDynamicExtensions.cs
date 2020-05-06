using Awesomni.Codes.FlowRx;
using ImpromptuInterface;
using System;
using System.Dynamic;

namespace FlowRx.Dynamic
{
    public static class DataDynamicExtensions
    {

        public static dynamic AsDynamic(this IDataObject @object, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess)
        {
            switch (@object)
            {
                case IDataDirectory<object> directory:
                    return directory.AsDynamic(syntaxOptions);
                case IDataItem<object?> item:
                    return item.AsDynamic(syntaxOptions);
                case IDataList<IDataObject> list:
                    return list.AsDynamic(syntaxOptions);
                case IDataDictionary<object, IDataObject> dictionary:
                    return dictionary.AsDynamic(syntaxOptions);
                case IDataObservable observable:
                    return observable.AsDynamic(syntaxOptions);
                default:
                    throw new ArgumentException("Unknown IDataObject provided");
            }
        }

        public static dynamic AsDynamic<TKey>(this IDataDirectory<TKey> directory, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess)
            => new DataDirectoryDynamicActor<TKey>(directory, syntaxOptions);

        public static dynamic AsDynamic<TDataObject>(this IDataList<TDataObject> list, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess) where TDataObject : class, IDataObject
            => new DataListDynamicActor<TDataObject>(list, syntaxOptions);

        public static dynamic AsDictionary<TKey, TDataObject>(this IDataDictionary<TKey, TDataObject> dictionary, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess) where TDataObject : class, IDataObject
            => new DataDictionaryDynamicActor<TKey, TDataObject>(dictionary, syntaxOptions);

        public static dynamic AsDynamic<T>(this IDataItem<T> item, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess)
            => new DataItemDynamicActor<T>(item, syntaxOptions);

        public static dynamic AsDynamic(this IDataObservable observable, SyntaxOptions syntaxOptions = SyntaxOptions.DefaultAccess)
            => new DataObservableDynamicActor(observable, syntaxOptions);
    }
}
