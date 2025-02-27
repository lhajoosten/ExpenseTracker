import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { materialModules } from '../../../shared/shared.config';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    materialModules
  ]
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  hidePassword = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    protected router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  ngOnInit(): void {
    // Check if already logged in
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
    }
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      const { email, password } = this.loginForm.value;
      this.authService.login({ email, password }).subscribe({
        next: () => {
          // Redirect to the intended URL or home
          const redirectUrl = this.authService.redirectUrl || '/';
          this.router.navigate([redirectUrl]);
        },
        error: (error) => {
          // Handle login error (you might want to show a snackbar or error message)
          console.error('Login failed:', error);
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

  navigateToRegister(): void {
    this.router.navigate(['/auth/register']);
  }
}
