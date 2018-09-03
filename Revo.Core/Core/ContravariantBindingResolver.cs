using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject.Components;
using Ninject.Infrastructure;
using Ninject.Planning.Bindings;
using Ninject.Planning.Bindings.Resolvers;

namespace Revo.Core.Core
{
    public class ContravariantBindingResolver : NinjectComponent, IBindingResolver
    {
        /// <summary>
        /// Returns any bindings from the specified collection that match the specified service.
        /// </summary>
        public IEnumerable<IBinding> Resolve(Multimap<Type, IBinding> bindings, Type service)
        {
            if (service.IsGenericType)
            {
                var genericType = service.GetGenericTypeDefinition();
                var genericArguments = genericType.GetGenericArguments();
                var isContravariant = genericArguments.Length == 1
                                      && genericArguments
                                          .Single()
                                          .GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant);
                if (isContravariant)
                {
                    var argument = service.GetGenericArguments().Single();
                    var matches = bindings.Where(kvp => kvp.Key.IsGenericType
                                                        && kvp.Key.GetGenericTypeDefinition() == genericType
                                                        && kvp.Key.GetGenericArguments().Single() != argument
                                                        && kvp.Key.GetGenericArguments().Single().IsAssignableFrom(argument))
                        .SelectMany(kvp => kvp.Value);
                    return matches;
                }
            }

            return Enumerable.Empty<IBinding>();
        }
    }
}
