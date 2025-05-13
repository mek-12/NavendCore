namespace  Navend.Core.CQRS;
public class CommandResponse {
    public object? Result { get; private set; }
    public CommandResponse(object? result = null) {
        Result = result;
    }
    public static CommandResponse Status => new(string.Empty);
}