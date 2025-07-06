using Navend.Core.Step.Concrete;

namespace Navend.Core.Step.Abstract;

public interface IStep<TContext> where TContext : StepContext
{
    int Order { get; }
    Task ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}