using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CloudSourced
{
    public class Assemblies : IEnumerable<Assembly>
    {
        List<Assembly> assemblies = new List<Assembly>();

        public Assemblies()
        {

        }

        public Assemblies(Assembly assembly)
        {
            Add(assembly);
        }

        public Assemblies(Type type)
        {
            Add(type);
        }

        public Assemblies(params Assembly[] assemblies)
        {
            AddRange(assemblies);
        }

        public Assemblies(IEnumerable<Assembly> assemblies)
        {
            AddRange(assemblies);
        }

        public Assemblies(params Type[] types)
        {
            AddRange(types);
        }

        public Assemblies(IEnumerable<Type> types)
        {
            AddRange(types);
        }

        public void Add(Assembly assembly)
        {
            if (!assemblies.Contains(assembly))
                assemblies.Add(assembly);
        }

        public void Add(Type type)
        {
            Add(type.Assembly);
        }

        public void AddRange(IEnumerable<Assembly> assemblies)
        {
            assemblies.ToList().ForEach(a => Add(a));
        }

        public void AddRange(IEnumerable<Type> types)
        {
            types.ToList().ForEach(a => Add(a));
        }

        public IEnumerator<Assembly> GetEnumerator()
        {
            return assemblies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return assemblies.GetEnumerator();
        }

        public Type GetType(string name, bool throwOnError = false, bool ignoreCase = false)
        {
            Type type = assemblies.Select(a => a.GetType(name, false, ignoreCase)).Where(t => t != null).FirstOrDefault();
            if (throwOnError && type == null)
                throw new TypeLoadException("Error loading Type: " + name);
            return type;
        }
        public IEnumerable<Type> GetTypes()
        {
            return assemblies.Select(a => a.GetTypes()).SelectMany(arr => arr);
        }

        public IEnumerable<KeyValuePair<Type, IEnumerable<A>>> GetTypesWithAttribute<A>() where A : Attribute
        {
            return GetTypes().Select(t => new KeyValuePair<Type, IEnumerable<A>>(t, t.GetCustomAttributes(typeof(A), true).Where(o => o is A).Select(o => o as A))).Where(kv => kv.Value.Count() > 0);
        }

        public static implicit operator Assembly[] (Assemblies a)
        {
            return a.assemblies.ToArray();
        }
    }
}
