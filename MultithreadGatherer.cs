using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Executes callbacks on an object queue fed by multiple threads in a single safe thread.
/// All the object given by Enqueue are executed in their arrival order as a FIFO stack.
/// </summary>
public class MultiThreadGatherer<T>
{
    /// <summary>
    /// The queue containing all the objects. It is synchronized to avoid any concurency problems
    /// </summary>
    private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

    /// <summary>
    /// the task executing the loop
    /// </summary>
    private Task _task;

    /// <summary>
    /// The callback called for each elements in the queue;
    /// </summary>
    public Action<T> CallBack { get; protected set; }

    /// <summary>
    /// Adds a value to add in the FIFO callback execution queue. this is asynchronous
    /// </summary>
    /// <param name="toQueue"></param>
    public void EnQueue(T toQueue)
    {
        _queue.Enqueue(toQueue);

        if (_task != null && !_task.IsCompleted) return;
        _task = Task.Factory.StartNew(TaskLoop);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiThreadGatherer<T/>"/> class.
    /// </summary>
    /// <param name="cb">The callback to be called for each object in the queue.</param>
    protected MultiThreadGatherer(Action<T> cb) => CallBack = cb;

    /// <summary>
    /// the loop launching callbacks on queued values
    /// </summary>
    private void TaskLoop()
    {
        while (!_queue.IsEmpty)
            if (_queue.TryDequeue(out var result))
                CallBack(result);
    }
}