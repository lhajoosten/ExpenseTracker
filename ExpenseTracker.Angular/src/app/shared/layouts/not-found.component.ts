import { Component } from '@angular/core';
import { materialModules } from '../shared.config';
import { Router } from '@angular/router';
import { NavigationHistoryService } from '../../core/services/navigation-history.service';
import { Location } from '@angular/common';

@Component({
    selector: 'app-not-found',
    imports: [materialModules],
    standalone: true,
    template: `
        <div class="not-found-container">
            <mat-card class="content-box">
                <div class="error-code">404</div>
                <h1>Page Not Found</h1>
                <mat-divider class="custom-divider"></mat-divider>
                <p class="message">
                    Oops! The page you are looking for doesn't exist or has been moved.
                </p>
                <div class="illustration">
                    <mat-icon
                        aria-hidden="false"
                        aria-label="Search icon"
                        fontIcon="search"
                        class="large-icon"
                    ></mat-icon>
                </div>
                <p class="suggestion">Let's get you back on track!</p>
                <div class="actions">
                    <button mat-raised-button color="primary" (click)="goHome()">
                        Go to Dashboard
                    </button>
                    <button mat-stroked-button (click)="goBack()" class="back-button">
                        Go Back
                    </button>
                </div>
            </mat-card>
        </div>
    `,
    styles: [`
        .not-found-container {
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 80vh;
            padding: 20px;
        }

        .content-box {
            max-width: 600px;
            text-align: center;
            padding: 40px;
        }

        .error-code {
            font-size: 8rem;
            font-weight: bold;
            color: #3f51b5;
            line-height: 1;
            margin-bottom: 10px;
        }

        h1 {
            font-size: 2.5rem;
            color: rgba(0, 0, 0, 0.87);
            margin-bottom: 20px;
        }

        .custom-divider {
            width: 80px;
            margin: 20px auto;
        }

        .message {
            font-size: 1.2rem;
            color: rgba(0, 0, 0, 0.6);
            margin-bottom: 30px;
        }

        .illustration {
            margin: 30px 0;

            .large-icon {
                font-size: 4rem;
                height: 4rem;
                width: 4rem;
                color: #3f51b5;
            }
        }

        .suggestion {
            font-size: 1.1rem;
            color: rgba(0, 0, 0, 0.6);
            margin-bottom: 30px;
        }

        .actions {
            display: flex;
            justify-content: center;
            gap: 16px;
        }

        .back-button {
            margin-left: 8px;
        }
    `],
})
export class NotFoundComponent {
    constructor(
        private router: Router,
        private navigationService: NavigationHistoryService,
        private location: Location,
    ) { }

    goBack(): void {
        // Try to get previous URL from the service
        const previousUrl = this.navigationService.getPreviousUrl();

        if (previousUrl) {
            this.router.navigateByUrl(previousUrl);
        } else {
            // Fallback to browser history
            this.location.back();
        }
    }

    goHome(): void {
        this.router.navigate(['/']);
    }
}
