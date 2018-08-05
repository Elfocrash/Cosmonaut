using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cosmonaut.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<IEnumerable<T>> WhenAllTasksAsync<T>(this IEnumerable<Task<T>> tasks)
        {
            var allTask = Task.WhenAll(tasks);

            try
            {
                return await allTask;
            }
            catch (Exception)
            {
                // We don't throw the first one. We throw the aggragated one later
            }

            throw allTask.Exception ?? throw new Exception("There is no way this will ever be thrown. (What all great developers say)");
        }
    }
}