import { Component, OnInit } from '@angular/core';
import {
    FormBuilder,
    FormGroup,
    ReactiveFormsModule,
    Validators,
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { OAuthService } from '../../../core/services/oauth.service';
import { CommonModule } from '@angular/common';
import { materialModules } from '../../../shared/shared.config';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss'],
    imports: [CommonModule, materialModules, ReactiveFormsModule],
})
export class LoginComponent implements OnInit {
    loginForm: FormGroup;
    hidePassword = true;
    loading = false;
    submitted = false;
    errorMessage: string | null = null;

    constructor(
        private formBuilder: FormBuilder,
        private router: Router,
        private route: ActivatedRoute,
        private authService: AuthService,
        private oauthService: OAuthService,
        private snackBar: MatSnackBar,
    ) {
        this.loginForm = this.formBuilder.group({
            email: ['', [Validators.required, Validators.email]],
            password: ['', [Validators.required, Validators.minLength(6)]],
            rememberMe: [false],
        });
    }

    ngOnInit(): void {
        // Get return URL from route parameters or set default
        const returnUrl =
            this.route.snapshot.queryParamMap.get('returnUrl') || '/';
        this.authService.redirectUrl = returnUrl;

        // Check if there's an error message
        const error = this.route.snapshot.queryParamMap.get('error');
        if (error) {
            this.errorMessage = decodeURIComponent(error);
            this.snackBar.open(this.errorMessage, 'Close', {
                duration: 5000,
                panelClass: ['error-snackbar'],
            });
        }
    }

    onSubmit(): void {
        if (this.loginForm.invalid) {
            return;
        }

        this.loading = true;
        this.errorMessage = null;

        this.authService
            .login({
                email: this.loginForm.value.email,
                password: this.loginForm.value.password,
                rememberMe: this.loginForm.value.rememberMe,
            })
            .subscribe({
                next: () => {
                    const returnUrl = this.authService.redirectUrl || '/';
                    this.router.navigateByUrl(returnUrl);
                    this.snackBar.open('Login successful', 'Close', {
                        duration: 3000,
                        panelClass: ['success-snackbar'],
                    });
                },
                error: (error) => {
                    this.errorMessage = error.message || 'Invalid credentials';
                    this.snackBar.open(
                        this.errorMessage != null ? this.errorMessage : '',
                        'Close',
                        {
                            duration: 5000,
                            panelClass: ['error-snackbar'],
                        },
                    );
                    this.loading = false;
                },
            });
    }

    loginWithGithub(): void {
        this.oauthService.loginWithGithub();
    }

    loginWithMicrosoft(): void {
        this.oauthService.loginWithMicrosoft();
    }

    navigateToRegister(): void {
        this.router.navigate(['/auth/register']);
    }
}
