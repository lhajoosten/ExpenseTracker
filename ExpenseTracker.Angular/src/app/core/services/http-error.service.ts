import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { LoggerService } from './logger.service';
import { throwError } from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class HttpErrorService {
    constructor(private logger: LoggerService) {}

    handleError(error: HttpErrorResponse, operation = 'operation') {
        let errorMessage = '';

        if (error.error instanceof ErrorEvent) {
            // Client-side error
            errorMessage = `Client Error: ${error.error.message}`;
        } else {
            // Server-side error
            errorMessage = `Server Error: ${error.status} - ${error.statusText || ''} - ${error.error?.message || ''}`;
        }

        this.logger.error(`${operation} failed: ${errorMessage}`);

        // Return an observable with a user-facing error message
        return throwError(
            () =>
                new Error(
                    `${operation} failed. Please try again or contact support if the problem persists.`,
                ),
        );
    }

    // Generic error handler for components to display user-friendly messages
    getUserFriendlyErrorMessage(error: any): string {
        if (error?.status === 401) {
            return 'You need to log in to perform this action';
        } else if (error?.status === 403) {
            return 'You do not have permission to perform this action';
        } else if (error?.status === 404) {
            return 'The requested resource was not found';
        } else if (error?.status >= 500) {
            return 'A server error occurred. Please try again later';
        }

        return error?.message || 'An unexpected error occurred';
    }
}
