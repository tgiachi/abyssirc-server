using AbyssIrc.Core.Interfaces.Services;

namespace AbyssIrc.Server.Interfaces.Services.System;

public interface IDiagnosticSystemService : IAbyssStarStopService
{
    string PidFileName { get; }
}
