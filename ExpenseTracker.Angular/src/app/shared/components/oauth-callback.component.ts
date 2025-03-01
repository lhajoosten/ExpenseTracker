import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { OAuthService } from '../../core/services/oauth.service';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-auth-callback',
    imports: [CommonModule],
    template: `<div class="auth-callback-container">
        <div *ngIf="loading" class="loading">
            <p>Authenticating...</p>
            <!-- Add spinner here if desired -->
        </div>
        <div *ngIf="error" class="error">
            <p>{{errorMessage}}</p>
        </div>
    </div>`,
    styles: [`
        .auth-callback-container {
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
        }
    `]
})
export class AuthCallbackComponent implements OnInit {
    loading = true;
    error = false;
    errorMessage = '';

    constructor(
        private router: Router,
        private oauthService: OAuthService,
        private authService: AuthService
    ) { }

    ngOnInit(): void {
        // Check for success indicators first
        if (this.oauthService.hasSuccessfulAuthInUrl()) {
            console.log('Detected successful authentication in URL');
            this.processAuthenticationSuccess();
            return;
        }

        // Check URL for error parameters
        const url = new URL(window.location.href);
        const error = url.searchParams.get('error');
        const errorMessage = url.searchParams.get('message') || url.searchParams.get('details');

        if (error && error !== 'state_error') {
            // Only treat non-state errors as true errors
            this.handleError(error, errorMessage);
            return;
        }

        // Process the authentication
        this.processAuthentication();
    }

    // When we know the auth was successful from URL params
    processAuthenticationSuccess(): void {
        this.authService.refreshAuthStatus().pipe(
            finalize(() => this.loading = false)
        ).subscribe({
            next: (success) => {
                if (success) {
                    // Redirect to the stored return URL or default route
                    const returnUrl = this.oauthService.getStoredReturnUrl();
                    this.router.navigateByUrl(returnUrl);
                } else {
                    // If status check fails but URL indicates success, try ensure-user
                    this.useEnsureUserFallback();
                }
            },
            error: () => {
                // If status check fails but URL indicates success, try ensure-user
                this.useEnsureUserFallback();
            }
        });
    }

    // Normal auth processing
    processAuthentication(): void {
        // First check auth status with normal endpoint
        this.authService.checkAuthStatus().pipe(
            catchError(() => {
                // If normal auth check fails, try ensure-user endpoint
                return this.oauthService.ensureUser();
            }),
            finalize(() => this.loading = false)
        ).subscribe({
            next: (success) => {
                if (success) {
                    // Redirect to the stored return URL or default route
                    const returnUrl = this.oauthService.getStoredReturnUrl();
                    this.router.navigateByUrl(returnUrl);
                } else {
                    this.handleError('Authentication failed', 'Could not retrieve user information.');
                }
            },
            error: (err) => {
                this.handleError('Authentication error', err.message || 'Unknown error occurred');
            }
        });
    }

    // Fallback to ensure-user endpoint
    useEnsureUserFallback(): void {
        this.oauthService.ensureUser().subscribe({
            next: (response) => {
                if (response.success) {
                    const returnUrl = this.oauthService.getStoredReturnUrl();
                    this.router.navigateByUrl(returnUrl);
                } else {
                    this.handleError('Authentication check failed', response.error || 'Unknown error');
                }
            },
            error: (err) => {
                this.handleError('Authentication fallback error', err.message || 'Unknown error occurred');
            }
        });
    }

    handleError(error: string, details?: string | null): void {
        this.error = true;
        this.loading = false;
        this.errorMessage = `${error}${details ? ': ' + details : ''}`;
        console.error('OAuth authentication error:', error, details);

        // After a delay, redirect to login
        setTimeout(() => {
            this.router.navigate(['/auth/signin']);
        }, 3000);
    }
}
