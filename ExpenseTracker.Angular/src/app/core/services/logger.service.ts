import { Injectable } from '@angular/core';

export enum LogLevel {
    Off = 0,
    Error,
    Warning,
    Info,
    Debug,
}

@Injectable({
    providedIn: 'root',
})
export class LoggerService {
    private level: LogLevel = LogLevel.Info;

    constructor() {}

    debug(message: string, ...optionalParams: any[]): void {
        this.logWith(LogLevel.Debug, message, optionalParams);
    }

    info(message: string, ...optionalParams: any[]): void {
        this.logWith(LogLevel.Info, message, optionalParams);
    }

    warn(message: string, ...optionalParams: any[]): void {
        this.logWith(LogLevel.Warning, message, optionalParams);
    }

    error(message: string, ...optionalParams: any[]): void {
        this.logWith(LogLevel.Error, message, optionalParams);
    }

    private logWith(
        level: LogLevel,
        message: string,
        optionalParams: any[],
    ): void {
        if (level <= this.level) {
            switch (level) {
                case LogLevel.Debug:
                    console.debug(message, ...optionalParams);
                    break;
                case LogLevel.Info:
                    console.info(message, ...optionalParams);
                    break;
                case LogLevel.Warning:
                    console.warn(message, ...optionalParams);
                    break;
                case LogLevel.Error:
                    console.error(message, ...optionalParams);
                    break;
            }
        }
    }

    setLogLevel(level: LogLevel): void {
        this.level = level;
    }
}
