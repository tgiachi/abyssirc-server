namespace AbyssIrc.Server.Core.Events.Variables;

public record AddVariableBuilderEvent(string VariableName, Func<object> Builder);
