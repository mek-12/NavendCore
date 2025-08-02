using Navend.Core.Attributes;
using Navend.Core.Step.Abstract;
using Navend.Core.Step.Concrete;
using Navend.Core.UOW.Helper;

namespace Navend.Core.UOW.Decorators;
[Decorator]
public class StepDecorator<TStepContext>(IUnitOfWork unitOfWork, IStep<TStepContext> step): IStep<TStepContext> where TStepContext : StepContext
{
    private int _order = default;
    public int Order => _order;

    internal void SetOrder(int order) {
        _order = order;
    }
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var isUnitOfWorkEnabled = true;
        try {
            Console.WriteLine("StepDecorator Runned!");
            isUnitOfWorkEnabled = UnitOfWorkHelper.IsUnitOfWorkEnabled(step.GetType());
            if (!isUnitOfWorkEnabled)
            {
                await step.ExecuteAsync(cancellationToken);
                return;
            }
            await unitOfWork.StartTransactionAsync();
            await step.ExecuteAsync();
            await unitOfWork.CommitTransactionAsync();
        } catch (Exception ex) {
            if(isUnitOfWorkEnabled){
                await unitOfWork.RollbackTransactionAsync();
            }
            Console.WriteLine(ex);
            throw;
        }
    }
}
