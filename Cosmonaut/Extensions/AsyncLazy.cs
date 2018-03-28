using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Cosmonaut.Extensions
{
    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) :
            base(() => Task.Run(valueFactory))
        { }

        public AsyncLazy(Func<Task<T>> taskFactory) :
            base(() => Task.Run(taskFactory)) { }

        public TaskAwaiter<T> GetAwaiter() { return Value.GetAwaiter(); }
    }
}