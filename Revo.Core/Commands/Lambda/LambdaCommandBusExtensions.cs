using System;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Commands.Lambda
{
    public static class LambdaCommandBusExtensions
    {
        #region SendLambdaCommandAsync for commands

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync(this ICommandBus commandBus, Func<Task> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1>(this ICommandBus commandBus, Func<T1, Task> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }


        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1, T2>(this ICommandBus commandBus, Func<T1, T2, Task> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }


        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1, T2, T3>(this ICommandBus commandBus, Func<T1, T2, T3, Task> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }
        
        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1, T2, T3, T4>(this ICommandBus commandBus, Func<T1, T2, T3, T4, Task> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }
        
        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1, T2, T3, T4, T5>(this ICommandBus commandBus, Func<T1, T2, T3, T4, T5, Task> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1, T2, T3, T4, T5, T6>(this ICommandBus commandBus, Func<T1, T2, T3, T4, T5, T6, Task> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1, T2, T3, T4, T5, T6, T7>(this ICommandBus commandBus, Func<T1, T2, T3, T4, T5, T6, T7, Task> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }

        #endregion

        #region SendLambdaCommandAsync for commands, with CommandExecutionOptions

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync(this ICommandBus commandBus, Func<Task> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1>(this ICommandBus commandBus, Func<T1, Task> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1, T2>(this ICommandBus commandBus, Func<T1, T2, Task> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1, T2, T3>(this ICommandBus commandBus,
            Func<T1, T2, T3, Task> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1, T2, T3, T4>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, Task> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1, T2, T3, T4, T5>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, Task> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1, T2, T3, T4, T5, T6>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, T6, Task> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<T1, T2, T3, T4, T5, T6, T7>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, T6, T7, Task> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        #endregion

        #region SendLambdaCommandAsync for queries

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult>(this ICommandBus commandBus, Func<Task<TResult>> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1>(this ICommandBus commandBus, Func<T1, Task<TResult>> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1, T2>(this ICommandBus commandBus, Func<T1, T2, Task<TResult>> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1, T2, T3>(this ICommandBus commandBus, Func<T1, T2, T3, Task<TResult>> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1, T2, T3, T4>(this ICommandBus commandBus, Func<T1, T2, T3, T4, Task<TResult>> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1, T2, T3, T4, T5>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, Task<TResult>> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1, T2, T3, T4, T5, T6>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, T6, Task<TResult>> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1, T2, T3, T4, T5, T6, T7>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, T6, T7, Task<TResult>> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), cancellationToken);
        }

        #endregion

        #region SendLambdaCommandAsync for queries, with CommandExecutionOptions

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult>(this ICommandBus commandBus, Func<Task<TResult>> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1>(this ICommandBus commandBus, Func<T1, Task<TResult>> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1, T2>(this ICommandBus commandBus, Func<T1, T2, Task<TResult>> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1, T2, T3>(this ICommandBus commandBus,
            Func<T1, T2, T3, Task<TResult>> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1, T2, T3, T4>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, Task<TResult>> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1, T2, T3, T4, T5>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, Task<TResult>> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1, T2, T3, T4, T5, T6>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, T6, Task<TResult>> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommandAsync<TResult, T1, T2, T3, T4, T5, T6, T7>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, T6, T7, Task<TResult>> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaResultCommand(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        #endregion
    }
}