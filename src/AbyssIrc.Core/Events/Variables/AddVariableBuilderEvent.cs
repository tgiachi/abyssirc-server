namespace AbyssIrc.Core.Events.Variables;

public record AddVariableBuilderEvent(string VariableName, Func<object> Builder);
