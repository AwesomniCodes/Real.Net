using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowRx.Utility.Mvvm.DynamicProperties
{
    public class DynamicTypeDescriptionProvider : TypeDescriptionProvider
    {
        private readonly List<PropertyDescriptor> _properties = new List<PropertyDescriptor>();
        private readonly TypeDescriptionProvider _provider;

        public DynamicTypeDescriptionProvider(Type type)
        {
            _provider = TypeDescriptor.GetProvider(type);
        }

        public IList<PropertyDescriptor> Properties => _properties;

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            => new DynamicCustomTypeDescriptor(this, _provider.GetTypeDescriptor(objectType, instance));

        private class DynamicCustomTypeDescriptor : CustomTypeDescriptor
        {
            private readonly DynamicTypeDescriptionProvider provider;

            public DynamicCustomTypeDescriptor(DynamicTypeDescriptionProvider provider,
               ICustomTypeDescriptor descriptor)
                  : base(descriptor)
            {
                this.provider = provider;
            }

            public override PropertyDescriptorCollection GetProperties() => GetProperties(null);

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                var properties = new PropertyDescriptorCollection(null);

                foreach (PropertyDescriptor property in base.GetProperties(attributes))
                {
                    properties.Add(property);
                }

                foreach (PropertyDescriptor property in provider.Properties)
                {
                    properties.Add(property);
                }
                return properties;
            }
        }
    }
}