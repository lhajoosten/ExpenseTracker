import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthRoutingModule } from './auth-routing.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ConfirmEmailComponent } from './confirm-email/confirm-email.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { RequestPasswordResetComponent } from './request-password-reset/request-password-reset.component';
import { ChangePasswordComponent } from './change-password/change-password.component';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    AuthRoutingModule,

    LoginComponent,
    RegisterComponent,
    ConfirmEmailComponent,
    RequestPasswordResetComponent,
    ChangePasswordComponent,
  ],
  exports: [
    LoginComponent,
    RegisterComponent,
    ConfirmEmailComponent,
    RequestPasswordResetComponent,
    ChangePasswordComponent,
  ],
})
export class AuthModule {}
