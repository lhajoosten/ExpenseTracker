import { Component } from '@angular/core';
import {
    FormBuilder,
    FormGroup,
    Validators,
    AbstractControl,
    ValidationErrors,
    ReactiveFormsModule,
} from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';
import { OAuthService } from '../../../core/services/oauth.service';
import { CommonModule } from '@angular/common';
import { materialModules } from '../../../shared/shared.config';

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss'],
    imports: [CommonModule, materialModules, ReactiveFormsModule],
    standalone: true,
})
export class RegisterComponent {
    registerForm: FormGroup;
    loading = false;
    hidePassword = true;
    hideConfirmPassword = true;
    errorMessage: string | null = null;

    constructor(
        private formBuilder: FormBuilder,
        private router: Router,
        private authService: AuthService,
        private oauthService: OAuthService,
        private snackBar: MatSnackBar,
    ) {
        this.registerForm = this.formBuilder.group(
            {
                firstName: ['', [Validators.required]],
                lastName: ['', [Validators.required]],
                email: ['', [Validators.required, Validators.email]],
                password: [
                    '',
                    [
                        Validators.required,
                        Validators.minLength(8),
                        this.createPasswordStrengthValidator(),
                    ],
                ],
                confirmPassword: ['', Validators.required],
                acceptTerms: [false, Validators.requiredTrue],
            },
            {
                validators: this.passwordMatchValidator,
            },
        );
    }

    // Custom validator to ensure passwords match
    passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
        const password = control.get('password');
        const confirmPassword = control.get('confirmPassword');

        if (
            password &&
            confirmPassword &&
            password.value !== confirmPassword.value
        ) {
            confirmPassword.setErrors({ passwordMismatch: true });
            return { passwordMismatch: true };
        }

        return null;
    }

    // Custom validator for password strength
    createPasswordStrengthValidator(): (
        control: AbstractControl,
    ) => ValidationErrors | null {
        return (control: AbstractControl): ValidationErrors | null => {
            const value = control.value;

            if (!value) {
                return null;
            }

            const hasUpperCase = /[A-Z]+/.test(value);
            const hasLowerCase = /[a-z]+/.test(value);
            const hasNumeric = /[0-9]+/.test(value);
            const hasSpecialChar =
                /[!@#$%^&*()_+\-=\\[\]{};':"\\|,.<>\\/?]+/.test(value);

            const passwordValid =
                hasUpperCase && hasLowerCase && hasNumeric && hasSpecialChar;

            return !passwordValid ? { passwordStrength: true } : null;
        };
    }

    getPasswordErrorMessage(): string {
        const passwordControl = this.registerForm.get('password');

        if (passwordControl?.hasError('required')) {
            return 'Password is required';
        }

        if (passwordControl?.hasError('minlength')) {
            return 'Password must be at least 8 characters';
        }

        if (passwordControl?.hasError('passwordStrength')) {
            return 'Password must include uppercase, lowercase, number and special character';
        }

        return '';
    }

    onSubmit(): void {
        if (this.registerForm.invalid) {
            return;
        }

        this.loading = true;
        this.errorMessage = null;

        const registrationData = {
            firstname: this.registerForm.value.firstName,
            lastname: this.registerForm.value.lastName,
            email: this.registerForm.value.email,
            password: this.registerForm.value.password,
            confirmPassword: this.registerForm.value.confirmPassword,
        };

        this.authService.register(registrationData).subscribe({
            next: () => {
                this.snackBar.open(
                    'Registration successful! Please check your email to verify your account.',
                    'Close',
                    {
                        duration: 5000,
                        panelClass: ['success-snackbar'],
                    },
                );
                this.router.navigate(['/auth/signin']);
            },
            error: (error) => {
                this.errorMessage =
                    error.message || 'Registration failed. Please try again.';
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

    navigateToLogin(): void {
        this.router.navigate(['/auth/signin']);
    }
}
