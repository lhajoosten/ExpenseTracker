import { Component, OnInit } from '@angular/core';
import { Router, RouterModule, RouterOutlet } from '@angular/router';
import { HeaderComponent } from './shared/components/header/header.component';
import { materialModules } from './shared/shared.config';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/services/auth.service';

@Component({
    selector: 'app-root',
    imports: [CommonModule, RouterModule, HeaderComponent, materialModules],
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    standalone: true
})
export class AppComponent implements OnInit {
    title = 'Dashboard App';
    isDarkTheme = false;

    // Mock history items for the sidebar
    historyItems: any[] = Array(6).fill(0).map((_, i) => ({
        id: i + 1,
        text: `Action ${i + 1} performed`,
        time: `${i + 1}h ago`
    }));

    constructor(private router: Router, private authService: AuthService) { }

    ngOnInit(): void {
        // Initialization logic if needed
    }

    toggleTheme() {
        this.isDarkTheme = !this.isDarkTheme;
        document.body.classList.toggle('dark-theme');
        document.body.classList.toggle('light-theme');
    }

    // Navigation and action methods

    openCalendar(): void {
        console.log('Calendar clicked');
        // Implement calendar view functionality
        // You could navigate to a calendar page
        // this.router.navigate(['/calendar']);
    }

    openNotifications(): void {
        console.log('Notifications clicked');
        // Implement notifications view functionality
        // Could be a dropdown or a page navigation
    }

    viewProfile(): void {
        console.log('View profile clicked');
        this.router.navigate(['/profile']);
    }

    openSettings(): void {
        console.log('Settings clicked');
        this.router.navigate(['/settings']);
    }

    logout(): void {
        this.authService.logout().subscribe();
    }
}
