using Microsoft.Extensions.DependencyInjection;

namespace Navend.Core.CQRS;

public class QuerySender : IQuerySender {
    private readonly IServiceProvider _serviceProvider;

    public QuerySender(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> SendAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult> {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return await handler.HandleAsync(query);
    }
}