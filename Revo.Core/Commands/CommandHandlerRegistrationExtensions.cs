using System;
using System.Collections.Generic;
using System.Linq;
using Ninject.Syntax;
using Revo.Core.Core;

namespace Revo.Core.Commands
{
    internal static class CommandHandlerRegistrationExtensions
    {
        public static IBindingNamedWithOrOnSyntax<T> RegisterLocalCommandHandlerWithPipeline<T>(
            this BindingRoot bindingRoot, params Type[] services)
        {
            var handlerInterfaces = GetCommandHandlerInterfaces(typeof(T));

            return bindingRoot
                .Bind(handlerInterfaces.Concat(services).Concat(new[] {typeof(T)}).ToArray())
                .To<T>()
                .InTaskScope();
        }

        public static IBindingNamedWithOrOnSyntax<object> BindLocalCommandHandle(
            BindingRoot bindingRoot, Type commandHandlerType, params Type[] services)
        {
            var handlerInterfaces = GetCommandHandlerInterfaces(commandHandlerType);

            return bindingRoot
                .Bind(handlerInterfaces.Concat(services).Concat(new[] {commandHandlerType}).ToArray())
                .To(commandHandlerType)
                .InTaskScope();
        }

        public static Type[] GetCommandHandlerInterfaces(Type commandHandlerType)
        {
            var intfs = GetInterfaces(commandHandlerType);

            intfs = intfs
                .Where(x => x.IsGenericType
                            && new Type[] { typeof(ICommandHandler<>), typeof(ICommandHandler<,>), typeof(IQueryHandler<,>) }
                                .Contains(x.GetGenericTypeDefinition())); //TODO: inherited?!

            return intfs.Distinct().ToArray();
        }

        private static IEnumerable<Type> GetInterfaces(Type handlerType)
        {
            var intfs = (IEnumerable<Type>)handlerType.GetInterfaces();
            var nestedIntfs = intfs.SelectMany(x => x.GetInterfaces().Length > 0 ? GetInterfaces(x) : new Type[] {}).ToArray();
            if (nestedIntfs.Length > 0)
            {
                intfs = intfs.Concat(nestedIntfs);
            }

            return intfs;
        }
    }
}
