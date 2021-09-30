using System;

namespace EntityCache.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        ///     Friendly wrapper for <see cref="Type.IsAssignableFrom" />
        /// </summary>
        public static bool IsAssignableTo<TOtherType>(this Type thisType)
        {
            if (thisType == null)
            {
                throw new ArgumentNullException(nameof(thisType));
            }

            return typeof(TOtherType).IsAssignableFrom(thisType);
        }
    }
}