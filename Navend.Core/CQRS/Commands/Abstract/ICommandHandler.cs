namespace Navend.Core.CQRS;
public interface ICommandHandler<TCommand> where TCommand : ICommand {
    Task HandleAsync(TCommand command);
}