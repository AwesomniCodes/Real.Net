using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Awesomni.Codes.Real.Net.Utility
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets The Default Value for this Type
        /// </summary>
        public static object? GetDefault(this Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;


        /// <summary>
        /// Gets The Default Value for this Type
        /// </summary>
        public static object? TryCreate(this Type type)
        {
            return type.GetDefault() ?? type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
        }

        public static IEnumerable<Type> GetTypesIfImplemented(this Type type, Type interfaceType)
        {
            if (interfaceType.IsGenericType && !interfaceType.IsConstructedGenericType)
            {
                return type.GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType)
                    .OrderByDescending(x => x.GetGenericArguments().Single().IsGenericType);
            }

            return type.GetInterfaces().Where(x => x == interfaceType);
        }

        public static T NullThrow<T>(this T? value, string paramName = "") where T : class
            => value != null ? value : throw new ArgumentNullException("The value must not be null in this case");

        public static T NullSubstitute<T>(this T? value, T substitute) where T : class
            => value ?? substitute;

        public static T Convert<T>(this string input)
        {
            if(input is T tInput)
            {
                return tInput;
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                return (T)converter.ConvertFromString(input);
            }

            throw new NotSupportedException();
        }
    }
}
