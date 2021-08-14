using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace UpNet
{
    /// <summary>
    /// Extends <see cref="MulticastDelegate"/> that produce async results to allow sequentialization of invocation.
    /// </summary>
    public static class MulticastDelegateSequentialExtensions
    {
        /// <summary>
        /// Creates a sequentialized wrapper on the delegate that takes no arguments. 
        /// </summary>
        /// <param name="receiver">The multicast delegate to convert.</param>
        /// <returns>
        /// Returns a function that, when invoked, calls the delegates invocation list in order and awaits each before
        /// invoking the next.
        /// </returns>
        public static Func<Task> ToSequential(this MulticastDelegate? receiver)
        {
            // If receiver is null, return no-op function.
            if (null == receiver) return () => Task.CompletedTask;

            // Require multicast type dynamically.
            Contract.Requires(receiver.Method.GetParameters().Length == 0, "receiver may not take arguments");
            Contract.Requires(receiver.Method.ReturnType == typeof(Task), "receiver must return task");

            // Get invocation list and target conversion list.
            var invocationList = receiver.GetInvocationList();
            var converted = new Func<Task>[invocationList.Length];

            // Convert all invocations to functions.
            for (var i = 0; i < invocationList.Length; i++)
            {
                var target = invocationList[i].Target;
                var method = invocationList[i].Method;
                converted[i] = Expression
                    .Lambda<Func<Task>>(Expression.Call(Expression.Constant(target), method))
                    .Compile();
            }

            // Return an async function invoking in sequence and awaiting.
            return async () =>
            {
                foreach (var function in converted)
                    await function();
            };
        }

        /// <summary>
        /// Creates a sequentialized wrapper on the delegate that takes one argument. 
        /// </summary>
        /// <param name="receiver">The multicast delegate to convert.</param>
        /// <typeparam name="T">The type of the single argument.</typeparam>
        /// <returns>
        /// Returns a function that, when invoked, calls the delegates invocation list in order and awaits each before
        /// invoking the next.
        /// </returns>
        public static Func<T, Task> ToSequential<T>(this MulticastDelegate? receiver)
        {
            // If receiver is null, return no-op function.
            if (null == receiver) return _ => Task.CompletedTask;

            // Require multicast type dynamically.
            Contract.Requires(receiver.Method.GetParameters().Length == 1, "receiver must take one argument");
            Contract.Requires(receiver.Method.ReturnType == typeof(Task), "receiver must return task");

            // Get invocation list and target conversion list.
            var invocationList = receiver.GetInvocationList();
            var converted = new Func<T, Task>[invocationList.Length];

            // Create parameter, make argument array and parameter array for use in all invocations.
            var param = Expression.Parameter(typeof(T));
            var args = new Expression[] {param};
            var @params = new[] {param};

            // Convert all invocations to functions.
            for (var i = 0; i < invocationList.Length; i++)
            {
                var target = invocationList[i].Target;
                var method = invocationList[i].Method;
                converted[i] = Expression
                    .Lambda<Func<T, Task>>(Expression.Call(Expression.Constant(target), method, args), @params)
                    .Compile();
            }

            // Return an async function invoking in sequence and awaiting.
            return async arg =>
            {
                foreach (var function in converted)
                    await function(arg);
            };
        }

        /// <summary>
        /// Creates a sequentialized wrapper on the delegate that takes two arguments. 
        /// </summary>
        /// <param name="receiver">The multicast delegate to convert.</param>
        /// <typeparam name="T1">The type of the first argument.</typeparam>
        /// <typeparam name="T2">The type of the second argument.</typeparam>
        /// <returns>
        /// Returns a function that, when invoked, calls the delegates invocation list in order and awaits each before
        /// invoking the next.
        /// </returns>
        public static Func<T1, T2, Task> ToSequential<T1, T2>(this MulticastDelegate? receiver)
        {
            // If receiver is null, return no-op function.
            if (null == receiver) return (_, _) => Task.CompletedTask;

            // Require multicast type dynamically.
            Contract.Requires(receiver.Method.GetParameters().Length == 2, "receiver must take two argument");
            Contract.Requires(receiver.Method.ReturnType == typeof(Task), "receiver must return task");

            // Get invocation list and target conversion list.
            var invocationList = receiver.GetInvocationList();
            var converted = new Func<T1, T2, Task>[invocationList.Length];

            // Create parameters, make argument array and parameter array for use in all invocations.
            var param1 = Expression.Parameter(typeof(T1));
            var param2 = Expression.Parameter(typeof(T2));
            var args = new Expression[] {param1, param2};
            var @params = new[] {param1, param2};

            // Convert all invocations to functions.
            for (var i = 0; i < invocationList.Length; i++)
            {
                var target = invocationList[i].Target;
                var method = invocationList[i].Method;
                converted[i] = Expression
                    .Lambda<Func<T1, T2, Task>>(Expression.Call(Expression.Constant(target), method, args), @params)
                    .Compile();
            }

            // Return an async function invoking in sequence and awaiting.
            return async (arg1, arg2) =>
            {
                foreach (var function in converted)
                    await function(arg1, arg2);
            };
        }

        /// <summary>
        /// Creates a sequentialized wrapper on the delegate that takes three arguments. 
        /// </summary>
        /// <param name="receiver">The multicast delegate to convert.</param>
        /// <typeparam name="T1">The type of the first argument.</typeparam>
        /// <typeparam name="T2">The type of the second argument.</typeparam>
        /// <typeparam name="T3">The type of the third argument.</typeparam>
        /// <returns>
        /// Returns a function that, when invoked, calls the delegates invocation list in order and awaits each before
        /// invoking the next.
        /// </returns>
        public static Func<T1, T2, T3, Task> ToSequential<T1, T2, T3>(this MulticastDelegate? receiver)
        {
            // If receiver is null, return no-op function.
            if (null == receiver) return (_, _, _) => Task.CompletedTask;

            // Require multicast type dynamically.
            Contract.Requires(receiver.Method.GetParameters().Length == 3, "receiver must take three argument");
            Contract.Requires(receiver.Method.ReturnType == typeof(Task), "receiver must return task");

            // Get invocation list and target conversion list.
            var invocationList = receiver.GetInvocationList();
            var converted = new Func<T1, T2, T3, Task>[invocationList.Length];

            // Create parameters, make argument array and parameter array for use in all invocations.
            var param1 = Expression.Parameter(typeof(T1));
            var param2 = Expression.Parameter(typeof(T2));
            var param3 = Expression.Parameter(typeof(T3));
            var args = new Expression[] {param1, param2, param3};
            var @params = new[] {param1, param2, param3};

            // Convert all invocations to functions.
            for (var i = 0; i < invocationList.Length; i++)
            {
                var target = invocationList[i].Target;
                var method = invocationList[i].Method;
                converted[i] = Expression
                    .Lambda<Func<T1, T2, T3, Task>>(Expression.Call(Expression.Constant(target), method, args), @params)
                    .Compile();
            }

            // Return an async function invoking in sequence and awaiting.
            return async (arg1, arg2, arg3) =>
            {
                foreach (var function in converted)
                    await function(arg1, arg2, arg3);
            };
        }
    }
}