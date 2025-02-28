import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { LoggerService } from '../../core/services/logger.service';
import { OAuthService } from '../../core/services/oauth.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
    selector: 'app-oauth-callback',
    template: `<div class="oauth-callback">
        <div class="text-center p-5">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-3">Completing authentication, please wait...</p>
        </div>
    </div>`,
})
export class OAuthCallbackComponent implements OnInit {
    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private oauthService: OAuthService,
        private authService: AuthService,
        private logger: LoggerService,
    ) {}

    ngOnInit(): void {
        // Check if there was an error
        const error = this.route.snapshot.queryParamMap.get('error');
        const authSuccess = this.route.snapshot.queryParamMap.get('auth') === 'success';

        if (error) {
            this.logger.error('OAuth authentication error', error);
            this.router.navigate(['/auth/signin'], {
                queryParams: { error },
            });
            return;
        }

        // Check for direct success parameter (new)
        if (authSuccess) {
            this.logger.info('Authentication successful via direct parameter');
            const returnUrl = this.oauthService.getStoredReturnUrl();
            this.router.navigateByUrl(returnUrl);
            return;
        }

        // If no direct indicators, check auth status and continue
        this.authService.checkAuthStatus().subscribe({
            next: (isAuthenticated) => {
                if (isAuthenticated) {
                    const returnUrl = this.oauthService.getStoredReturnUrl();
                    this.router.navigateByUrl(returnUrl);
                } else {
                    this.logger.warn('No authentication data available after OAuth callback');
                    this.router.navigate(['/auth/signin']);
                }
            },
            error: (err) => {
                this.logger.error('Error checking auth status in callback', err);
                this.router.navigate(['/auth/signin']);
            },
        });
    }
}
