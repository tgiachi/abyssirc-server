using AbyssIrc.Server.Network.Types;

namespace AbyssIrc.Server.Network.Data;

public record ServerDataObject(string Id, object Server, ServerConnectionType ServerType);

