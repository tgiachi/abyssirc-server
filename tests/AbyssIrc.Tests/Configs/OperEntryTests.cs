using NUnit.Framework;
using AbyssIrc.Core.Data.Configs.Sections.Oper;
using System;

namespace AbyssIrc.Tests.Configs;

[TestFixture]
public class OperEntryTests
{
    [Test]
    public void OperEntry_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var operEntry = new OperEntry
        {
            Username = "testadmin",
            Password = "securepassword",
            Host = "192.168.1.*"
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operEntry.Username, Is.EqualTo("testadmin"));
            Assert.That(operEntry.Password, Is.EqualTo("securepassword"));
            Assert.That(operEntry.Host, Is.EqualTo("192.168.1.*"));
        });
    }

    [Test]
    public void OperEntry_CanBeCreated_WithDomainHost()
    {
        // Arrange & Act
        var operEntry = new OperEntry
        {
            Username = "webadmin",
            Password = "complexpassword",
            Host = "*.example.com"
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operEntry.Username, Is.EqualTo("webadmin"));
            Assert.That(operEntry.Password, Is.EqualTo("complexpassword"));
            Assert.That(operEntry.Host, Is.EqualTo("*.example.com"));
        });
    }

    [Test]
    public void OperEntry_CanHandleEmptyOrNullValues()
    {
        // Arrange & Act
        var operEntry = new OperEntry
        {
            Username = "",
            Password = null,
            Host = null
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operEntry.Username, Is.EqualTo(""));
            Assert.That(operEntry.Password, Is.Null);
            Assert.That(operEntry.Host, Is.Null);
        });
    }

    [Test]
    public void OperEntry_SupportsLongValues()
    {
        // Arrange
        string longUsername = new string('a', 100);
        string longPassword = new string('b', 200);
        string longHost = new string('c', 255);

        // Act
        var operEntry = new OperEntry
        {
            Username = longUsername,
            Password = longPassword,
            Host = longHost
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operEntry.Username, Is.EqualTo(longUsername));
            Assert.That(operEntry.Password, Is.EqualTo(longPassword));
            Assert.That(operEntry.Host, Is.EqualTo(longHost));
        });
    }

    [Test]
    public void OperEntry_CanBeCompared()
    {
        // Arrange
        var operEntry1 = new OperEntry
        {
            Username = "admin1",
            Password = "pass1",
            Host = "192.168.1.1"
        };

        var operEntry2 = new OperEntry
        {
            Username = "admin1",
            Password = "pass1",
            Host = "192.168.1.1"
        };

        var operEntry3 = new OperEntry
        {
            Username = "admin2",
            Password = "pass2",
            Host = "192.168.1.2"
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(operEntry1.Username, Is.EqualTo(operEntry2.Username));
            Assert.That(operEntry1.Password, Is.EqualTo(operEntry2.Password));
            Assert.That(operEntry1.Host, Is.EqualTo(operEntry2.Host));

            Assert.That(operEntry1.Username, Is.Not.EqualTo(operEntry3.Username));
            Assert.That(operEntry1.Password, Is.Not.EqualTo(operEntry3.Password));
            Assert.That(operEntry1.Host, Is.Not.EqualTo(operEntry3.Host));
        });
    }
}
