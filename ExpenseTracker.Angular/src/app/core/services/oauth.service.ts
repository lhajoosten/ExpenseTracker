import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { catchError, Observable, of, tap } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root',
})
export class OAuthService {
    private readonly apiUrl = environment.apiUrl;

    constructor(
        private http: HttpClient,
        private router: Router,
        private authService: AuthService,
    ) { }

    // Initiate GitHub OAuth login
    loginWithGithub(useStateTracking = true): void {
        const returnUrl = this.storeCurrentRoute();
        window.location.href = `${this.apiUrl
            }/oauth/login/GitHub?returnUrl=${encodeURIComponent(returnUrl)}${useStateTracking ? '&trackState=true' : ''}`;
    }

    // Initiate Microsoft Entra ID login
    loginWithMicrosoft(useStateTracking = true): void {
        const returnUrl = this.storeCurrentRoute();
        window.location.href = `${this.apiUrl
            }/oauth/login/Microsoft?returnUrl=${encodeURIComponent(returnUrl)}${useStateTracking ? '&trackState=true' : ''}`;
    }

    // This is useful after a state error to ensure we have a user
    ensureUser(): Observable<any> {
        return this.http.get(`${this.apiUrl}/oauth/ensure-user`).pipe(
            tap((response: any) => {
                if (response.success && response.user) {
                    // Update the auth service with the user data
                    this.authService['currentUserSubject'].next(response.user);
                }
            }),
            catchError(error => {
                console.error('Failed to ensure user:', error);
                // Return a more useful value if the request fails
                return of({ success: false, error: error.message });
            })
        );
    }

    // Get current user from OAuth
    getCurrentUser(): Observable<unknown> {
        return this.http.get(`${this.apiUrl}/oauth/status`);
    }

    // Logout (uses OAuth controller's logout endpoint)
    logout(): Observable<unknown> {
        return this.http.post(`${this.apiUrl}/oauth/logout`, {});
    }

    // Check if the URL indicates a successful authentication despite state errors
    hasSuccessfulAuthInUrl(): boolean {
        const url = new URL(window.location.href);
        return url.searchParams.get('auth') === 'success' ||
            url.searchParams.get('note') === 'authenticated_state_error' ||
            url.searchParams.get('note') === 'already_authenticated';
    }

    // Store the current route for post-login redirect
    private storeCurrentRoute(): string {
        // We'll return the frontend callback URL, which will handle redirection to the original page
        const callbackUrl = `${window.location.origin}/auth/callback`;

        // Store the actual destination for after callback processing
        const originalUrl = this.authService.redirectUrl || this.router.url;
        if (
            originalUrl &&
            originalUrl !== '/auth/signin' &&
            originalUrl !== '/auth/callback'
        ) {
            localStorage.setItem('oauth_return_url', originalUrl);
        }

        return callbackUrl;
    }

    // Get the stored return URL
    getStoredReturnUrl(): string {
        const url = localStorage.getItem('oauth_return_url') || '/';
        localStorage.removeItem('oauth_return_url'); // Clear after use
        return url;
    }
}
