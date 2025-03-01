import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-confirm-email',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule
    ],
    templateUrl: './confirm-email.component.html',
    styleUrl: './confirm-email.component.scss'
})
export class ConfirmEmailComponent implements OnInit {
    isLoading = true;
    isSuccess = false;
    errorMessage: string | null = null;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private authService: AuthService,
        private snackBar: MatSnackBar
    ) { }

    ngOnInit(): void {
        // Extract token and email from URL
        const token = this.route.snapshot.queryParamMap.get('token');
        const email = this.route.snapshot.queryParamMap.get('email');

        if (!token || !email) {
            this.isLoading = false;
            this.errorMessage = 'Invalid confirmation link. Missing token or email.';
            return;
        }

        this.confirmEmail(token, email);
    }

    confirmEmail(token: string, email: string): void {
        this.authService.confirmEmail(token, email).subscribe({
            next: () => {
                this.isLoading = false;
                this.isSuccess = true;
                this.snackBar.open('Email confirmed successfully!', 'Close', {
                    duration: 5000,
                    panelClass: ['success-snackbar'],
                });
            },
            error: (error) => {
                this.isLoading = false;
                this.errorMessage = error.message || 'Failed to confirm email. Please try again.';
                this.snackBar.open(this.errorMessage!, 'Close', {
                    duration: 5000,
                    panelClass: ['error-snackbar'],
                });
            }
        });
    }

    navigateToLogin(): void {
        this.router.navigate(['/auth/signin']);
    }

    resendConfirmation(): void {
        const email = this.route.snapshot.queryParamMap.get('email');
        if (!email) {
            this.snackBar.open('Email address not available. Please try signing up again.', 'Close', {
                duration: 5000,
                panelClass: ['error-snackbar'],
            });
            return;
        }

        this.isLoading = true;
        this.authService.resendConfirmationEmail(email).subscribe({
            next: () => {
                this.isLoading = false;
                this.snackBar.open('Confirmation email sent successfully!', 'Close', {
                    duration: 5000,
                    panelClass: ['success-snackbar'],
                });
            },
            error: (error) => {
                this.isLoading = false;
                this.errorMessage = error.message || 'Failed to resend confirmation email.';
                this.snackBar.open(this.errorMessage!, 'Close', {
                    duration: 5000,
                    panelClass: ['error-snackbar'],
                });
            }
        });
    }
}