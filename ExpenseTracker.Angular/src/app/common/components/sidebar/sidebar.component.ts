import { Component, OnInit, Input } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';
import { NotificationService } from '../../services/notification.service';
import { Router } from '@angular/router';
import { User } from '../../models/user.model';
import { AngularMaterialModule } from '../angular-material.module';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'app-sidebar',
    templateUrl: './sidebar.component.html',
    styleUrls: ['./sidebar.component.scss'],
    standalone: true,
    imports: [AngularMaterialModule, CommonModule, RouterModule],
})
export class SidebarComponent implements OnInit {
    @Input() collapsed = false;
    isAuthenticated = false;
    user: User | null = null;
    notificationCount = 0;
    isDarkTheme = false; // Default to light theme

    constructor(
        private authService: AuthService,
        private themeService: ThemeService,
        private notificationService: NotificationService,
        private router: Router
    ) {}

    ngOnInit(): void {
        // Subscribe to authentication state
        this.authService.currentUser().subscribe((user) => {
            this.isAuthenticated = !!user;
            this.user = user;
        });

        // Subscribe to theme changes
        this.themeService.isDarkTheme.subscribe((isDark) => {
            this.isDarkTheme = isDark;
        });

        // Subscribe to notifications
        this.notificationService.getNotificationCount().subscribe((count) => {
            this.notificationCount = count;
        });
    }

    logout(): void {
        this.authService.logout().subscribe(() => {
            this.router.navigate(['/auth/login']);
        });
    }

    toggleTheme(): void {
        this.themeService.toggleDarkTheme();
    }
}
