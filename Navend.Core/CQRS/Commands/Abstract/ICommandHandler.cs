namespace Navend.Core.CQRS;
public interface ICommandHandler<TCommand> where TCommand : ICommand {
    Task<CommandResponse> HandleAsync(TCommand command);
}