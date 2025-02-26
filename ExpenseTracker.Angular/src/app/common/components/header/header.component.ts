import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { AngularMaterialModule } from '../angular-material.module';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { NotificationService } from '../../services/notification.service';
import { ThemeService } from '../../services/theme.service';
import { User } from '../../models/user.model';
import { RouterModule } from '@angular/router';
import { QuickAddCategoryComponent } from '../quick-actions/quick-add-category/quick-add-category.component';
import { QuickAddExpenseComponent } from '../quick-actions/quick-add-expense/quick-add-expense.component';
import { QuickAddIncomeComponent } from '../quick-actions/quick-add-income/quick-add-income.component';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.scss'],
    imports: [AngularMaterialModule, CommonModule, RouterModule],
    standalone: true,
})
export class HeaderComponent implements OnInit {
    @Output() sidebarToggle = new EventEmitter<void>();
    @Input() sidenavCollapsed = false;
    isAuthenticated = false;
    //user: User | null = null;
    user = {
        displayName: 'John Doe',
        email: 'john@example.com',
        photoUrl: 'assets/images/default-avatar.png',
    } as User | null;
    notificationCount = 0;
    isDarkTheme = false; // Default to light theme
    isOnline = true; // User online status
    pageTitle = 'Dashboard'; // Default page title

    constructor(
        private authService: AuthService,
        private router: Router,
        private themeService: ThemeService,
        private notificationService: NotificationService,
        private dialog: MatDialog
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

        // Update page title based on route
        this.router.events
            .pipe(filter((event) => event instanceof NavigationEnd))
            .subscribe(() => {
                const urlParts = this.router.url
                    .split('/')
                    .filter((part) => part);
                if (urlParts.length > 0) {
                    const routeKey = urlParts[0];
                    this.setPageTitle(routeKey);
                } else {
                    this.pageTitle = 'Dashboard';
                }
            });
    }

    setPageTitle(routeKey: string): void {
        const titles: Record<string, string> = {
            dashboard: 'Dashboard',
            expenses: 'Expenses',
            reports: 'Reports',
            categories: 'Categories',
            budgets: 'Budgets',
            profile: 'Profile',
            settings: 'Settings',
            notifications: 'Notifications',
            auth: 'Authentication',
        };

        this.pageTitle = titles[routeKey] || 'Dashboard';
    }

    toggleSidebar(): void {
        this.sidebarToggle.emit();
    }

    logout(): void {
        this.authService.logout().subscribe(() => {
            this.router.navigate(['/auth/login']);
        });
    }

    toggleTheme(): void {
        this.themeService.toggleDarkTheme();
    }

    clearAllNotifications(): void {
        this.notificationService.clearNotifications();
    }

    quickAddExpense(): void {
        const dialogRef = this.dialog.open(QuickAddExpenseComponent, {
            width: '400px',
        });

        dialogRef.afterClosed().subscribe((result) => {
            if (result) {
                // Handle saving new expense
                console.log('Expense added:', result);
            }
        });
    }

    quickAddCategory(): void {
        const dialogRef = this.dialog.open(QuickAddCategoryComponent, {
            width: '400px',
        });

        dialogRef.afterClosed().subscribe((result) => {
            if (result) {
                // Handle saving new category
                console.log('Category added:', result);
            }
        });
    }

    quickAddIncome(): void {
        const dialogRef = this.dialog.open(QuickAddIncomeComponent, {
            width: '400px',
        });

        dialogRef.afterClosed().subscribe((result) => {
            if (result) {
                // Handle saving new income
                console.log('Income added:', result);
            }
        });
    }
}
