import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

interface CategorySummary {
    name: string;
    amount: number;
    percentage: number;
    icon: string;
    color: string;
}

@Component({
    selector: 'app-expenses-summary',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatIconModule,
        MatDividerModule,
        MatButtonModule,
        MatTooltipModule
    ],
    templateUrl: './expenses-summary.component.html',
    styleUrl: './expenses-summary.component.scss'
})
export class ExpensesSummaryComponent implements OnInit {
    totalExpenses: number = 0;
    categoryData: CategorySummary[] = [];
    currentMonth: string = '';

    constructor() { }

    ngOnInit(): void {
        this.setCurrentMonth();
        this.loadCategorySummary();
    }

    setCurrentMonth(): void {
        const monthNames = [
            'January', 'February', 'March', 'April', 'May', 'June',
            'July', 'August', 'September', 'October', 'November', 'December'
        ];
        const currentDate = new Date();
        this.currentMonth = monthNames[currentDate.getMonth()];
    }

    loadCategorySummary(): void {
        // This would normally come from your expense service
        const mockCategoryData = [
            { name: 'Food & Dining', amount: 425.75, icon: 'restaurant', color: '#4CAF50' },
            { name: 'Transportation', amount: 210.30, icon: 'directions_car', color: '#2196F3' },
            { name: 'Housing', amount: 800.00, icon: 'home', color: '#9C27B0' },
            { name: 'Entertainment', amount: 150.50, icon: 'movie', color: '#FF9800' },
            { name: 'Utilities', amount: 195.25, icon: 'power', color: '#F44336' }
        ];

        this.totalExpenses = mockCategoryData.reduce((total, category) => total + category.amount, 0);

        // Calculate percentage for each category
        this.categoryData = mockCategoryData.map(category => ({
            ...category,
            percentage: (category.amount / this.totalExpenses) * 100
        }));
    }

    getProgressWidth(percentage: number): string {
        return `${percentage}%`;
    }

    formatAmount(amount: number): string {
        return `$${amount.toFixed(2)}`;
    }

    viewAllCategories(): void {
        console.log('View all categories clicked');
        // Will navigate to detailed categories view
    }
}