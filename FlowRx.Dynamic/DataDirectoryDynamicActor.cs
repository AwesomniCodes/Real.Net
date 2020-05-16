// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataDirectoryDynamicActor.cs" project="FlowRx.Dynamic" solution="FlowRx" />
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
    internal class DataDirectoryDynamicActor<TKey> : DynamicObject, IDataDynamicActor
    {
        private readonly IDataDirectory<TKey> _directory;
        private readonly SyntaxOptions _syntaxOptions;

        internal DataDirectoryDynamicActor(IDataDirectory<TKey> directory, SyntaxOptions syntaxOptions)
        {
            _directory = directory;
            _syntaxOptions = syntaxOptions;
        }


        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            var dataObject = ResolveMember(binder.Name.Convert<TKey>(), binder.ReturnType); //TODO - type has to be determined in non basememberaccess

            result = (dataObject is IDataObject dataDirectory) ? dataDirectory.AsDynamic() : dataObject;
            //result = (dataObject is IDataDirectory<object> dataDirectory) ? dataDirectory.AsDynamic() : dataObject;

            return result != null;
        }

        private IDataObject? ResolveMember(TKey name, Type type)
            => _syntaxOptions.HasFlag(SyntaxOptions.AutoCreate)
                ? _directory.GetOrAdd(name, () => DataObject.Create(type))
                : _directory.Get(name);

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (value is IDataObject dataObject)
            {
                _directory.Remove(binder.Name.Convert<TKey>());
                _directory.Add(binder.Name.Convert<TKey>(), dataObject);
                return true;
            }

            return false;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (typeof(IDataDirectory<TKey>).IsAssignableFrom(binder.Type))
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
