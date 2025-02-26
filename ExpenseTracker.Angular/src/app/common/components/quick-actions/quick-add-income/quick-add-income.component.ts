import { Component, Inject } from '@angular/core';
import {
    FormBuilder,
    FormGroup,
    Validators,
    ReactiveFormsModule,
} from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AngularMaterialModule } from '../../angular-material.module';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-quick-add-income',
    templateUrl: './quick-add-income.component.html',
    styleUrls: ['./quick-add-income.component.scss'],
    standalone: true,
    imports: [AngularMaterialModule, ReactiveFormsModule, CommonModule],
})
export class QuickAddIncomeComponent {
    incomeForm: FormGroup;
    categories = [
        { id: 101, name: 'Salary' },
        { id: 102, name: 'Freelance' },
        { id: 103, name: 'Investments' },
        { id: 104, name: 'Rental Income' },
        { id: 105, name: 'Gifts' },
        { id: 106, name: 'Refunds' },
        { id: 107, name: 'Other Income' },
    ];

    recurrenceOptions = [
        { value: 'one-time', label: 'One-time' },
        { value: 'daily', label: 'Daily' },
        { value: 'weekly', label: 'Weekly' },
        { value: 'bi-weekly', label: 'Bi-weekly' },
        { value: 'monthly', label: 'Monthly' },
        { value: 'quarterly', label: 'Quarterly' },
        { value: 'yearly', label: 'Yearly' },
    ];

    constructor(
        private fb: FormBuilder,
        public dialogRef: MatDialogRef<QuickAddIncomeComponent>,
        @Inject(MAT_DIALOG_DATA) public data: unknown
    ) {
        this.incomeForm = this.fb.group({
            amount: ['', [Validators.required, Validators.min(0.01)]],
            description: ['', Validators.required],
            categoryId: ['', Validators.required],
            date: [new Date(), Validators.required],
            recurrence: ['one-time'],
            notes: [''],
        });
    }

    onSubmit(): void {
        if (this.incomeForm.valid) {
            this.dialogRef.close(this.incomeForm.value);
        }
    }

    onCancel(): void {
        this.dialogRef.close();
    }
}
