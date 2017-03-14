using System;
using System.Collections.Generic;
using System.Linq;
using GTRevo.Platform.Core;
using MediatR;
using Ninject.Syntax;

namespace GTRevo.Platform.Commands
{
    public static class CommandBindExtensions
    {
        public static IBindingInNamedWithOrOnSyntax<T> BindCommandHandler<T>(
            this BindingRoot bindingRoot, params Type[] services)
        {
            var handlerInterfaces = GetCommandHandlerInterfaces(typeof(T));
            var decoratorTypes = GetPipelineDecoratorTypes(handlerInterfaces);

            foreach (var decoratorTypePair in decoratorTypes)
            {
                bindingRoot
                    .Bind(decoratorTypePair.Key)
                    .To(decoratorTypePair.Value)
                    .InRequestOrJobScope();
            }

            return bindingRoot
                .Bind(handlerInterfaces.Concat(services).ToArray())
                .To<T>()
                //.When(x => x.Target?.Type.Name.StartsWith("CommandHandlerPipeline") ?? false);
            .WhenInjectedInto(decoratorTypes.Values.ToArray());
        }
        
        private static IEnumerable<Type> GetInterfaces(Type handlerType)
        {
            var intfs = (IEnumerable<Type>)handlerType.GetInterfaces();
            var nestedIntfs = intfs.SelectMany(x => x.GetInterfaces().Length > 0 ? GetInterfaces(x) : new Type[] { });
            if (nestedIntfs.Count() > 0)
            {
                intfs = intfs.Concat(nestedIntfs.ToList());
            }

            return intfs;
        }

        private static Dictionary<Type, Type> GetPipelineDecoratorTypes(Type[] handlerInterfaces)
        {
            return handlerInterfaces
                .ToDictionary(
                x => x,
                x =>
                {
                    if (x.GenericTypeArguments.Length == 1)
                    {
                        return typeof(CommandHandlerPipeline<>).MakeGenericType(x.GenericTypeArguments[0]);
                    }
                    else
                    {
                        return typeof(CommandHandlerPipeline<,>).MakeGenericType(x.GenericTypeArguments[0], x.GenericTypeArguments[1]);
                    }
                });
        }

        private static Type[] GetCommandHandlerInterfaces(Type requestHandlerType)
        {
            var intfs = GetInterfaces(requestHandlerType);

            intfs = intfs
                .Where(x => x.IsGenericType
                    && new Type[] { typeof(IAsyncCommandHandler<>), typeof(IAsyncCommandHandler<,>), typeof(IAsyncQueryHandler<,>),
                                    typeof(IAsyncRequestHandler<>), typeof(IAsyncRequestHandler<,>),
                                    typeof(IRequestHandler<>), typeof(IRequestHandler<,>) }
                            .Contains(x.GetGenericTypeDefinition())); //TODO: inherited?!

            return intfs.Distinct().ToArray();
        }
    }
}
