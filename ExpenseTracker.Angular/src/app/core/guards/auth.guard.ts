import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const AuthGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    // First check the current auth status to ensure it's up to date
    return authService.checkAuthStatus().pipe(
        take(1),
        map((isAuthenticated) => {
            if (isAuthenticated) {
                return true;
            }

            // Store the attempted URL for redirecting after login
            authService.redirectUrl = state.url;

            // Navigate to the login page
            router.navigate(['/auth/signin'], {
                queryParams: { returnUrl: state.url },
            });

            return false;
        }),
    );
};
