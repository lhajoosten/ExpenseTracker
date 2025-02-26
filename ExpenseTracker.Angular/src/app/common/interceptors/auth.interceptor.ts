import { Injectable } from '@angular/core';
import {
    HttpEvent,
    HttpInterceptor,
    HttpHandler,
    HttpRequest,
    HttpErrorResponse,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private router: Router) {}

    intercept(
        req: HttpRequest<unknown>,
        next: HttpHandler
    ): Observable<HttpEvent<unknown>> {
        // Clone the request to ensure that withCredentials is always set.
        const clonedRequest = req.clone({ withCredentials: true });
        return next.handle(clonedRequest).pipe(
            catchError((error: HttpErrorResponse) => {
                if (error.status === 401) {
                    // For example, redirect to login if unauthorized.
                    this.router.navigate(['/login']);
                }
                return throwError(() => error);
            })
        );
    }
}
