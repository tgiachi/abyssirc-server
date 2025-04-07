using System.ComponentModel.DataAnnotations;

namespace AbyssIrc.Server.Core.Data.Rest;

public record LoginRequestData([Required] string Username, [Required] string Password);
