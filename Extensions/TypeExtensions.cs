using System;
using System.Reflection;
using System.Text;

namespace ServiceFabric.Serialization.V2.Extensions
{
    public static class TypeExtensions
    {
        public static int ComputeId(this MethodInfo method)
        {
            var hashCode = method.Name.GetHashCode();

            if (method.DeclaringType == null)
            {
                return hashCode;
            }

            if (method.DeclaringType.Namespace != null)
            {
                hashCode = HashCombine(method.DeclaringType.Namespace.GetHashCode(), hashCode);
            }

            hashCode = HashCombine(method.DeclaringType.Name.GetHashCode(), hashCode);

            return hashCode;
        }

        public static int ComputeIdWithCRC(this MethodInfo method)
        {
            var name = method.Name;

            if (method.DeclaringType == null)
            {
                return ComputeIdWithCRC(name);
            }

            if (method.DeclaringType.Namespace != null)
            {
                name = string.Concat(method.DeclaringType.Namespace, name);
            }

            name = string.Concat(method.DeclaringType.Name, name);

            return ComputeIdWithCRC(name);
        }

        public static int ComputeId(this Type type)
        {
            var hashCode = type.Name.GetHashCode();

            if (type.Namespace != null)
            {
                hashCode = HashCombine(type.Namespace.GetHashCode(), hashCode);
            }

            return hashCode;
        }

        public static int ComputeIdWithCRC(this Type type)
        {
            var name = type.Name;

            if (type.Namespace != null)
            {
                name = string.Concat(type.Namespace, name);
            }

            return ComputeIdWithCRC(name);
        }

        #region Private Methods

        private static int HashCombine(int hash1, int hash2)
        {
            return (hash2 * -1521134295) + hash1;
        }

        private static int ComputeIdWithCRC(string name)
        {
            return (int)CRC64.ToCRC64(Encoding.UTF8.GetBytes(name));
        }

        #endregion Private Methods
    }
}