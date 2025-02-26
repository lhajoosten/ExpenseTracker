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
    selector: 'app-quick-add-expense',
    templateUrl: './quick-add-expense.component.html',
    styleUrls: ['./quick-add-expense.component.scss'],
    standalone: true,
    imports: [AngularMaterialModule, ReactiveFormsModule, CommonModule],
})
export class QuickAddExpenseComponent {
    expenseForm: FormGroup;
    categories = [
        { id: 1, name: 'Food & Drinks' },
        { id: 2, name: 'Shopping' },
        { id: 3, name: 'Transportation' },
        { id: 4, name: 'Entertainment' },
        { id: 5, name: 'Bills & Utilities' },
        { id: 6, name: 'Other' },
    ];

    constructor(
        private fb: FormBuilder,
        public dialogRef: MatDialogRef<QuickAddExpenseComponent>,
        @Inject(MAT_DIALOG_DATA) public data: unknown
    ) {
        this.expenseForm = this.fb.group({
            amount: ['', [Validators.required, Validators.min(0.01)]],
            description: ['', Validators.required],
            categoryId: ['', Validators.required],
            date: [new Date(), Validators.required],
        });
    }

    onSubmit(): void {
        if (this.expenseForm.valid) {
            this.dialogRef.close(this.expenseForm.value);
        }
    }

    onCancel(): void {
        this.dialogRef.close();
    }
}
