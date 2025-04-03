using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Commands.Errors;
using AbyssIrc.Protocol.Messages.Commands.Replies;
using AbyssIrc.Protocol.Messages.Types;

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

    [Test]
    public void TestParse_ManyMembers()
    {
        var namesReply = new RplNameReply();
        namesReply.Parse(":irc.example.com 353 john = #largechannel :@alice +bob charlie @dave +eve frank @george +henry");

        Assert.That(namesReply.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(namesReply.Nickname, Is.EqualTo("john"));
        Assert.That(namesReply.Visibility, Is.EqualTo(ChannelVisibility.Public));
        Assert.That(namesReply.ChannelName, Is.EqualTo("#largechannel"));

        Assert.That(
            namesReply.Members,
            Is.EqualTo(new[] { "@alice", "+bob", "charlie", "@dave", "+eve", "frank", "@george", "+henry" })
        );
    }

    [Test]
    public void TestParse_SpreadAcrossMultipleParts()
    {
        var namesReply = new RplNameReply();
        namesReply.Parse(
            ":irc.example.com 353 john = #multichannel :@alice +bob charlie @dave +eve frank @george +henry indigo"
        );

        Assert.That(namesReply.Members.Count, Is.EqualTo(9));
        Assert.That(
            namesReply.Members,
            Is.EqualTo(new[] { "@alice", "+bob", "charlie", "@dave", "+eve", "frank", "@george", "+henry", "indigo" })
        );
    }

    [Test]
    public void TestParse_UnicodeNicknames()
    {
        var namesReply = new RplNameReply();
        namesReply.Parse(":irc.example.com 353 john = #unicode :@김철수 +박영희 도경수");

        Assert.That(namesReply.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(namesReply.Nickname, Is.EqualTo("john"));
        Assert.That(namesReply.ChannelName, Is.EqualTo("#unicode"));

        Assert.That(
            namesReply.Members,
            Is.EqualTo(new[] { "@김철수", "+박영희", "도경수" })
        );
    }

    [Test]
    public void TestParse_ComplexPrefixes()
    {
        var namesReply = new RplNameReply();
        namesReply.Parse(":irc.example.com 353 john = #complexchannel :@@alice +@bob charlie");

        Assert.That(
            namesReply.Members,
            Is.EqualTo(new[] { "@@alice", "+@bob", "charlie" })
        );
    }

    [Test]
    public void TestCreate_DifferentVisibilities()
    {
        var publicReply = RplNameReply.Create(
            "irc.example.com",
            "john",
            "#public",
            new[] { "@alice", "+bob", "charlie" }
        );

        Assert.That(publicReply.Visibility, Is.EqualTo(ChannelVisibility.Public));
        Assert.That(publicReply.ChannelName, Is.EqualTo("#public"));

        var secretReply = RplNameReply.Create(
            "irc.example.com",
            "john",
            "#secret",
            new[] { "@alice", "+bob", "charlie" },
            ChannelVisibility.Secret
        );

        Assert.That(secretReply.Visibility, Is.EqualTo(ChannelVisibility.Secret));
        Assert.That(secretReply.ChannelName, Is.EqualTo("#secret"));
    }

    [Test]
    public void TestWrite_StandardScenario()
    {
        var namesReply = new RplNameReply
        {
            ServerName = "irc.example.com",
            Nickname = "john",
            ChannelName = "#testchannel",
            Visibility = ChannelVisibility.Public,
            Members = new List<string> { "@alice", "+bob", "charlie" }
        };

        var writtenReply = namesReply.Write();

        Assert.That(
            writtenReply,
            Is.EqualTo(":irc.example.com 353 john = #testchannel :@alice +bob charlie")
        );
    }

    [Test]
    public void TestWrite_ThrowsOnInvalidState()
    {
        var namesReply = new RplNameReply();

        Assert.Throws<InvalidOperationException>(() => { namesReply.Write(); });
    }

    // [Test]
    // public void TestRplWelcome_Parsing()
    // {
    //     var welcomeCmd = new RplWelcome();
    //     welcomeCmd.Parse(":irc.example.com 001 john :Welcome to the Internet Relay Network john");
    //
    //     Assert.That(welcomeCmd.ServerName, Is.EqualTo("irc.example.com"));
    //     Assert.That(welcomeCmd.Ta Is.EqualTo("john"));
    //     Assert.That(welcomeCmd.Message, Is.EqualTo("Welcome to the Internet Relay Network john"));
    // }
    //
    [Test]
    public void TestRplYourHost_Parsing()
    {
        var yourHostCmd = new RplYourHost();
        yourHostCmd.Parse(":irc.example.com 002 john :Your host is irc.example.com, running version AbyssIRC-1.0");

        Assert.That(yourHostCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(yourHostCmd.Nickname, Is.EqualTo("john"));
        Assert.That(yourHostCmd.Message, Is.EqualTo("Your host is irc.example.com, running version AbyssIRC-1.0"));
    }

    [Test]
    public void TestRplCreated_Parsing()
    {
        var createdCmd = new RplCreated();
        createdCmd.Parse(":irc.example.com 003 john :This server was created Mon Jan 15 2024 at 10:15:00 UTC");

        Assert.That(createdCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(createdCmd.Nickname, Is.EqualTo("john"));
        Assert.That(createdCmd.CreationMessage, Is.EqualTo("This server was created Mon Jan 15 2024 at 10:15:00 UTC"));
    }

    [Test]
    public void TestRplMyInfo_Parsing()
    {
        var myInfoCmd = new RplMyInfo();
        myInfoCmd.Parse(":irc.example.com 004 john irc.example.net AbyssIRC-1.0 aoOirw biklmnopstv");

        Assert.That(myInfoCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(myInfoCmd.TargetNick, Is.EqualTo("john"));
        Assert.That(myInfoCmd.Version, Is.EqualTo("AbyssIRC-1.0"));
        Assert.That(myInfoCmd.UserModes, Is.EqualTo("aoOirw"));
        Assert.That(myInfoCmd.ChannelModes, Is.EqualTo("biklmnopstv"));
    }

    [Test]
    public void TestRplTopic_Parsing()
    {
        var topicCmd = new RplTopic();
        topicCmd.Parse(":irc.example.com 332 john #mychannel :Welcome to our awesome channel!");

        Assert.That(topicCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(topicCmd.Nickname, Is.EqualTo("john"));
        Assert.That(topicCmd.ChannelName, Is.EqualTo("#mychannel"));
        Assert.That(topicCmd.Topic, Is.EqualTo("Welcome to our awesome channel!"));
    }

    [Test]
    public void TestRplTopicWhoTime_Parsing()
    {
        var topicWhoTimeCmd = new RplTopicWhoTime();
        topicWhoTimeCmd.Parse(":irc.example.com 333 john #mychannel alice!user@host 1609459200");

        Assert.That(topicWhoTimeCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(topicWhoTimeCmd.Nickname, Is.EqualTo("john"));
        Assert.That(topicWhoTimeCmd.ChannelName, Is.EqualTo("#mychannel"));
        Assert.That(topicWhoTimeCmd.SetterMask, Is.EqualTo("alice!user@host"));
        Assert.That(topicWhoTimeCmd.SetTimestamp, Is.EqualTo(1609459200));
    }

    [Test]
    public void TestErrNoSuchNick_Parsing()
    {
        var noSuchNickCmd = new ErrNoSuchNick();
        noSuchNickCmd.Parse(":irc.example.com 401 john targetuser :No such nick/channel");

        Assert.That(noSuchNickCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(noSuchNickCmd.Nickname, Is.EqualTo("john"));
        Assert.That(noSuchNickCmd.TargetNick, Is.EqualTo("targetuser"));
    }

    [Test]
    public void TestErrNoSuchServer_Parsing()
    {
        var noSuchServerCmd = new ErrNoSuchServer();
        noSuchServerCmd.Parse(":irc.example.com 402 john target.server :No such server");

        Assert.That(noSuchServerCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(noSuchServerCmd.Nickname, Is.EqualTo("john"));
        Assert.That(noSuchServerCmd.TargetServer, Is.EqualTo("target.server"));
    }

    [Test]
    public void TestErrNoRecipient_Parsing()
    {
        var noRecipientCmd = new ErrNoRecipient();
        noRecipientCmd.Parse(":irc.example.com 411 john :No recipient given (PRIVMSG)");

        Assert.That(noRecipientCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(noRecipientCmd.Nickname, Is.EqualTo("john"));
        Assert.That(noRecipientCmd.Command, Is.EqualTo("PRIVMSG"));
    }

    [Test]
    public void TestErrCannotSendToChan_Parsing()
    {
        var cannotSendCmd = new ErrCannotSendToChan();
        cannotSendCmd.Parse(":irc.example.com 404 john #channel :Cannot send to channel");

        Assert.That(cannotSendCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(cannotSendCmd.Nickname, Is.EqualTo("john"));
        Assert.That(cannotSendCmd.ChannelName, Is.EqualTo("#channel"));
        Assert.That(cannotSendCmd.Reason, Is.EqualTo("Cannot send to channel"));
    }

    [Test]
    public void TestRplList_Parsing()
    {
        var listCmd = new RplList();
        listCmd.Parse(":irc.example.com 322 john #mychannel 42 :A nice channel");

        Assert.That(listCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(listCmd.Nickname, Is.EqualTo("john"));
        Assert.That(listCmd.ChannelName, Is.EqualTo("#mychannel"));
        Assert.That(listCmd.VisibleUserCount, Is.EqualTo(42));
        Assert.That(listCmd.Topic, Is.EqualTo("A nice channel"));
    }

    [Test]
    public void TestRplListStart_Parsing()
    {
        var listStartCmd = new RplListStart();
        listStartCmd.Parse(":irc.example.com 321 john :Channel Users Name");

        Assert.That(listStartCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(listStartCmd.Nickname, Is.EqualTo("john"));
    }

    [Test]
    public void TestRplListEnd_Parsing()
    {
        var listEndCmd = new RplListEnd();
        listEndCmd.Parse(":irc.example.com 323 john :End of /LIST");

        Assert.That(listEndCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(listEndCmd.Nickname, Is.EqualTo("john"));
    }

    [Test]
    public void TestPrivMsgCommand_ComplexCtcp()
    {
        var privMsgCmd = new PrivMsgCommand();
        privMsgCmd.Parse(":user!~user@host PRIVMSG #channel :\u0001VERSION mIRC v7.63 K.aIRc\u0001");

        Assert.That(privMsgCmd.IsCtcp, Is.True);
        Assert.That(privMsgCmd.CtcpCommand, Is.EqualTo("VERSION"));
        Assert.That(privMsgCmd.CtcpParameters, Is.EqualTo("mIRC v7.63 K.aIRc"));
        Assert.That(privMsgCmd.Source, Is.EqualTo("user!~user@host"));
        Assert.That(privMsgCmd.Target, Is.EqualTo("#channel"));
    }

    [Test]
    public void TestPrivMsgCommand_UnicodeMessage()
    {
        var privMsgCmd = new PrivMsgCommand();
        privMsgCmd.Parse(":user!~user@host PRIVMSG john :こんにちは世界");

        Assert.That(privMsgCmd.Source, Is.EqualTo("user!~user@host"));
        Assert.That(privMsgCmd.Target, Is.EqualTo("john"));
        Assert.That(privMsgCmd.Message, Is.EqualTo("こんにちは世界"));
    }

    [Test]
    public void TestJoinCommand_MultiChannelWithKeys()
    {
        var joinCmd = new JoinCommand();
        joinCmd.Parse("JOIN #channel1,#channel2 key1,key2");

        Assert.That(joinCmd.Channels.Count, Is.EqualTo(2));
        Assert.That(joinCmd.Channels[0].ChannelName, Is.EqualTo("#channel1"));
        Assert.That(joinCmd.Channels[0].Key, Is.EqualTo("key1"));
        Assert.That(joinCmd.Channels[1].ChannelName, Is.EqualTo("#channel2"));
        Assert.That(joinCmd.Channels[1].Key, Is.EqualTo("key2"));
    }

    [Test]
    public void TestPartCommand_MultiChannelWithMessage()
    {
        var partCmd = new PartCommand();
        partCmd.Parse("PART #channel1,#channel2 :Leaving both channels");

        Assert.That(partCmd.Channels.Count, Is.EqualTo(2));
        Assert.That(partCmd.Channels[0], Is.EqualTo("#channel1"));
        Assert.That(partCmd.Channels[1], Is.EqualTo("#channel2"));
        Assert.That(partCmd.PartMessage, Is.EqualTo("Leaving both channels"));
    }

    [Test]
    public void TestListCommand_ComplexFilter()
    {
        var listCmd = new ListCommand();
        listCmd.Parse("LIST C<60");

        Assert.That(listCmd.FilterType, Is.EqualTo(ListFilterType.Created));
        Assert.That(listCmd.Comparison, Is.EqualTo(ComparisonType.LessThan));
        Assert.That(listCmd.FilterValue, Is.EqualTo(60));
    }

    [Test]
    public void TestNameReply_ComplexPrefixes()
    {
        var namesReply = new RplNameReply();
        namesReply.Parse(":irc.example.com 353 john = #channel :@@@superop @@op +voice normal");

        Assert.That(
            namesReply.Members,
            Is.EqualTo(new[] { "@@@superop", "@@op", "+voice", "normal" })
        );
    }

    [Test]
    public void TestErrorCommand_ComplexMessage()
    {
        var errorCmd = new ErrorCommand();
        errorCmd.Parse(":server.com ERROR :Closing Link: nick[host] (Ping timeout: 180 seconds)");

        Assert.That(errorCmd.Source, Is.EqualTo("server.com"));
        Assert.That(
            errorCmd.Message,
            Is.EqualTo("Closing Link: nick[host] (Ping timeout: 180 seconds)")
        );
    }

    [Test]
    public void TestIsonCommand_MultipleNicknames()
    {
        var isonCmd = new IsonCommand();
        isonCmd.Parse("ISON nick1 nick2 nick3");

        Assert.That(
            isonCmd.Nicknames,
            Is.EqualTo(new[] { "nick1", "nick2", "nick3" })
        );
    }

    [Test]
    public void TestModeCommand_ComplexMode()
    {
        var modeCmd = new ModeCommand();
        modeCmd.Parse(":nick!user@host MODE #channel +beI alice!*@* bob!*@example.com charlie!*@*.edu");

        Assert.That(modeCmd.Source, Is.EqualTo("nick!user@host"));
        Assert.That(modeCmd.Target, Is.EqualTo("#channel"));
    }

    // [Test]
    // public void TestCapCommand_MultilineResponse()
    // {
    //     var capCmd = new CapCommand();
    //     capCmd.Parse(":server 005 nickname cap1 cap2 cap3 :multi line caps");
    //
    //     Assert.That(capCmd.Subcommand, Is.EqualTo(CapCommand.CapSubcommand.LS));
    //     Assert.That(
    //         capCmd.Capabilities.Select(c => c.Name),
    //         Is.EqualTo(new[] { "cap1", "cap2", "cap3" })
    //     );
    // }

    [Test]
    public void TestRplTopic_SpecialCharacters()
    {
        var topicCmd = new RplTopic();
        topicCmd.Parse(":irc.example.com 332 john #channel :A topic with # special @ characters!");

        Assert.That(topicCmd.ServerName, Is.EqualTo("irc.example.com"));
        Assert.That(topicCmd.Nickname, Is.EqualTo("john"));
        Assert.That(topicCmd.ChannelName, Is.EqualTo("#channel"));
        Assert.That(topicCmd.Topic, Is.EqualTo("A topic with # special @ characters!"));
    }
}
