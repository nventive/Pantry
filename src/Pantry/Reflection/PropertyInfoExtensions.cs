using System;
using System.Linq;
using System.Reflection;

namespace Pantry.Reflection
{
    /// <summary>
    /// <see cref="PropertyInfo"/> extension methods.
    /// </summary>
    public static class PropertyInfoExtensions
    {
        /// <summary>
        /// Like <see cref="PropertyInfo.SetValue(object, object)"/>, but attempts to use
        /// casting operators in both source and target if they exists.
        /// </summary>
        /// <param name="property">The <see cref="PropertyInfo"/>.</param>
        /// <param name="target">The object whose property value will be set.</param>
        /// <param name="value">The new property value.</param>
        public static void SetValueWithCastOperators(this PropertyInfo property, object target, object value)
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (value is null)
            {
                property.SetValue(target, null);
                return;
            }

            var srcType = value.GetType();
            var destType = property.PropertyType;
            if (srcType == destType)
            {
                property.SetValue(target, value);
                return;
            }

            var bf = BindingFlags.Static | BindingFlags.Public;
            var castOperator = destType.GetMethods(bf)
                .Union(srcType.GetMethods(bf))
                .Where(mi => mi.Name == "op_Explicit" || mi.Name == "op_Implicit")
                .Where(mi =>
                {
                    var pars = mi.GetParameters();
                    return pars.Length == 1 && pars[0].ParameterType == srcType;
                })
                .Where(mi => mi.ReturnType == destType)
                .FirstOrDefault();
            if (castOperator != null)
            {
                value = castOperator.Invoke(null, new object[] { value });
            }

            property.SetValue(target, value);
        }
    }
}
