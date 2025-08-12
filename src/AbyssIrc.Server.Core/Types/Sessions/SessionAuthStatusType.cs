namespace AbyssIrc.Server.Core.Types.Sessions;

[Flags]
public enum SessionAuthStatusType
{
    None = 0,
    Nickname = 1,
    Username = 2,
    Completed = Nickname | Username,
}
