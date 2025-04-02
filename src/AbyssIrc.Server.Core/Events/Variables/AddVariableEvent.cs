namespace AbyssIrc.Server.Core.Events.Variables;

public record AddVariableEvent(string VariableName, object Value);
