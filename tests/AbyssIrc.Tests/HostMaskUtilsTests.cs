using AbyssIrc.Core.Utils;
using AbyssIrc.Server.Core.Utils.Hosts;
using NUnit.Framework;

namespace AbyssIrc.Tests;

public class HostMaskUtilsTests
{
    [Test]
    public void IsHostMaskMatch_WithFullFormat_MatchesCorrectly()
    {
        // Full format matches
        Assert.That(HostMaskUtils.IsHostMaskMatch("nick!user@host.com", "nick!user@host.com"), Is.True);
        Assert.That(HostMaskUtils.IsHostMaskMatch("ni*!user@*.com", "nick!user@host.com"), Is.True);
        Assert.That(HostMaskUtils.IsHostMaskMatch("*!*@*.com", "nick!user@host.com"), Is.True);
        Assert.That(HostMaskUtils.IsHostMaskMatch("n*!u*@*.com", "nick!user@host.com"), Is.True);

        // Non-matches
        Assert.That(HostMaskUtils.IsHostMaskMatch("other!user@host.com", "nick!user@host.com"), Is.False);
        Assert.That(HostMaskUtils.IsHostMaskMatch("nick!user@host.org", "nick!user@host.com"), Is.False);
    }

    [Test]
    public void IsHostMaskMatch_WithPartialFormat_MatchesCorrectly()
    {
        // User@Host format
        Assert.That(HostMaskUtils.IsHostMaskMatch("user@*.tim.it", "nickname!user@mario.tim.it"), Is.True);
        Assert.That(HostMaskUtils.IsHostMaskMatch("*@*.tim.it", "nickname!anyuser@mario.tim.it"), Is.True);
        Assert.That(HostMaskUtils.IsHostMaskMatch("user@*.org", "nickname!user@mario.tim.it"), Is.False);

        // Host only format
        Assert.That(HostMaskUtils.IsHostMaskMatch("*.tim.it", "nickname!user@mario.tim.it"), Is.True);
        Assert.That(HostMaskUtils.IsHostMaskMatch("*.org", "nickname!user@mario.tim.it"), Is.False);
    }

    [Test]
    public void IsHostMaskMatch_WithWildcards_MatchesEverything()
    {
        Assert.That(HostMaskUtils.IsHostMaskMatch("*", "nickname!user@host.com"), Is.True);
        Assert.That(HostMaskUtils.IsHostMaskMatch("*@*", "nickname!user@host.com"), Is.True);
        Assert.That(HostMaskUtils.IsHostMaskMatch("*!*@*", "nickname!user@host.com"), Is.True);
    }

    [Test]
    public void ParseUserMask_WithValidMasks_ExtractsComponents()
    {
        // Full mask
        HostMaskUtils.ParseUserMask("nick!user@host.com", out var nick, out var user, out var host);
        Assert.That(nick, Is.EqualTo("nick"));
        Assert.That(user, Is.EqualTo("user"));
        Assert.That(host, Is.EqualTo("host.com"));

        // User@host format
        HostMaskUtils.ParseUserMask("user@host.com", out nick, out user, out host);
        Assert.That(nick, Is.EqualTo(""));
        Assert.That(user, Is.EqualTo("user"));
        Assert.That(host, Is.EqualTo("host.com"));

        // Host only
        HostMaskUtils.ParseUserMask("host.com", out nick, out user, out host);
        Assert.That(nick, Is.EqualTo(""));
        Assert.That(user, Is.EqualTo(""));
        Assert.That(host, Is.EqualTo("host.com"));

        // Nick!user format
        HostMaskUtils.ParseUserMask("nick!user", out nick, out user, out host);
        Assert.That(nick, Is.EqualTo("nick"));
        Assert.That(user, Is.EqualTo("user"));
        Assert.That(host, Is.EqualTo(""));
    }

    [Test]
    public void IsHostMaskMatch_WithSpecificExamples_MatchesAsExpected()
    {
        // Your specific example
        Assert.That(HostMaskUtils.IsHostMaskMatch("user@*.tim.it", "nickname!user@mario.tim.it"), Is.True);

        // Some other common patterns
        Assert.That(HostMaskUtils.IsHostMaskMatch("*@192.168.*.*", "nick!user@192.168.1.5"), Is.True);

        // This test verifies the username part matches correctly
        Assert.That(
            HostMaskUtils.IsHostMaskMatch("bob@*", "alice!bob@any.host.org"),
            Is.True,
            "Should match because 'bob' matches the username, not the nickname"
        );
        Assert.That(
            HostMaskUtils.IsHostMaskMatch("bob@*", "bob!alice@host.org"),
            Is.False,
            "Should not match because 'bob' doesn't match username 'alice'"
        );

        // Question mark tests
        Assert.That(HostMaskUtils.IsHostMaskMatch("nick?!user@host.com", "nick1!user@host.com"), Is.True);
        Assert.That(HostMaskUtils.IsHostMaskMatch("nick?!user@host.com", "nickXX!user@host.com"), Is.False);
    }

    [Test]
    public void IsHostMaskMatch_UsernamePatternsMatchCorrectly()
    {
        // Test username patterns matching the username (not the nickname)
        Assert.That(
            HostMaskUtils.IsHostMaskMatch("bob@*", "alice!bob@host.com"),
            Is.True,
            "Username pattern should match the username part"
        );
        Assert.That(
            HostMaskUtils.IsHostMaskMatch("bob@*", "bob!alice@host.com"),
            Is.False,
            "Username pattern should not match if username is different"
        );

        // More complex test cases
        Assert.That(HostMaskUtils.IsHostMaskMatch("b*@*", "nick!bob@host.com"), Is.True);
        Assert.That(HostMaskUtils.IsHostMaskMatch("*b@*", "nick!rob@host.com"), Is.True);
        Assert.That(HostMaskUtils.IsHostMaskMatch("*b@*", "nick!alice@host.com"), Is.False);
    }
}
