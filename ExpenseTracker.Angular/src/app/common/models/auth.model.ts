import { User } from "./user.model";

export interface RegisterRequest {
    email: string;
    password: string;
    displayName?: string;
    firstName?: string;
    lastName?: string;
  }

  export interface LoginRequest {
    email: string;
    password: string;
    rememberMe?: boolean;
  }

  export interface LoginResponse {
    user: User;
    token?: string;
    expiresAt?: number;
  }

  export interface EmailConfirmationRequest {
    token: string;
    email: string;
  }

  export interface PasswordResetRequest {
    token: string;
    email: string;
    password: string;
    confirmPassword: string;
  }

  export interface ApiResponse {
    success: boolean;
    message?: string;
  }
