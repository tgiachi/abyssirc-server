/**
 * AbyssIRC Server JavaScript API TypeScript Definitions
 * Auto-generated documentation
 **/

// Constants

/**
 * VERSION constant 
 * ""1.0.0""
 */
declare const VERSION: string;


/**
 * LoggerModule module
 */
declare const logger: {
    /**
     * Log an informational message
     * @param message string
     * @param args any[]
     */
    Info(message: string, args: any[]): void;
};

/**
 * EventsModule module
 */
declare const events: {
    /**
     * Register a callback to be called when the script abyssirc is started
     * @param action () => void
     */
    OnStarted(action: () => void): void;
    /**
     * Hook into an event
     * @param eventName string
     * @param eventHandler (arg: any) => void
     */
    HookEvent(eventName: string, eventHandler: (arg: any) => void): void;
};

/**
 * SchedulerModule module
 */
declare const scheduler: {
    /**
     * Schedule a task to be run every x seconds
     * @param name string
     * @param seconds number
     * @param callback () => void
     */
    ScheduleTask(name: string, seconds: number, callback: () => void): void;
};

/**
 * IrcManagerModule module
 */
declare const irc_manager: {
    /**
     * 
     * @param commandCode string
     * @param callback (arg1: string, arg2: any) => void
     */
    HookCommand(commandCode: string, callback: (arg1: string, arg2: any) => void): void;
};

