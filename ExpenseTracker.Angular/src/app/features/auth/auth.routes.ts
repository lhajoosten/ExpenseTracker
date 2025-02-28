import { Routes } from '@angular/router';
import { ConfirmEmailComponent } from './confirm-email/confirm-email.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { OAuthCallbackComponent } from '../../shared/components/oauth-callback.component';

export const routes: Routes = [
    // Public routes
    { path: 'signin', component: LoginComponent },
    { path: 'signup', component: RegisterComponent },
    { path: 'confirm-email', component: ConfirmEmailComponent },
    { path: 'callback', component: OAuthCallbackComponent },
    { path: '', redirectTo: 'signin', pathMatch: 'full' },
];
