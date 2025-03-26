/**
 * HamLink JavaScript API TypeScript Definitions
 * Auto-generated documentation
 */

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
     * Register a callback to be called when the script hamlink is started
     * @param action any
     */
    OnStarted(action: any): void;
};

