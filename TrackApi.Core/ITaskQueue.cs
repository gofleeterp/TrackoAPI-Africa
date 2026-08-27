using System;
using System.Threading.Tasks;

namespace TrackoApi.Queue
{
    public interface ITaskQueue
    {
        Task Enqueue(Func<Task> taskGenerator);
        Task<T> Enqueue<T>(Func<Task<T>> taskGenerator);
        //void Enqueue(Task task);
    }
}