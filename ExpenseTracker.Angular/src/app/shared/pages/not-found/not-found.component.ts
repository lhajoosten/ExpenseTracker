import { Component } from '@angular/core';
import { materialModules } from '../../shared.config';
import { Router } from '@angular/router';
import { NavigationHistoryService } from '../../../core/services/navigation-history.service';
import { Location } from '@angular/common';

@Component({
    selector: 'app-not-found',
    imports: [materialModules],
    templateUrl: './not-found.component.html',
    styleUrl: './not-found.component.scss',
    standalone: true,
})
export class NotFoundComponent {
    constructor(
        private router: Router,
        private navigationService: NavigationHistoryService,
        private location: Location,
    ) {}

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
