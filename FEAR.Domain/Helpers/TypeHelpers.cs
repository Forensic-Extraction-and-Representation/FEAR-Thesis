using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEAR.Domain.Helpers
{
    public  static class TypeHelpers
    {
        public static List<Type> GetAllDerivedTypes<T>()
        {
            var baseType = typeof(T);
            var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type))
                .ToList();
            return derivedTypes;
        }
    }
}
