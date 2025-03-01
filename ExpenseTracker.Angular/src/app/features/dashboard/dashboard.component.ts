// dashboard-layout.component.ts
import { Component, OnInit } from '@angular/core';
import { materialModules } from '../../shared/shared.config';
import { CommonModule } from '@angular/common';
import { ExpensesSummaryComponent } from './expenses-summary/expenses-summary.component';
import { MonthlyChartComponent } from './monthly-chart/monthly-chart.component';
import { WeeklyCalendarComponent } from './weekly-calendar/weekly-calendar.component';

@Component({
    selector: 'app-dashboard-layout',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.scss'],
    imports: [CommonModule, materialModules, ExpensesSummaryComponent, MonthlyChartComponent, WeeklyCalendarComponent]
})
export class DashboardComponent implements OnInit {
    // Sample user data
    userName: string = 'John Doe';
    currentMonth: string = new Date().toLocaleString('default', { month: 'long' });
    currentYear: number = new Date().getFullYear();

    // Mock data for transactions
    recentTransactions: any[] = [
        {
            id: 1,
            name: 'Grocery Shopping',
            category: 'Food & Groceries',
            amount: -85.20,
            date: new Date(2025, 2, 28) // March 28, 2025
        },
        {
            id: 2,
            name: 'Salary Deposit',
            category: 'Income',
            amount: 3200.00,
            date: new Date(2025, 2, 25) // March 25, 2025
        },
        {
            id: 3,
            name: 'Electricity Bill',
            category: 'Utilities',
            amount: -142.50,
            date: new Date(2025, 2, 22) // March 22, 2025
        },
        {
            id: 4,
            name: 'Online Shopping',
            category: 'Shopping',
            amount: -65.99,
            date: new Date(2025, 2, 20) // March 20, 2025
        },
        {
            id: 5,
            name: 'Restaurant',
            category: 'Dining Out',
            amount: -38.75,
            date: new Date(2025, 2, 18) // March 18, 2025
        }
    ];

    // Mock data for the history log
    historyItems: any[] = Array(6).fill(0).map((_, i) => ({
        id: i + 1,
        text: `Action ${i + 1} performed`,
        time: `${i + 1}h ago`
    }));

    constructor() { }

    ngOnInit(): void {
        // Initialize component data, fetch from services, etc.
    }

    // Format currency amount with dollar sign and decimal points
    formatAmount(amount: number): string {
        const prefix = amount >= 0 ? '+$' : '-$';
        return `${prefix}${Math.abs(amount).toFixed(2)}`;
    }

    // Determine CSS class based on transaction amount
    getAmountClass(amount: number): string {
        return amount >= 0 ? 'income-amount' : 'expense-amount';
    }

    // Add new expense button click handler
    addNewExpense(): void {
        console.log('Add new expense clicked');
        // Implement functionality to open new expense dialog/form
    }

    // Toggle sidebar (for responsive mobile view)
    toggleSidebar(): void {
        // Implement sidebar toggle for mobile view
        console.log('Toggle sidebar clicked');
    }

    // Profile menu functions
    openProfileMenu(): void {
        console.log('Profile menu opened');
    }

    viewProfile(): void {
        console.log('View profile clicked');
    }

    logout(): void {
        console.log('Logout clicked');
        // Implement logout functionality
    }

    openCalendar(): void {
        console.log('Calendar clicked');
        // Implement calendar view functionality
    }

    openNotifications(): void {
        console.log('Notifications clicked');
        // Implement notifications view functionality
    }
}