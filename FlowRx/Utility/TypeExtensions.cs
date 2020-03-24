using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awesomni.Codes.FlowRx.Utility
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets The Default Value for this Type
        /// </summary>
        public static object? GetDefault(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static Type? GetTypeIfImplemented(this Type type, Type interfaceType)
        {
            if (interfaceType.IsGenericType && !interfaceType.IsConstructedGenericType)
            {
                return type.GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType)
                    .OrderByDescending(x => x.GetGenericArguments().Single().IsGenericType)
                    .FirstOrDefault();
            }

            return type.GetInterfaces().FirstOrDefault(x => x == interfaceType);
        }

        public static T NullThrow<T>(this T? value, string paramName = "") where T : class
            => value != null ? value : throw new ArgumentNullException("The value must not be null in this case");

        public static T NullSubstitute<T>(this T? value, T substitute) where T : class
            => value ?? substitute;
    }
}
