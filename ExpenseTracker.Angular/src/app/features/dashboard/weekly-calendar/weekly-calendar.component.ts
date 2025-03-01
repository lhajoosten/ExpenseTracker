import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';

interface CalendarDay {
    date: Date;
    isCurrentMonth: boolean;
    isToday: boolean;
    hasExpenses: boolean;
    totalAmount: number;
    transactions: number;
}

interface UpcomingTransaction {
    id: number;
    name: string;
    amount: number;
    dueDate: Date;
    isPaid: boolean;
    category: string;
    isRecurring: boolean;
}

@Component({
    selector: 'app-weekly-calendar',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatIconModule,
        MatButtonModule,
        MatBadgeModule,
        MatTooltipModule
    ],
    templateUrl: './weekly-calendar.component.html',
    styleUrl: './weekly-calendar.component.scss'
})
export class WeeklyCalendarComponent implements OnInit {
    weekDays: string[] = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    calendarDays: CalendarDay[] = [];
    currentWeekStart: Date = new Date();
    upcomingTransactions: UpcomingTransaction[] = [];

    constructor() { }

    ngOnInit(): void {
        this.setCurrentWeekStart();
        this.generateCalendarDays();
        this.loadUpcomingTransactions();
    }

    setCurrentWeekStart(): void {
        const today = new Date();
        const dayOfWeek = today.getDay(); // 0 = Sunday, 6 = Saturday

        // Set to the beginning of the week (Sunday)
        this.currentWeekStart = new Date(today);
        this.currentWeekStart.setDate(today.getDate() - dayOfWeek);
    }

    generateCalendarDays(): void {
        this.calendarDays = [];
        const today = new Date();

        for (let i = 0; i < 7; i++) {
            const date = new Date(this.currentWeekStart);
            date.setDate(this.currentWeekStart.getDate() + i);

            // Mock data for expenses
            const hasExpenses = Math.random() > 0.3; // 70% chance to have expenses
            const totalAmount = hasExpenses ? Math.floor(Math.random() * 150) + 10 : 0;
            const transactions = hasExpenses ? Math.floor(Math.random() * 3) + 1 : 0;

            this.calendarDays.push({
                date,
                isCurrentMonth: date.getMonth() === today.getMonth(),
                isToday: date.toDateString() === today.toDateString(),
                hasExpenses,
                totalAmount,
                transactions
            });
        }
    }

    loadUpcomingTransactions(): void {
        // This would normally come from your transactions service
        this.upcomingTransactions = [
            {
                id: 1,
                name: 'Rent Payment',
                amount: 950,
                dueDate: this.addDays(new Date(), 2),
                isPaid: false,
                category: 'Housing',
                isRecurring: true
            },
            {
                id: 2,
                name: 'Internet Bill',
                amount: 59.99,
                dueDate: this.addDays(new Date(), 4),
                isPaid: false,
                category: 'Utilities',
                isRecurring: true
            },
            {
                id: 3,
                name: 'Car Insurance',
                amount: 108.50,
                dueDate: this.addDays(new Date(), 7),
                isPaid: false,
                category: 'Insurance',
                isRecurring: true
            }
        ];
    }

    addDays(date: Date, days: number): Date {
        const result = new Date(date);
        result.setDate(date.getDate() + days);
        return result;
    }

    previousWeek(): void {
        this.currentWeekStart.setDate(this.currentWeekStart.getDate() - 7);
        this.generateCalendarDays();
    }

    nextWeek(): void {
        this.currentWeekStart.setDate(this.currentWeekStart.getDate() + 7);
        this.generateCalendarDays();
    }

    getCurrentWeekLabel(): string {
        const weekEnd = new Date(this.currentWeekStart);
        weekEnd.setDate(this.currentWeekStart.getDate() + 6);

        const startMonth = this.currentWeekStart.toLocaleString('default', { month: 'short' });
        const endMonth = weekEnd.toLocaleString('default', { month: 'short' });

        const startDate = this.currentWeekStart.getDate();
        const endDate = weekEnd.getDate();

        if (startMonth === endMonth) {
            return `${startMonth} ${startDate} - ${endDate}`;
        }

        return `${startMonth} ${startDate} - ${endMonth} ${endDate}`;
    }

    formatDate(date: Date): string {
        return date.getDate().toString();
    }

    getDaysUntil(date: Date): string {
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        const dueDate = new Date(date);
        dueDate.setHours(0, 0, 0, 0);

        const diffTime = dueDate.getTime() - today.getTime();
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        if (diffDays === 0) return 'Today';
        if (diffDays === 1) return 'Tomorrow';
        return `in ${diffDays} days`;
    }

    markAsPaid(transaction: UpcomingTransaction): void {
        transaction.isPaid = true;
        // In a real app, you would call your transaction service here
    }
}