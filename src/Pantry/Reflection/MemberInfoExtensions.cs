using System;
using System.Collections;
using System.Reflection;

namespace Pantry.Reflection
{
    /// <summary>
    /// <see cref="MemberInfo"/> extension methods.
    /// </summary>
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Return true if the <paramref name="memberInfo"/> underlying type is <see cref="IEnumerable"/>, but not a <see cref="string"/>.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/>.</param>
        /// <returns>true if it is <see cref="IEnumerable"/> and not a <see cref="string"/>, false otherwise.</returns>
        public static bool IsEnumerableAndNotString(this MemberInfo memberInfo)
        {
            var underlyingType = memberInfo.GetUnderlyingType();
            return underlyingType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(underlyingType);
        }

        /// <summary>
        /// Returns the underlying type behing <paramref name="memberInfo"/>.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/>.</param>
        /// <returns>The underlying type.</returns>
        public static Type GetUnderlyingType(this MemberInfo memberInfo)
        {
            if (memberInfo is null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            return memberInfo.MemberType switch
            {
                MemberTypes.Event => ((EventInfo)memberInfo).EventHandlerType,
                MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
                MemberTypes.Method => ((MethodInfo)memberInfo).ReturnType,
                MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
                _ => throw new ArgumentException($"{memberInfo} must be of type {nameof(EventInfo)}, {nameof(FieldInfo)}, {nameof(MethodInfo)}, or {nameof(PropertyInfo)}."),
            };
        }
    }
}
