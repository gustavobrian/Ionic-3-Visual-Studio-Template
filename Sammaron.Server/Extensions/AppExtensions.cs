using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Sammaron.Server.Extensions
{
    public static class AppExtensions
    {
        public static bool HasAttribute<TAttribute>(this Type type)
        {
            return type.CustomAttributes.Any(e => e.AttributeType == typeof(TAttribute));
        }

        public static bool HasAttribute<TAttribute>(this MethodInfo type)
        {
            return type.CustomAttributes.Any(e => e.AttributeType == typeof(TAttribute));
        }

        public static bool HasAttribute<TAttribute>(this ControllerActionDescriptor actionDescriptor)
        {
            return actionDescriptor.MethodInfo.HasAttribute<TAttribute>()
                   || actionDescriptor.ControllerTypeInfo.HasAttribute<TAttribute>()
                   || actionDescriptor.ControllerTypeInfo.BaseType.HasAttribute<TAttribute>();
        }
        public static string ToCamelCase(this string value)
        {
            var array = value.ToCharArray();
            array[0] = char.ToLower(array[0]);
            return new string(array);
        }
    }
}