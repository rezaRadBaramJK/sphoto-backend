using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.Framework.Services.BackgroundJobs
{
    public interface IBaramjkBackgroundJob
    {
        bool Delete(string jobid);
        string Enqueue(Expression<Func<Task>> methodCall);
        string Enqueue(Expression<Action> methodCall);
        string Enqueue(string queue, Expression<Action> methodCall);
        string Enqueue(string queue, Expression<Func<Task>> methodCall);

        string Schedule(Expression<Action> methodCall,
            TimeSpan delay);

        string Schedule(
            string queue,
            Expression<Action> methodCall,
            TimeSpan delay);

        string Schedule(
            Expression<Func<Task>> methodCall,
            TimeSpan delay);
        
        string Schedule(
            string queue,
            Expression<Func<Task>> methodCall,
            TimeSpan delay);

        string Schedule(
            Expression<Action> methodCall,
            DateTimeOffset enqueueAt);
        
        string Schedule(
            string queue,
            Expression<Action> methodCall,
            DateTimeOffset enqueueAt);

        string Schedule(
            Expression<Func<Task>> methodCall,
            DateTimeOffset enqueueAt);

        string Schedule(
            string queue,
            Expression<Func<Task>> methodCall,
            DateTimeOffset enqueueAt);
    }
}