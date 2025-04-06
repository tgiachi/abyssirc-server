using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Commands.Replies;
using AbyssIrc.Server.Core.Interfaces.Modules;
using AbyssIrc.Server.Extensions;
using AbyssIrc.Server.Handlers;

namespace AbyssIrc.Server.Plugins.Core.Modules;

public class MessageAndHandlerContainerModule : IAbyssContainerModule
{
    public IServiceCollection InitializeModule(IServiceCollection services)
    {
        services
            .RegisterIrcCommandListener<QuitMessageHandler>(new QuitCommand())
            .RegisterIrcCommandListener<NickUserHandler>(new UserCommand())
            .RegisterIrcCommandListener<NickUserHandler>(new NickCommand())
            .RegisterIrcCommandListener<NickUserHandler>(new IsonCommand())
            .RegisterIrcCommandListener<NickUserHandler>(new ModeCommand())
            .RegisterIrcCommandListener<PingPongHandler>(new PingCommand())
            .RegisterIrcCommandListener<PingPongHandler>(new PongCommand())
            .RegisterIrcCommandListener<ServerCommandsListener>(new RestartCommand())
            .RegisterIrcCommandListener<PassHandler>(new PassCommand())
            .RegisterIrcCommandListener<PrivMsgHandler>(new PrivMsgCommand())
            .RegisterIrcCommandListener<TimeHandler>(new TimeCommand())
            .RegisterIrcCommandListener<InviteHandler>(new InviteCommand())

            //Channel management
            .RegisterIrcCommandListener<ChannelsHandler>(new PrivMsgCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new JoinCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new PartCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new ModeCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new ListCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new NamesCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new TopicCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new PartCommand())
            .RegisterIrcCommandListener<ChannelsHandler>(new KickCommand())
            .RegisterIrcCommandListener<WhoHandler>(new WhoCommand())
            .RegisterIrcCommandListener<WhoHandler>(new WhoIsCommand())
            .RegisterIrcCommandListener<OperHandler>(new OperCommand())
            .RegisterIrcCommandListener<OperHandler>(new KillCommand())
            ;


        services
            .RegisterIrcCommand(new RplMyInfo())
            .RegisterIrcCommand(new RplWelcome())
            .RegisterIrcCommand(new RplYourHost())
            .RegisterIrcCommand(new CapCommand())
            .RegisterIrcCommand(new NickCommand())
            .RegisterIrcCommand(new UserCommand())
            .RegisterIrcCommand(new NoticeCommand())
            .RegisterIrcCommand(new PingCommand())
            .RegisterIrcCommand(new PongCommand())
            .RegisterIrcCommand(new PrivMsgCommand())
            .RegisterIrcCommand(new ModeCommand())
            .RegisterIrcCommand(new QuitCommand())
            .RegisterIrcCommand(new IsonCommand())
            .RegisterIrcCommand(new UserhostCommand())
            .RegisterIrcCommand(new PassCommand())
            .RegisterIrcCommand(new ListCommand())
            .RegisterIrcCommand(new AdminCommand())
            .RegisterIrcCommand(new InfoCommand())
            .RegisterIrcCommand(new JoinCommand())
            .RegisterIrcCommand(new PartCommand())
            .RegisterIrcCommand(new ListCommand())
            .RegisterIrcCommand(new RestartCommand())
            .RegisterIrcCommand(new NamesCommand())
            .RegisterIrcCommand(new TopicCommand())
            .RegisterIrcCommand(new KickCommand())
            .RegisterIrcCommand(new InviteCommand())
            .RegisterIrcCommand(new TimeCommand())
            .RegisterIrcCommand(new OperCommand())
            .RegisterIrcCommand(new KillCommand())
            .RegisterIrcCommand(new WhoCommand())
            .RegisterIrcCommand(new WhoIsCommand())
            ;


        services.RegisterIrcHandler<ConnectionHandler>()
            .RegisterIrcHandler<NickUserHandler>()
            .RegisterIrcHandler<PingPongHandler>()
            .RegisterIrcHandler<PrivMsgHandler>()
            .RegisterIrcHandler<QuitMessageHandler>()
            .RegisterIrcHandler<WelcomeHandler>()
            ;

        return services;
    }
}
