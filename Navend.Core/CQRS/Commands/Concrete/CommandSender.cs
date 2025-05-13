using Microsoft.Extensions.DependencyInjection;

namespace Navend.Core.CQRS;
public class CommandSender : ICommandSender {
    private readonly IServiceProvider _serviceProvider;

    public CommandSender(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public async Task<CommandResponse> SendAsync<TCommand>(TCommand command) where TCommand : ICommand {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        return await handler.HandleAsync(command);
    }
}