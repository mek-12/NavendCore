namespace Navend.Core.CQRS
{
    public interface ICommandSender
    {
        Task<CommandResponse> SendAsync<TCommand>(TCommand command) where TCommand : ICommand;
    }
}
