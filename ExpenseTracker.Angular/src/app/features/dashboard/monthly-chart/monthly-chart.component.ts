import { Component, OnInit, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { Chart, registerables } from 'chart.js';

// Register Chart.js components
Chart.register(...registerables);

interface MonthlyData {
    expenses: number[];
    income: number[];
    savings: number[];
    labels: string[];
}

@Component({
    selector: 'app-monthly-chart',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonToggleModule,
        MatIconModule,
        MatButtonModule,
        MatFormFieldModule,
        MatSelectModule,
        FormsModule
    ],
    templateUrl: './monthly-chart.component.html',
    styleUrl: './monthly-chart.component.scss'
})
export class MonthlyChartComponent implements OnInit, AfterViewInit {
    @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;

    chart: Chart | null = null;
    selectedView: string = 'monthly';
    selectedYear: number = new Date().getFullYear();
    selectedPeriod: string = 'sixMonths';

    yearOptions: number[] = [];
    chartData: MonthlyData = {
        expenses: [],
        income: [],
        savings: [],
        labels: []
    };

    constructor() { }

    ngOnInit(): void {
        this.initYearOptions();
        this.loadChartData();
    }

    ngAfterViewInit(): void {
        this.initChart();
    }

    initYearOptions(): void {
        const currentYear = new Date().getFullYear();
        for (let i = currentYear - 2; i <= currentYear; i++) {
            this.yearOptions.push(i);
        }
    }

    loadChartData(): void {
        // This would normally come from your finance service
        // Mock data for demonstration
        const mockMonthlyData: MonthlyData = {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            expenses: [1850, 1750, 1950, 1800, 1650, 1700, 1900, 1850, 1950, 1800, 1700, 1650],
            income: [3200, 3200, 3400, 3200, 3200, 3500, 3200, 3200, 3400, 3200, 3200, 3500],
            savings: [1350, 1450, 1450, 1400, 1550, 1800, 1300, 1350, 1450, 1400, 1500, 1850]
        };

        // Filter based on selected period
        const today = new Date();
        const currentMonth = today.getMonth();

        if (this.selectedPeriod === 'threeMonths') {
            this.chartData.labels = this.getLast3MonthsLabels(currentMonth);
            this.chartData.expenses = this.getLast3MonthsData(mockMonthlyData.expenses, currentMonth);
            this.chartData.income = this.getLast3MonthsData(mockMonthlyData.income, currentMonth);
            this.chartData.savings = this.getLast3MonthsData(mockMonthlyData.savings, currentMonth);
        } else if (this.selectedPeriod === 'sixMonths') {
            this.chartData.labels = this.getLast6MonthsLabels(currentMonth);
            this.chartData.expenses = this.getLast6MonthsData(mockMonthlyData.expenses, currentMonth);
            this.chartData.income = this.getLast6MonthsData(mockMonthlyData.income, currentMonth);
            this.chartData.savings = this.getLast6MonthsData(mockMonthlyData.savings, currentMonth);
        } else {
            this.chartData = { ...mockMonthlyData };
        }
    }

    getLast3MonthsLabels(currentMonth: number): string[] {
        const labels = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        const result = [];

        for (let i = 2; i >= 0; i--) {
            const monthIndex = (currentMonth - i + 12) % 12;
            result.push(labels[monthIndex]);
        }

        return result;
    }

    getLast3MonthsData(data: number[], currentMonth: number): number[] {
        const result = [];

        for (let i = 2; i >= 0; i--) {
            const monthIndex = (currentMonth - i + 12) % 12;
            result.push(data[monthIndex]);
        }

        return result;
    }

    getLast6MonthsLabels(currentMonth: number): string[] {
        const labels = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        const result = [];

        for (let i = 5; i >= 0; i--) {
            const monthIndex = (currentMonth - i + 12) % 12;
            result.push(labels[monthIndex]);
        }

        return result;
    }

    getLast6MonthsData(data: number[], currentMonth: number): number[] {
        const result = [];

        for (let i = 5; i >= 0; i--) {
            const monthIndex = (currentMonth - i + 12) % 12;
            result.push(data[monthIndex]);
        }

        return result;
    }

    initChart(): void {
        if (!this.chartCanvas) return;

        const ctx = this.chartCanvas.nativeElement.getContext('2d');
        if (!ctx) return;

        this.chart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: this.chartData.labels,
                datasets: [
                    {
                        label: 'Income',
                        data: this.chartData.income,
                        backgroundColor: 'rgba(76, 175, 80, 0.6)',
                        borderColor: 'rgba(76, 175, 80, 1)',
                        borderWidth: 1
                    },
                    {
                        label: 'Expenses',
                        data: this.chartData.expenses,
                        backgroundColor: 'rgba(244, 67, 54, 0.6)',
                        borderColor: 'rgba(244, 67, 54, 1)',
                        borderWidth: 1
                    },
                    {
                        label: 'Savings',
                        data: this.chartData.savings,
                        backgroundColor: 'rgba(33, 150, 243, 0.6)',
                        borderColor: 'rgba(33, 150, 243, 1)',
                        borderWidth: 1,
                        type: 'line',
                        fill: false,
                        tension: 0.1
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        grid: {
                            display: false
                        }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) {
                                return '$' + value.toLocaleString();
                            }
                        }
                    }
                },
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                let label = context.dataset.label || '';
                                if (label) {
                                    label += ': ';
                                }
                                label += '$' + context.parsed.y.toLocaleString();
                                return label;
                            }
                        }
                    },
                    legend: {
                        position: 'top',
                        align: 'end'
                    }
                }
            }
        });
    }

    updateChartView(): void {
        this.loadChartData();

        if (this.chart) {
            this.chart.data.labels = this.chartData.labels;
            this.chart.data.datasets[0].data = this.chartData.income;
            this.chart.data.datasets[1].data = this.chartData.expenses;
            this.chart.data.datasets[2].data = this.chartData.savings;
            this.chart.update();
        }
    }

    onPeriodChange(): void {
        this.updateChartView();
    }

    onYearChange(): void {
        this.updateChartView();
    }
}