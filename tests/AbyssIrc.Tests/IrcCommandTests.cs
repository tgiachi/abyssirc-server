using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Commands.Errors;
using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Network.Types;

namespace AbyssIrc.Tests;

[TestFixture]
public class IrcCommandTests
{
    [Test]
    public void TestPartCommand_SingleChannel()
    {
        // Parsing
        var partCmd = new PartCommand();
        partCmd.Parse("PART #mychannel");

        Assert.That(partCmd.Channels.Count, Is.EqualTo(1));
        Assert.That(partCmd.Channels[0], Is.EqualTo("#mychannel"));
        Assert.That(partCmd.PartMessage, Is.Null);
        Assert.That(partCmd.Source, Is.Null);

        // Writing
        var writtenCmd = partCmd.Write();
        Assert.That(writtenCmd, Is.EqualTo("PART #mychannel"));
    }

    [Test]
    public void TestPartCommand_MultipleChannels()
    {
        // Parsing
        var partCmd = new PartCommand();
        partCmd.Parse("PART #channel1,#channel2");

        Assert.That(partCmd.Channels.Count, Is.EqualTo(2));
        Assert.That(partCmd.Channels[0], Is.EqualTo("#channel1"));
        Assert.That(partCmd.Channels[1], Is.EqualTo("#channel2"));
        Assert.That(partCmd.PartMessage, Is.Null);
    }

    [Test]
    public void TestPartCommand_WithMessage()
    {
        // Parsing
        var partCmd = new PartCommand();
        partCmd.Parse("PART #mychannel :Goodbye");

        Assert.That(partCmd.Channels.Count, Is.EqualTo(1));
        Assert.That(partCmd.Channels[0], Is.EqualTo("#mychannel"));
        Assert.That(partCmd.PartMessage, Is.EqualTo("Goodbye"));
    }

    [Test]
    public void TestPartCommand_WithSource()
    {
        // Parsing
        var partCmd = new PartCommand();
        partCmd.Parse(":nick!user@host PART #mychannel :Goodbye");

        Assert.That(partCmd.Source, Is.EqualTo("nick!user@host"));
        Assert.That(partCmd.Channels.Count, Is.EqualTo(1));
        Assert.That(partCmd.Channels[0], Is.EqualTo("#mychannel"));
        Assert.That(partCmd.PartMessage, Is.EqualTo("Goodbye"));
    }

    [Test]
    public void TestJoinCommand_SingleChannel()
    {
        // Parsing
        var joinCmd = new JoinCommand();
        joinCmd.Parse("JOIN #mychannel");

        Assert.That(joinCmd.Channels.Count, Is.EqualTo(1));
        Assert.That(joinCmd.Channels[0].ChannelName, Is.EqualTo("#mychannel"));
        Assert.That(joinCmd.Channels[0].Key, Is.Null);
    }

    [Test]
    public void TestJoinCommand_WithKey()
    {
        // Parsing
        var joinCmd = new JoinCommand();
        joinCmd.Parse("JOIN #mychannel secretkey");

        Assert.That(joinCmd.Channels.Count, Is.EqualTo(1));
        Assert.That(joinCmd.Channels[0].ChannelName, Is.EqualTo("#mychannel"));
        Assert.That(joinCmd.Channels[0].Key, Is.EqualTo("secretkey"));
    }

    [Test]
    public void TestJoinCommand_MultipleChannels()
    {
        // Parsing
        var joinCmd = new JoinCommand();
        joinCmd.Parse("JOIN #channel1,#channel2");

        Assert.That(joinCmd.Channels.Count, Is.EqualTo(2));
        Assert.That(joinCmd.Channels[0].ChannelName, Is.EqualTo("#channel1"));
        Assert.That(joinCmd.Channels[1].ChannelName, Is.EqualTo("#channel2"));
    }

    [Test]
    public void TestListCommand_AllChannels()
    {
        // Parsing
        var listCmd = new ListCommand();
        listCmd.Parse("LIST");

        Assert.That(listCmd.Channels.Count, Is.EqualTo(0));
        Assert.That(listCmd.FilterType, Is.Null);
    }

    [Test]
    public void TestListCommand_SpecificChannels()
    {
        // Parsing
        var listCmd = new ListCommand();
        listCmd.Parse("LIST #channel1,#channel2");

        Assert.That(listCmd.Channels.Count, Is.EqualTo(2));
        Assert.That(listCmd.Channels[0], Is.EqualTo("#channel1"));
        Assert.That(listCmd.Channels[1], Is.EqualTo("#channel2"));
    }

    [Test]
    public void TestListCommand_UserCountFilter()
    {
        // Parsing
        var listCmd = new ListCommand();
        listCmd.Parse("LIST >3");

        Assert.That(listCmd.FilterType, Is.EqualTo(ListFilterType.Users));
        Assert.That(listCmd.Comparison, Is.EqualTo(ComparisonType.GreaterThan));
        Assert.That(listCmd.FilterValue, Is.EqualTo(3));
    }

    [Test]
    public void TestErrorCommands_Parsing()
    {
        // Test ErrNoPrivileges
        var noPrivsError = new ErrNoPrivileges();
        noPrivsError.Parse(":irc.example.com 481 john :Permission denied");

        Assert.That(noPrivsError.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(noPrivsError.Nickname, Is.EqualTo("john"));
        Assert.That(noPrivsError.ErrorMessage, Is.EqualTo("Permission denied"));

        // Test ErrNotOnChannel
        var notOnChannelError = new ErrNotOnChannel();
        notOnChannelError.Parse(":irc.example.com 442 john #mychannel :You're not on that channel");

        Assert.That(notOnChannelError.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(notOnChannelError.Nickname, Is.EqualTo("john"));
        Assert.That(notOnChannelError.ChannelName, Is.EqualTo("#mychannel"));
        Assert.That(notOnChannelError.ErrorMessage, Is.EqualTo("You're not on that channel"));
    }

    [Test]
    public void TestNameReply_Parsing()
    {
        // Parsing
        var namesReply = new RplNameReply();
        namesReply.Parse(":irc.example.com 353 john = #mychannel :@alice +bob charlie");

        Assert.That(namesReply.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(namesReply.Nickname, Is.EqualTo("john"));
        Assert.That(namesReply.ChannelName, Is.EqualTo("#mychannel"));
        Assert.That(namesReply.Visibility, Is.EqualTo(ChannelVisibility.Public));

        Assert.That(
            namesReply.Members,
            Is.EqualTo(new[] { "@alice", "+bob", "charlie" })
        );
    }
}
