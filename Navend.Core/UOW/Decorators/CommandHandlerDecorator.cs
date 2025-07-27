using Navend.Core.Attributes;
using Navend.Core.CQRS;
using Navend.Core.UOW.Helper;

namespace Navend.Core.UOW.Decorator;

[UOW]
public class CommandHandlerDecorator<TCommand> : ICommandHandler<TCommand> where TCommand : ICommand {
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICommandHandler<TCommand> _commandHandler;
    public CommandHandlerDecorator(IUnitOfWork unitOfWork, ICommandHandler<TCommand> commandHandler) {
        _unitOfWork = unitOfWork;
        _commandHandler = commandHandler;
    }
    public async Task<CommandResponse> HandleAsync(TCommand command) {
        var isUnitOfWorkEnabled = true;
        try {
            isUnitOfWorkEnabled = UnitOfWorkHelper.IsUnitOfWorkEnabled(_commandHandler.GetType());
            if(!isUnitOfWorkEnabled){
                return await _commandHandler.HandleAsync(command);
            }
            await _unitOfWork.StartTransactionAsync();
            var response = await _commandHandler.HandleAsync(command);
            await _unitOfWork.CommitTransactionAsync();
            return response;
        } catch (System.Exception ex) {
            if(isUnitOfWorkEnabled){
                await _unitOfWork.RollbackTransactionAsync();
            }
            Console.WriteLine(ex);
            throw;
        }
    }
}