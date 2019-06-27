using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowRx.Utility.Mvvm.DynamicProperties
{
    public class DynamicPropertyDescriptor<TTarget, TProperty> : PropertyDescriptor
    {
        private readonly Func<TTarget, TProperty> _getter;
        private readonly string _propertyName;
        private readonly Action<TTarget, TProperty> _setter;

        public DynamicPropertyDescriptor(
           string propertyName,
           Func<TTarget, TProperty> getter,
           Action<TTarget, TProperty> setter,
           Attribute[] attributes)
              : base(propertyName, attributes ?? new Attribute[] { })
        {
            _setter = setter;
            _getter = getter;
            _propertyName = propertyName;
        }

        public override Type ComponentType => typeof(TTarget);

        public override bool IsReadOnly => _setter == null;

        public override Type PropertyType => typeof(TProperty);

        public override bool CanResetValue(object component) => true;

        public override bool Equals(object obj) => obj is DynamicPropertyDescriptor<TTarget, TProperty> o && o._propertyName.Equals(_propertyName);

        public override int GetHashCode() => _propertyName.GetHashCode();

        public override object GetValue(object component) => _getter((TTarget)component);

        public override void ResetValue(object component) { }

        public override void SetValue(object component, object value) => _setter((TTarget)component, (TProperty)value);

        public override bool ShouldSerializeValue(object component) => true;
    }
}