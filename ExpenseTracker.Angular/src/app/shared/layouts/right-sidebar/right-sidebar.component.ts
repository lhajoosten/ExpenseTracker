import { Component, OnInit } from '@angular/core';
import { materialModules } from '../../shared.config';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-right-sidebar',
  standalone: true,
  imports: [CommonModule, FormsModule, materialModules],
  templateUrl: './right-sidebar.component.html',
  styleUrls: ['./right-sidebar.component.scss']
})
export class RightSidebarComponent implements OnInit {
  // User information
  userName = 'Luc Joosten';
  userEmail = 'lhajoosten@expense-tracker.com';
  userAvatar = 'assets/images/default-avatar.png';

  // Activity filtering
  activityFilter: string = '';
  Math = Math; // To use Math in template

  // Budget alerts
  budgetAlerts = [
    {
      category: 'Dining Out',
      message: 'You\'ve spent 85% of your monthly budget',
      percentage: 85,
      spent: 425,
      limit: 500,
      color: 'warn'
    },
    {
      category: 'Entertainment',
      message: 'You\'ve spent 70% of your monthly budget',
      percentage: 70,
      spent: 140,
      limit: 200,
      color: 'primary'
    }
  ];

  // Activity items
  activityItems = [
    {
      id: 1,
      type: 'expense',
      title: 'Grocery Shopping',
      description: 'Weekly grocery shopping at Whole Foods',
      amount: -78.35,
      time: '2 hours ago',
      category: 'Groceries'
    },
    {
      id: 2,
      type: 'income',
      title: 'Salary Deposit',
      description: 'Monthly salary from Acme Inc.',
      amount: 3500.00,
      time: '1 day ago',
      category: 'Income'
    },
    {
      id: 3,
      type: 'expense',
      title: 'Coffee Shop',
      description: 'Coffee and pastry at Starbucks',
      amount: -12.50,
      time: '2 days ago',
      category: 'Dining Out'
    },
    {
      id: 4,
      type: 'transfer',
      title: 'To Savings',
      description: 'Monthly transfer to savings account',
      amount: -500.00,
      time: '3 days ago',
      category: 'Transfer'
    },
    {
      id: 5,
      type: 'system',
      title: 'Budget Created',
      description: 'Created new budget for Dining Out',
      amount: 0,
      time: '4 days ago',
      category: 'System'
    }
  ];

  // Filtered activity based on search
  filteredActivityItems = this.activityItems;

  ngOnInit() {
    // Initial filtering setup
    this.updateFilteredItems();
  }

  // Activity filtering
  updateFilteredItems() {
    if (!this.activityFilter) {
      this.filteredActivityItems = this.activityItems;
      return;
    }

    const filter = this.activityFilter.toLowerCase();
    this.filteredActivityItems = this.activityItems.filter(item =>
      item.title.toLowerCase().includes(filter) ||
      item.description.toLowerCase().includes(filter) ||
      item.category.toLowerCase().includes(filter)
    );
  }

  // Get appropriate icon based on activity type
  getIconForActivityType(type: string): string {
    switch (type) {
      case 'expense': return 'arrow_downward';
      case 'income': return 'arrow_upward';
      case 'transfer': return 'swap_horiz';
      case 'system': return 'settings';
      default: return 'history';
    }
  }

  // Action methods
  viewProfile() {
    console.log('View profile clicked');
  }

  openAccountSettings() {
    console.log('Account settings clicked');
  }

  logout() {
    console.log('Logout clicked');
  }

  addExpense() {
    console.log('Add expense clicked');
  }

  addIncome() {
    console.log('Add income clicked');
  }

  transfer() {
    console.log('Transfer clicked');
  }

  manageBudget() {
    console.log('Budget management clicked');
  }

  viewAllActivity() {
    console.log('View all activity clicked');
  }
}