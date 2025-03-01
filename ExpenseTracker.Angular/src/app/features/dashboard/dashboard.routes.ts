import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard.component';
import { AuthGuard } from '../../core/guards/auth.guard';

export const routes: Routes = [
    {
        path: '',
        component: DashboardComponent,
        canActivate: [AuthGuard],
        children: [
            // Add child routes when you develop more features
        ]
    }
];