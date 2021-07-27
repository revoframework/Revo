using System;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Commands.Lambda
{
    public static class LambdaCommandBusExtensions
    {
        #region SendLambdaCommand for commands

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand(this ICommandBus commandBus, Func<Task> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1>(this ICommandBus commandBus, Func<Task, T1> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }


        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1, T2>(this ICommandBus commandBus, Func<Task, T1, T2> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }


        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1, T2, T3>(this ICommandBus commandBus, Func<Task, T1, T2, T3> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }
        
        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1, T2, T3, T4>(this ICommandBus commandBus, Func<Task, T1, T2, T3, T4> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }
        
        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1, T2, T3, T4, T5>(this ICommandBus commandBus, Func<Task, T1, T2, T3, T4, T5> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1, T2, T3, T4, T5, T6>(this ICommandBus commandBus, Func<Task, T1, T2, T3, T4, T5, T6> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1, T2, T3, T4, T5, T6, T7>(this ICommandBus commandBus, Func<Task, T1, T2, T3, T4, T5, T6, T7> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), cancellationToken);
        }

        #endregion

        #region SendLambdaCommand for commands, with CommandExecutionOptions

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand(this ICommandBus commandBus, Func<Task> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1>(this ICommandBus commandBus, Func<Task, T1> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1, T2>(this ICommandBus commandBus, Func<Task, T1, T2> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1, T2, T3>(this ICommandBus commandBus,
            Func<Task, T1, T2, T3> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1, T2, T3, T4>(this ICommandBus commandBus,
            Func<Task, T1, T2, T3, T4> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1, T2, T3, T4, T5>(this ICommandBus commandBus,
            Func<Task, T1, T2, T3, T4, T5> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1, T2, T3, T4, T5, T6>(this ICommandBus commandBus,
            Func<Task, T1, T2, T3, T4, T5, T6> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<T1, T2, T3, T4, T5, T6, T7>(this ICommandBus commandBus,
            Func<Task, T1, T2, T3, T4, T5, T6, T7> command,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaCommand(command), executionOptions, cancellationToken);
        }

        #endregion

        #region SendLambdaCommand for queries

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult>(this ICommandBus commandBus, Func<Task<TResult>, TResult> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1>(this ICommandBus commandBus, Func<T1, TResult> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1, T2>(this ICommandBus commandBus, Func<T1, T2, TResult> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1, T2, T3>(this ICommandBus commandBus, Func<T1, T2, T3, TResult> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1, T2, T3, T4>(this ICommandBus commandBus, Func<T1, T2, T3, T4, TResult> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1, T2, T3, T4, T5>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, TResult> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1, T2, T3, T4, T5, T6>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, T6, TResult> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1, T2, T3, T4, T5, T6, T7>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), cancellationToken);
        }

        #endregion

        #region SendLambdaCommand for queries, with CommandExecutionOptions

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult>(this ICommandBus commandBus, Func<Task<TResult>, TResult> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1>(this ICommandBus commandBus, Func<T1, TResult> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1, T2>(this ICommandBus commandBus, Func<T1, T2, TResult> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1, T2, T3>(this ICommandBus commandBus,
            Func<T1, T2, T3, TResult> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1, T2, T3, T4>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, TResult> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1, T2, T3, T4, T5>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, TResult> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1, T2, T3, T4, T5, T6>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, T6, TResult> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        /// <summary>
        /// Executes specified lambda function like it was performed by any regular command handler.
        /// This includes scoped resolution of its parameter dependencies, committing the unit of work, etc.
        /// </summary>
        public static Task SendLambdaCommand<TResult, T1, T2, T3, T4, T5, T6, T7>(this ICommandBus commandBus,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> query,
            CommandExecutionOptions executionOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return commandBus.SendAsync(new LambdaQuery(query, typeof(TResult)), executionOptions, cancellationToken);
        }

        #endregion
    }
}