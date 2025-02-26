import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { AdminComponent } from './features/admin/admin.component';

export const routes: Routes = [
    // Public routes
    { path: 'auth', loadChildren: () => import('./features/auth/auth.routes').then(m => m.routes) },

    // Protected routes
    {
        path: 'dashboard',
        component: DashboardComponent,
        canActivate: [AuthGuard],
    },

    // Role-protected routes
    {
        path: 'admin',
        component: AdminComponent,
        canActivate: [AuthGuard, RoleGuard],
        data: { role: 'admin' },
    },

    // Redirect to dashboard
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
];
