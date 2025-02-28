import {
    CanActivateFn,
    ActivatedRouteSnapshot,
    RouterStateSnapshot,
    Router,
} from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { map, take } from 'rxjs/operators';

export const RoleGuard: CanActivateFn = (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot,
) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    // Get the required roles from route data
    const requiredRoles = route.data?.['roles'] as string[] | undefined;

    // If no roles are specified, assume no restriction
    if (!requiredRoles || requiredRoles.length === 0) {
        return true;
    }

    return authService.currentUser$.pipe(
        take(1),
        map((user) => {
            // If no user or no roles, deny access
            if (!user || !user.roles) {
                authService.redirectUrl = state.url;
                router.navigate(['/auth/signin']);
                return false;
            }

            // Check if the user has at least one of the required roles
            const hasRole = requiredRoles.some((role) =>
                user.roles?.includes(role),
            );

            if (!hasRole) {
                router.navigate(['/forbidden']);
            }

            return hasRole;
        }),
    );
};
