import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AngularMaterialModule } from '../../angular-material.module';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-quick-add-category',
  templateUrl: './quick-add-category.component.html',
  styleUrls: ['./quick-add-category.component.scss'],
  standalone: true,
  imports: [AngularMaterialModule, ReactiveFormsModule, CommonModule]
})
export class QuickAddCategoryComponent {
  categoryForm: FormGroup;
  iconOptions = [
    'shopping_cart', 'fastfood', 'local_cafe', 'local_grocery_store', 'local_taxi',
    'local_gas_station', 'flight', 'theaters', 'restaurant', 'home', 'fitness_center',
    'account_balance', 'school', 'business_center', 'medical_services', 'pets',
    'family_restroom', 'card_giftcard', 'sports_esports', 'local_movies',
    'local_pharmacy', 'devices', 'emoji_events'
  ];

  colorOptions = [
    '#f44336', '#e91e63', '#9c27b0', '#673ab7', '#3f51b5', '#2196f3',
    '#03a9f4', '#00bcd4', '#009688', '#4caf50', '#8bc34a', '#cddc39',
    '#ffeb3b', '#ffc107', '#ff9800', '#ff5722', '#795548', '#607d8b'
  ];

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<QuickAddCategoryComponent>,
    @Inject(MAT_DIALOG_DATA) public data: unknown
  ) {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(50)]],
      icon: ['shopping_cart', Validators.required],
      color: ['#2196f3', Validators.required],
      isIncome: [false]
    });
  }

  onSubmit(): void {
    if (this.categoryForm.valid) {
      this.dialogRef.close(this.categoryForm.value);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
