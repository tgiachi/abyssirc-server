using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Network.Types;

namespace AbyssIrc.Tests;

public class TestRplMessages
{
    [Test]
    public void TestRplWelcome()
    {
        // Test parsing and writing of RPL_WELCOME (001)
        var welcomeMessage = ":irc.example.net 001 Mario :Welcome to the Internet Relay Chat Network Mario!user@host";
        var rplWelcome = new RplWelcome();
        rplWelcome.Parse(welcomeMessage);

        Assert.That(rplWelcome.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplWelcome.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplWelcome.NetworkName, Is.EqualTo("Internet Relay Chat"));
        Assert.That(rplWelcome.HostMask, Is.EqualTo("Mario!user@host"));

        var writtenMessage = rplWelcome.Write();
        Assert.That(writtenMessage, Does.Contain("001"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplYourHost()
    {
        // Test parsing and writing of RPL_YOURHOST (002)
        var yourHostMessage = ":irc.example.net 002 Mario :Your host is irc.example.net, running version AbyssIRC-1.0.0";
        var rplYourHost = new RplYourHost();
        rplYourHost.Parse(yourHostMessage);

        Assert.That(rplYourHost.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplYourHost.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplYourHost.Version, Is.EqualTo("AbyssIRC-1.0.0"));

        var writtenMessage = rplYourHost.Write();
        Assert.That(writtenMessage, Does.Contain("002"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplCreated()
    {
        // Test parsing and writing of RPL_CREATED (003)
        var createdMessage = ":irc.example.net 003 Mario :This server was created Mon Jan 15 2024 at 10:15:00 UTC";
        var rplCreated = new RplCreated();
        rplCreated.Parse(createdMessage);

        Assert.That(rplCreated.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplCreated.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplCreated.CreationMessage, Is.EqualTo("This server was created Mon Jan 15 2024 at 10:15:00 UTC"));

        var writtenMessage = rplCreated.Write();
        Assert.That(writtenMessage, Does.Contain("003"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplMyInfo()
    {
        // Test parsing and writing of RPL_MYINFO (004)
        var myInfoMessage = ":irc.example.net 004 Mario irc.example.net AbyssIRC-1.0.0 aoOirw biklmnopstv";
        var rplMyInfo = new RplMyInfo();
        rplMyInfo.Parse(myInfoMessage);

        Assert.That(rplMyInfo.TargetNick, Is.EqualTo("Mario"));
        Assert.That(rplMyInfo.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplMyInfo.Version, Is.EqualTo("AbyssIRC-1.0.0"));
        Assert.That(rplMyInfo.UserModes, Is.EqualTo("aoOirw"));
        Assert.That(rplMyInfo.ChannelModes, Is.EqualTo("biklmnopstv"));

        var writtenMessage = rplMyInfo.Write();
        Assert.That(writtenMessage, Does.Contain("004"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplLuserClient()
    {
        // Test parsing and writing of RPL_LUSERCLIENT (251)
        var luserClientMessage = ":irc.example.net 251 Mario :There are 3 users and 1 invisible on 2 servers";
        var rplLuserClient = new RplLuserClient();
        rplLuserClient.Parse(luserClientMessage);

        Assert.That(rplLuserClient.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplLuserClient.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplLuserClient.VisibleUsers, Is.EqualTo(3));
        Assert.That(rplLuserClient.InvisibleUsers, Is.EqualTo(1));
        Assert.That(rplLuserClient.Servers, Is.EqualTo(2));

        var writtenMessage = rplLuserClient.Write();
        Assert.That(writtenMessage, Does.Contain("251"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplMotd()
    {
        // Test parsing and writing of RPL_MOTD (372)
        var motdMessage = ":irc.example.net 372 Mario :- Welcome to AbyssIRC Server!";
        var rplMotd = new RplMotd();
        rplMotd.Parse(motdMessage);

        Assert.That(rplMotd.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplMotd.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplMotd.Text, Is.EqualTo("Welcome to AbyssIRC Server!"));

        var writtenMessage = rplMotd.Write();
        Assert.That(writtenMessage, Does.Contain("372"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplEndOfMotd()
    {
        // Test parsing and writing of RPL_ENDOFMOTD (376)
        var endOfMotdMessage = ":irc.example.net 376 Mario :End of /MOTD command.";
        var rplEndOfMotd = new RplEndOfMotd();
        rplEndOfMotd.Parse(endOfMotdMessage);

        Assert.That(rplEndOfMotd.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplEndOfMotd.ServerName, Is.EqualTo("irc.example.net"));

        var writtenMessage = rplEndOfMotd.Write();
        Assert.That(writtenMessage, Does.Contain("376"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplList()
    {
        // Test parsing and writing of RPL_LIST (322)
        var listMessage = ":irc.example.net 322 Mario #channel 42 :Channel topic goes here";
        var rplList = new RplList();
        rplList.Parse(listMessage);

        Assert.That(rplList.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplList.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplList.ChannelName, Is.EqualTo("#channel"));
        Assert.That(rplList.VisibleUserCount, Is.EqualTo(42));
        Assert.That(rplList.Topic, Is.EqualTo("Channel topic goes here"));

        var writtenMessage = rplList.Write();
        Assert.That(writtenMessage, Does.Contain("322"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplTopic()
    {
        // Test parsing and writing of RPL_TOPIC (332)
        var topicMessage = ":irc.example.net 332 Mario #channel :This is the channel topic";
        var rplTopic = new RplTopic();
        rplTopic.Parse(topicMessage);

        Assert.That(rplTopic.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplTopic.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplTopic.ChannelName, Is.EqualTo("#channel"));
        Assert.That(rplTopic.Topic, Is.EqualTo("This is the channel topic"));

        var writtenMessage = rplTopic.Write();
        Assert.That(writtenMessage, Does.Contain("332"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplNameReply()
    {
        // Test parsing and writing of RPL_NAMREPLY (353)
        var nameReplyMessage = ":irc.example.net 353 Mario = #channel :nick1 @nick2 +nick3";
        var rplNameReply = new RplNameReply();
        rplNameReply.Parse(nameReplyMessage);

        Assert.That(rplNameReply.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplNameReply.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplNameReply.ChannelName, Is.EqualTo("#channel"));
        Assert.That(rplNameReply.Visibility, Is.EqualTo(ChannelVisibility.Public));
        Assert.That(rplNameReply.Members, Has.Count.EqualTo(3));
        Assert.That(rplNameReply.Members, Does.Contain("nick1"));
        Assert.That(rplNameReply.Members, Does.Contain("@nick2"));
        Assert.That(rplNameReply.Members, Does.Contain("+nick3"));

        var writtenMessage = rplNameReply.Write();
        Assert.That(writtenMessage, Does.Contain("353"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }


    [Test]
    public void TestRplNoTopic()
    {
        // Test parsing and writing of RPL_NOTOPIC (331)
        var noTopicMessage = ":irc.example.net 331 Mario #channel :No topic is set";
        var rplNoTopic = new RplNoTopic();
        rplNoTopic.Parse(noTopicMessage);

        Assert.That(rplNoTopic.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplNoTopic.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplNoTopic.ChannelName, Is.EqualTo("#channel"));

        var writtenMessage = rplNoTopic.Write();
        Assert.That(writtenMessage, Does.Contain("331"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplTopicWhoTime()
    {
        // Test parsing and writing of RPL_TOPICWHOTIME (333)
        var topicWhoTimeMessage = ":irc.example.net 333 Mario #channel nick!user@host 1609459200";
        var rplTopicWhoTime = new RplTopicWhoTime();
        rplTopicWhoTime.Parse(topicWhoTimeMessage);

        Assert.That(rplTopicWhoTime.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplTopicWhoTime.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplTopicWhoTime.ChannelName, Is.EqualTo("#channel"));
        Assert.That(rplTopicWhoTime.SetterMask, Is.EqualTo("nick!user@host"));
        Assert.That(rplTopicWhoTime.SetTimestamp, Is.EqualTo(1609459200));

        var writtenMessage = rplTopicWhoTime.Write();
        Assert.That(writtenMessage, Does.Contain("333"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplListStart()
    {
        // Test parsing and writing of RPL_LISTSTART (321)
        var listStartMessage = ":irc.example.net 321 Mario :Channel Users Name";
        var rplListStart = new RplListStart();
        rplListStart.Parse(listStartMessage);

        Assert.That(rplListStart.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplListStart.ServerName, Is.EqualTo("irc.example.net"));

        var writtenMessage = rplListStart.Write();
        Assert.That(writtenMessage, Does.Contain("321"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplListEnd()
    {
        // Test parsing and writing of RPL_LISTEND (323)
        var listEndMessage = ":irc.example.net 323 Mario :End of /LIST";
        var rplListEnd = new RplListEnd();
        rplListEnd.Parse(listEndMessage);

        Assert.That(rplListEnd.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplListEnd.ServerName, Is.EqualTo("irc.example.net"));

        var writtenMessage = rplListEnd.Write();
        Assert.That(writtenMessage, Does.Contain("323"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplEndOfNames()
    {
        // Test parsing and writing of RPL_ENDOFNAMES (366)
        var endOfNamesMessage = ":irc.example.net 366 Mario #channel :End of /NAMES list";
        var rplEndOfNames = new RplEndOfNames();
        rplEndOfNames.Parse(endOfNamesMessage);

        Assert.That(rplEndOfNames.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplEndOfNames.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplEndOfNames.ChannelName, Is.EqualTo("#channel"));
        Assert.That(rplEndOfNames.Message, Is.EqualTo("End of /NAMES list"));

        var writtenMessage = rplEndOfNames.Write();
        Assert.That(writtenMessage, Does.Contain("366"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplLuserOp()
    {
        // Test parsing and writing of RPL_LUSEROP (252)
        var luserOpMessage = ":irc.example.net 252 Mario 3 :operator(s) online";
        var rplLuserOp = new RplLuserOp();
        rplLuserOp.Parse(luserOpMessage);

        Assert.That(rplLuserOp.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplLuserOp.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplLuserOp.OperatorCount, Is.EqualTo(3));

        var writtenMessage = rplLuserOp.Write();
        Assert.That(writtenMessage, Does.Contain("252"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplLuserUnknown()
    {
        // Test parsing and writing of RPL_LUSERUNKNOWN (253)
        var luserUnknownMessage = ":irc.example.net 253 Mario 2 :unknown connection(s)";
        var rplLuserUnknown = new RplLuserUnknown();
        rplLuserUnknown.Parse(luserUnknownMessage);

        Assert.That(rplLuserUnknown.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplLuserUnknown.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplLuserUnknown.UnknownCount, Is.EqualTo(2));

        var writtenMessage = rplLuserUnknown.Write();
        Assert.That(writtenMessage, Does.Contain("253"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplLuserChannels()
    {
        // Test parsing and writing of RPL_LUSERCHANNELS (254)
        var luserChannelsMessage = ":irc.example.net 254 Mario 15 :channels formed";
        var rplLuserChannels = new RplLuserChannels();
        rplLuserChannels.Parse(luserChannelsMessage);

        Assert.That(rplLuserChannels.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplLuserChannels.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplLuserChannels.ChannelCount, Is.EqualTo(15));

        var writtenMessage = rplLuserChannels.Write();
        Assert.That(writtenMessage, Does.Contain("254"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplLuserMe()
    {
        // Test parsing and writing of RPL_LUSERME (255)
        var luserMeMessage = ":irc.example.net 255 Mario :I have 10 clients and 2 servers";
        var rplLuserMe = new RplLuserMe();
        rplLuserMe.Parse(luserMeMessage);

        Assert.That(rplLuserMe.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplLuserMe.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplLuserMe.ClientCount, Is.EqualTo(10));
        Assert.That(rplLuserMe.ServerCount, Is.EqualTo(2));

        var writtenMessage = rplLuserMe.Write();
        Assert.That(writtenMessage, Does.Contain("255"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplVersion()
    {
        // Test parsing and writing of RPL_VERSION (351)
        var versionMessage = ":irc.example.net 351 Mario AbyssIRC-1.0.0 irc.example.net :Additional comments here";
        var rplVersion = new RplVersion();
        rplVersion.Parse(versionMessage);

        Assert.That(rplVersion.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplVersion.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplVersion.Version, Is.EqualTo("AbyssIRC-1.0.0"));
        Assert.That(rplVersion.ServerHost, Is.EqualTo("irc.example.net"));
        Assert.That(rplVersion.Comments, Is.EqualTo("Additional comments here"));

        var writtenMessage = rplVersion.Write();
        Assert.That(writtenMessage, Does.Contain("351"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplTime()
    {
        // Test parsing and writing of RPL_TIME (391)
        var timeMessage = ":irc.example.net 391 Mario irc.example.net :Current local time is Mon Jan 01 12:34:56 2024";
        var rplTime = new RplTime();
        rplTime.Parse(timeMessage);

        Assert.That(rplTime.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplTime.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplTime.TimeServer, Is.EqualTo("irc.example.net"));
        Assert.That(rplTime.TimeString, Is.EqualTo("Current local time is Mon Jan 01 12:34:56 2024"));

        var writtenMessage = rplTime.Write();
        Assert.That(writtenMessage, Does.Contain("391"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplAdminMe()
    {
        // Test parsing and writing of RPL_ADMINME (256)
        var adminMeMessage = ":irc.example.net 256 Mario :Administrative info about server.example.net";
        var rplAdminMe = new RplAdminMe();
        rplAdminMe.Parse(adminMeMessage);

        Assert.That(rplAdminMe.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplAdminMe.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplAdminMe.QueryServer, Is.EqualTo("server.example.net"));

        var writtenMessage = rplAdminMe.Write();
        Assert.That(writtenMessage, Does.Contain("256"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplAdminLoc1()
    {
        // Test parsing and writing of RPL_ADMINLOC1 (257)
        var adminLoc1Message = ":irc.example.net 257 Mario :New York, NY, USA";
        var rplAdminLoc1 = new RplAdminLoc1();
        rplAdminLoc1.Parse(adminLoc1Message);

        Assert.That(rplAdminLoc1.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplAdminLoc1.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplAdminLoc1.LocationInfo, Is.EqualTo("New York, NY, USA"));

        var writtenMessage = rplAdminLoc1.Write();
        Assert.That(writtenMessage, Does.Contain("257"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplAdminLoc2()
    {
        // Test parsing and writing of RPL_ADMINLOC2 (258)
        var adminLoc2Message = ":irc.example.net 258 Mario :Example Network Operations Center";
        var rplAdminLoc2 = new RplAdminLoc2();
        rplAdminLoc2.Parse(adminLoc2Message);

        Assert.That(rplAdminLoc2.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplAdminLoc2.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplAdminLoc2.AffiliationInfo, Is.EqualTo("Example Network Operations Center"));

        var writtenMessage = rplAdminLoc2.Write();
        Assert.That(writtenMessage, Does.Contain("258"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplAdminEmail()
    {
        // Test parsing and writing of RPL_ADMINEMAIL (259)
        var adminEmailMessage = ":irc.example.net 259 Mario :admin@example.com";
        var rplAdminEmail = new RplAdminEmail();
        rplAdminEmail.Parse(adminEmailMessage);

        Assert.That(rplAdminEmail.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplAdminEmail.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplAdminEmail.EmailAddress, Is.EqualTo("admin@example.com"));

        var writtenMessage = rplAdminEmail.Write();
        Assert.That(writtenMessage, Does.Contain("259"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplStatsUptime()
    {
        // Test parsing and writing of RPL_STATSUPTIME (242)
        var statsUptimeMessage = ":irc.example.net 242 Mario :Server Up 3 days, 2:34:56";
        var rplStatsUptime = new RplStatsUptime();
        rplStatsUptime.Parse(statsUptimeMessage);

        Assert.That(rplStatsUptime.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplStatsUptime.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplStatsUptime.UptimeMessage, Is.EqualTo("Server Up 3 days, 2:34:56"));

        var writtenMessage = rplStatsUptime.Write();
        Assert.That(writtenMessage, Does.Contain("242"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplLocal()
    {
        // Test parsing and writing of RPL_LOCALUSERS (265)
        var localUsersMessage = ":irc.example.net 265 Mario 42 50 :Current local users 42, max 50";
        var rplLocalUsers = new RplLocalUsers();
        rplLocalUsers.Parse(localUsersMessage);

        Assert.That(rplLocalUsers.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplLocalUsers.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplLocalUsers.CurrentLocalUsers, Is.EqualTo(42));
        Assert.That(rplLocalUsers.MaxLocalUsers, Is.EqualTo(50));

        var writtenMessage = rplLocalUsers.Write();
        Assert.That(writtenMessage, Does.Contain("265"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplGlobal()
    {
        // Test parsing and writing of RPL_GLOBALUSERS (266)
        var globalUsersMessage = ":irc.example.net 266 Mario 142 200 :Current global users 142, max 200";
        var rplGlobalUsers = new RplGlobalUsers();
        rplGlobalUsers.Parse(globalUsersMessage);

        Assert.That(rplGlobalUsers.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplGlobalUsers.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplGlobalUsers.CurrentGlobalUsers, Is.EqualTo(142));
        Assert.That(rplGlobalUsers.MaxGlobalUsers, Is.EqualTo(200));

        var writtenMessage = rplGlobalUsers.Write();
        Assert.That(writtenMessage, Does.Contain("266"));
        Assert.That(writtenMessage, Does.Contain("Mario"));
    }

    [Test]
    public void TestRplWhoisUser()
    {
        // Test parsing of RPL_WHOISUSER (311)
        var whoisUserMessage = ":irc.example.net 311 Mario targetuser username hostname * :Real Name";
        var rplWhoisUser = new RplWhoisUser();
        rplWhoisUser.Parse(whoisUserMessage);

        Assert.That(rplWhoisUser.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplWhoisUser.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplWhoisUser.QueriedNick, Is.EqualTo("targetuser"));
        Assert.That(rplWhoisUser.Username, Is.EqualTo("username"));
        Assert.That(rplWhoisUser.Hostname, Is.EqualTo("hostname"));
        Assert.That(rplWhoisUser.RealName, Is.EqualTo("Real Name"));

        var writtenMessage = rplWhoisUser.Write();
        Assert.That(writtenMessage, Does.Contain("311"));
        Assert.That(writtenMessage, Does.Contain("Mario"));

        // Test using Create method
        var createdRplWhoisUser = RplWhoisUser.Create(
            "irc.example.net",
            "Mario",
            "targetuser",
            "username",
            "hostname",
            "Real Name"
        );

        Assert.That(createdRplWhoisUser.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(createdRplWhoisUser.Nickname, Is.EqualTo("Mario"));
    }

    [Test]
    public void TestRplWhoisServer()
    {
        // Test parsing of RPL_WHOISSERVER (312)
        var whoisServerMessage = ":irc.example.net 312 Mario targetuser server.example.net :Server connection info";
        var rplWhoisServer = new RplWhoisServer();
        rplWhoisServer.Parse(whoisServerMessage);

        Assert.That(rplWhoisServer.Nickname, Is.EqualTo("Mario"));
        Assert.That(rplWhoisServer.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(rplWhoisServer.QueriedNick, Is.EqualTo("targetuser"));
        Assert.That(rplWhoisServer.UserServer, Is.EqualTo("server.example.net"));
        Assert.That(rplWhoisServer.ServerInfo, Is.EqualTo("Server connection info"));

        var writtenMessage = rplWhoisServer.Write();
        Assert.That(writtenMessage, Does.Contain("312"));
        Assert.That(writtenMessage, Does.Contain("Mario"));

        // Test using Create method
        var createdRplWhoisServer = RplWhoisServer.Create(
            "irc.example.net",
            "Mario",
            "targetuser",
            "server.example.net",
            "Server connection info"
        );

        Assert.That(createdRplWhoisServer.ServerName, Is.EqualTo("irc.example.net"));
        Assert.That(createdRplWhoisServer.Nickname, Is.EqualTo("Mario"));
    }
}
