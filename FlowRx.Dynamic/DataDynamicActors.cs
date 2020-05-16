// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataDynamicActors.cs" project="FlowRx.Dynamic" solution="FlowRx" />
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

    internal class DataListDynamicActor<TDataObject> : DynamicObject, IDataDynamicActor where TDataObject : class, IDataObject
    {
        internal DataListDynamicActor(IDataList<TDataObject> list, SyntaxOptions syntaxOptions)
        {
            //TODO iterate over T properties and fill Expando property and Directory
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            throw new NotImplementedException();
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            throw new NotImplementedException();

        }
    }

    internal class DataItemDynamicActor<TData> : DynamicObject, IDataDynamicActor
    {
        internal DataItemDynamicActor(IDataItem<TData> item, SyntaxOptions syntaxOptions)
        {
            //TODO iterate over T properties and fill Expando property and Directory
        }

    }

    internal class DataDictionaryDynamicActor<TKey, TDataObject> : DynamicObject, IDataDynamicActor where TDataObject : class, IDataObject
    {
        internal DataDictionaryDynamicActor(IDataDictionary<TKey, TDataObject> dictionary, SyntaxOptions syntaxOptions)
        {
            //TODO iterate over T properties and fill Expando property and Directory
        }

    }

    internal class DataObservableDynamicActor<TData> : DynamicObject, IDataDynamicActor
    {
        internal DataObservableDynamicActor(IDataObservable<TData> observable, SyntaxOptions syntaxOptions)
        {
            //TODO iterate over T properties and fill Expando property and Directory
        }

    }
}
