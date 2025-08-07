using System.Security.Cryptography;
using System.Text;

namespace AbyssIrc.Core.Extensions;

public static class ByteUtilsMethodEx
{
    /// <summary>
    ///     Perform Md5 checksum on a byte array
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetMd5Checksum(this byte[] value)
    {
        var stream = new MemoryStream();
        stream.Write(value, 0, value.Length);

        //important: get back to start of stream
        stream.Seek(0, SeekOrigin.Begin);

        //get a string value for the MD5 hash.
        using var md5Instance = MD5.Create();
        var hashResult = md5Instance.ComputeHash(stream);

        //***I did some formatting here, you may not want to remove the dashes, or use lower case depending on your application
        return BitConverter.ToString(hashResult).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    ///     Perform Md5 checksum on a string
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetMd5Checksum(this string value)
    {
        return Encoding.UTF8.GetBytes(value).GetMd5Checksum();
    }
}
