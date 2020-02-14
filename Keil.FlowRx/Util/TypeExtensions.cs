using System;
using System.Collections.Generic;
using System.Text;

namespace Keil.FlowRx.Utility.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets The Default Value for this Type
        /// </summary>
        public static object GetDefault(this Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
    }
}
