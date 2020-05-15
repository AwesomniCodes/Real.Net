// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataObject.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.FlowRx
{
    using Awesomni.Codes.FlowRx.Utility;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;

    public abstract class DataObject : IDataObject
    {
        public abstract ISubject<IEnumerable<IChange>> Changes { get; }

        public static Dictionary<Type, Type> interfaceToClassTypeMap { get; }
            = new Dictionary<Type, Type>
                {
                    { typeof(IDataDirectory<>), typeof(DataDirectory<>) },
                    { typeof(IDataList<>), typeof(DataList<>) },
                    { typeof(IDataDictionary<,>), typeof(DataDictionary<,>) },
                    { typeof(IDataObservable<>), typeof(DataObservable<>) },
                    { typeof(IDataItem<>), typeof(DataItem<>) },
                };

        private static IDataObject InvokeGenericCreation(Type dataObjectGenericDefinition, Type[] genericSubtypes, params object?[] arguments)
        => (IDataObject)dataObjectGenericDefinition
            .MakeGenericType(genericSubtypes)
            .GetMethod(nameof(Create), BindingFlags.Static | BindingFlags.Public)
            .Invoke(null, arguments);

        public static IDataObject Create(Type objectType, params object?[] constructorArgs)
        {
            var genericArguments = objectType.GetGenericArguments();
            var genericDefinition = objectType.GetGenericTypeDefinition();
            
            return typeof(IDataObject).IsAssignableFrom(genericArguments[0])
                    ? Create(genericArguments[0], new object[] { } )
                    : InvokeGenericCreation(interfaceToClassTypeMap[objectType.GetGenericTypeDefinition()], objectType.GetGenericArguments(), constructorArgs)
                        ?? throw new ArgumentException("The type is unknown", nameof(objectType));
        }
    }
}