using System.Reflection;
using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Interfaces.Commands;

namespace AbyssIrc.Tests;

[TestFixture]
public class IrcCommandConsistencyTests
{
    /// <summary>
    /// Verifies that all IRC commands inherit from BaseIrcCommand
    /// </summary>
    [Test]
    public void AllIrcCommands_ShouldInheritFromBaseIrcCommand()
    {
        // Find all types in the AbyssIrc.Network.Commands namespace
        var commandTypes = Assembly.GetAssembly(typeof(PrivMsgCommand))
            .GetTypes()
            .Where(
                t => t.Namespace == "AbyssIrc.Network.Commands" &&
                     !t.IsAbstract &&
                     typeof(IIrcCommand).IsAssignableFrom(t)
            );

        foreach (var commandType in commandTypes)
        {
            Assert.That(typeof(IIrcCommand).IsAssignableFrom(commandType),
                Is.True,
                $"Command {commandType.Name} does not implement IIrcCommand");

            // Check that it has a parameterless constructor
            var defaultConstructor = commandType.GetConstructor(Type.EmptyTypes);
            Assert.That(
                defaultConstructor,
                Is.Not.Null,
                $"Command {commandType.Name} lacks a parameterless constructor"
            );
        }
    }

    /// <summary>
    /// Ensures all commands have Parse and Write methods implemented
    /// </summary>
    [Test]
    public void AllIrcCommands_ShouldHaveParseAndWriteMethods()
    {
        var commandTypes = Assembly.GetAssembly(typeof(PrivMsgCommand))
            .GetTypes()
            .Where(
                t => t.Namespace == "AbyssIrc.Network.Commands" &&
                     !t.IsAbstract &&
                     typeof(IIrcCommand).IsAssignableFrom(t)
            );

        foreach (var commandType in commandTypes)
        {
            var parseMethod = commandType.GetMethod("Parse");
            var writeMethod = commandType.GetMethod("Write");

            Assert.That(
                parseMethod,
                Is.Not.Null,
                $"Command {commandType.Name} is missing Parse method"
            );
            Assert.That(
                writeMethod,
                Is.Not.Null,
                $"Command {commandType.Name} is missing Write method"
            );
        }
    }
}
