import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { LoggerService } from '../services/logger.service';

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const logger = inject(LoggerService);

    // Add withCredentials flag to allow cookies to be sent in cross-origin requests
    const authReq = req.clone({
        withCredentials: true,
    });

    // Forward the modified request
    return next(authReq).pipe(
        catchError((error: HttpErrorResponse) => {
            if (error.status === 401) {
                logger.warn('Authorization error', error);

                // Check if we're already on the login page to prevent redirect loops
                if (!router.url.includes('/login')) {
                    // Store the current URL for redirecting after login
                    authService.redirectUrl = router.url;

                    // Redirect to login
                    router.navigate(['/auth/signin']);
                }
            } else if (error.status === 403) {
                logger.warn('Forbidden resource', error);
                router.navigate(['/forbidden']);
            }

            return throwError(() => error);
        }),
    );
};
