using AbyssIrc.Core.Extensions;

namespace AbyssIrc.Tests;

[TestFixture]
public class ExtensionMethodTests
{
    [Test]
    public void DateTimeExtensions_ToUnixTimestamp_ShouldConvertCorrectly()
    {
        var testDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Assert.That(
            testDate.ToUnixTimestamp(),
            Is.EqualTo(0),
            "Unix epoch should return 0"
        );

        var someDate = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var expectedTimestamp = (long)(someDate - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

        Assert.That(
            someDate.ToUnixTimestamp(),
            Is.EqualTo(expectedTimestamp),
            "Unix timestamp conversion incorrect"
        );
    }
}
