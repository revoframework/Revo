using System;
using System.Linq;
using Ninject.Syntax;
using Revo.Core.Core;

namespace Revo.Core.Commands
{
    public static class CommandHandlerBindingExtensions
    {
        /// <summary>
        /// Binds command handler class to all of its implemented <see cref="ICommandHandler{T}"/>
        /// and <see cref="IQueryHandler{TQuery,TResult}"/> interfaces. Uses InTaskScope() life-time.
        /// Calling this method manually in your modules is not needed when using <see cref="CommandsConfiguration.AutoDiscoverCommandHandlers"/>.
        /// </summary>
        /// <typeparam name="T">Command handler type.</typeparam>
        /// <param name="bindingRoot">Ninject module or kernel.</param>
        /// <param name="additionalServices">Additional services to bind for.</param>
        public static IBindingNamedWithOrOnSyntax<T> BindCommandHandler<T>(
            this IBindingRoot bindingRoot, params Type[] additionalServices)
            where T : class
        {
            if (!typeof(T).IsClass || typeof(T).IsAbstract)
            {
                throw new ArgumentException($"{typeof(T)} is not a bindable non-abstract class");
            }

            var handlerInterfaces = GetCommandHandlerInterfaces(typeof(T));
            if (handlerInterfaces.Length == 0)
            {
                throw new ArgumentException($"{typeof(T)} does not implement any ICommandHandler<> nor ICommandHandler<,>");
            }

            return bindingRoot
                .Bind(handlerInterfaces.Concat(additionalServices).Append(typeof(T)).ToArray())
                .To<T>()
                .InTaskScope();
        }

        /// <summary>
        /// Binds command handler class to all of its implemented <see cref="ICommandHandler{T}"/>
        /// and <see cref="IQueryHandler{TQuery,TResult}"/> interfaces. Uses InTaskScope() life-time.
        /// Calling this method manually in your modules is not needed when using <see cref="CommandsConfiguration.AutoDiscoverCommandHandlers"/>.
        /// </summary>
        /// <param name="bindingRoot">Ninject module or kernel.</param>
        /// <param name="commandHandlerType">Command handler type.</param>
        /// <param name="additionalServices">Additional services to bind for.</param>
        public static IBindingNamedWithOrOnSyntax<object> BindCommandHandler(
            this IBindingRoot bindingRoot, Type commandHandlerType, params Type[] additionalServices)
        {
            if (!commandHandlerType.IsClass || commandHandlerType.IsAbstract)
            {
                throw new ArgumentException($"{commandHandlerType} is not a bindable non-abstract class");
            }

            var handlerInterfaces = GetCommandHandlerInterfaces(commandHandlerType);
            if (handlerInterfaces.Length == 0)
            {
                throw new ArgumentException($"{commandHandlerType} does not implement any ICommandHandler<> nor ICommandHandler<,>");
            }
            
            return bindingRoot
                .Bind(handlerInterfaces.Concat(additionalServices).Append(commandHandlerType).ToArray())
                .To(commandHandlerType)
                .InTaskScope();
        }

        public static Type[] GetCommandHandlerInterfaces(Type commandHandlerType)
        {
            return commandHandlerType.GetInterfaces()
                .Where(x => x.IsGenericType
                            && new[] {typeof(ICommandHandler<>), typeof(ICommandHandler<,>)}
                                .Contains(x.GetGenericTypeDefinition()))
                .ToArray();
        }
    }
}
