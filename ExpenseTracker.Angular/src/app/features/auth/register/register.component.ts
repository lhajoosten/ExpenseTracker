import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { materialModules } from '../../../shared/shared.config';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    materialModules
  ]
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  hidePassword = true;
  hideConfirmPassword = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', [Validators.required, Validators.minLength(3)]],
      lastName: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
    }
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null
      : { mismatch: true };
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      const { firstName, lastName, email, password } = this.registerForm.value;
      this.authService.register({ firstName, lastName, email, password }).subscribe({
        next: () => {
          const redirectUrl = this.authService.redirectUrl || '/';
          this.router.navigate([redirectUrl]);
        },
        error: (error) => {
          console.error('Registration failed:', error);
        }
      });
    }
  }

  loginWithGithub(): void {
    this.authService.loginWithGithub();
  }

  loginWithMicrosoft(): void {
    this.authService.loginWithMicrosoft();
  }

  navigateToLogin(): void {
    this.router.navigate(['/auth/login']);
  }
}
