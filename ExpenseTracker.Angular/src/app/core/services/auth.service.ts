import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, map, tap } from 'rxjs/operators';
import { LoggerService } from './logger.service';
import { environment } from '../../../environments/environment';
import { User } from '../models/user.models';
import {
    AuthResponse,
    LoginCredentials,
    RegisterCredentials,
} from '../models/auth.models';

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    private currentUserSubject = new BehaviorSubject<User | null>(null);
    public currentUser$ = this.currentUserSubject.asObservable();
    public isAuthenticated$ = this.currentUser$.pipe(map((user) => !!user));
    public redirectUrl: string | null = null;

    private readonly apiUrl = environment.apiUrl;

    constructor(
        private http: HttpClient,
        private router: Router,
        private logger: LoggerService,
    ) {
        this.checkAuthStatus();
    }

    // Check if user is already authenticated (cookies will be sent automatically)
    checkAuthStatus(): Observable<boolean> {
        return this.http.get<AuthResponse>(`${this.apiUrl}/oauth/status`).pipe(
            tap((response) => {
                if (response.success && response.user) {
                    this.currentUserSubject.next(response.user);
                    this.logger.info('User authenticated', response.user);
                } else {
                    this.currentUserSubject.next(null);
                }
            }),
            map((response) => response.success),
            catchError((error) => {
                this.logger.error('Auth status check failed', error);
                this.currentUserSubject.next(null);
                return of(false);
            }),
        );
    }

    refreshAuthStatus(): Observable<boolean> {
        return this.http.get<AuthResponse>(`${this.apiUrl}/oauth/status`, {
            // Force the request to go to the network, not use cache
            headers: {
                'Cache-Control': 'no-cache',
                'Pragma': 'no-cache'
            }
        }).pipe(
            tap((response) => {
                if (response.success && response.user) {
                    this.currentUserSubject.next(response.user);
                    this.logger.info('User authenticated', response.user);
                } else {
                    this.currentUserSubject.next(null);
                }
            }),
            map((response) => response.success),
            catchError((error) => {
                this.logger.error('Auth status refresh failed', error);
                return of(false);
            }),
        );
    }

    // Login with email/password
    login(userData: Partial<LoginCredentials>): Observable<User> {
        return this.http
            .post<AuthResponse>(`${this.apiUrl}/auth/signin`, userData)
            .pipe(
                map((response) => {
                    if (response.success && response.user) {
                        this.currentUserSubject.next(response.user);
                        this.logger.info('User logged in', response.user);
                        return response.user;
                    } else {
                        throw new Error(response.message || 'Login failed');
                    }
                }),
                catchError((error) => {
                    this.logger.error('Login failed', error);
                    return throwError(() => error);
                }),
            );
    }

    // Logout
    logout(): Observable<boolean> {
        return this.http
            .post<AuthResponse>(`${this.apiUrl}/auth/signout`, {})
            .pipe(
                tap(() => {
                    this.currentUserSubject.next(null);
                    this.router.navigate(['/auth/signin']);
                    this.logger.info('User logged out');
                }),
                map((response) => response.success),
                catchError((error) => {
                    this.logger.error('Logout failed', error);
                    // Even if server logout fails, clear local state
                    this.currentUserSubject.next(null);
                    return of(false);
                }),
            );
    }

    // Register
    register(userData: Partial<RegisterCredentials>): Observable<User> {
        return this.http
            .post<AuthResponse>(`${this.apiUrl}/auth/signup`, userData)
            .pipe(
                map((response) => {
                    if (response.success && response.user) {
                        this.currentUserSubject.next(response.user);
                        this.logger.info(
                            'User registered and logged in',
                            response.user,
                        );
                        return response.user;
                    } else {
                        throw new Error(
                            response.message || 'Registration failed',
                        );
                    }
                }),
                catchError((error) => {
                    this.logger.error('Registration failed', error);
                    return throwError(() => error);
                }),
            );
    }

    // Confirm email
    confirmEmail(token: string, email: string): Observable<boolean> {
        return this.http
            .post<AuthResponse>(`${this.apiUrl}/auth/confirm-email`, { token, email })
            .pipe(
                map((response) => {
                    if (response.success) {
                        this.logger.info('Email confirmed successfully');
                        // If the confirmation also logs the user in, update user state
                        if (response.user) {
                            this.currentUserSubject.next(response.user);
                        }
                        return true;
                    } else {
                        throw new Error(response.message || 'Email confirmation failed');
                    }
                }),
                catchError((error) => {
                    this.logger.error('Email confirmation failed', error);
                    return throwError(() => error);
                }),
            );
    }

    // Resend confirmation email
    resendConfirmationEmail(email: string): Observable<boolean> {
        return this.http
            .post<AuthResponse>(`${this.apiUrl}/auth/resend-confirmation`, { email })
            .pipe(
                map((response) => {
                    if (response.success) {
                        this.logger.info('Confirmation email sent successfully');
                        return true;
                    } else {
                        throw new Error(response.message || 'Failed to resend confirmation email');
                    }
                }),
                catchError((error) => {
                    this.logger.error('Failed to resend confirmation email', error);
                    return throwError(() => error);
                }),
            );
    }

    // Get current user profile
    getCurrentUser(): Observable<User> {
        return this.http.get<User>(`${this.apiUrl}/auth/current`).pipe(
            tap((user) => {
                this.currentUserSubject.next(user);
            }),
            catchError((error) => {
                this.logger.error('Failed to get current user', error);
                return throwError(() => error);
            }),
        );
    }

    // Helper methods
    get currentUser(): User | null {
        return this.currentUserSubject.value;
    }

    isAuthenticated(): boolean {
        return !!this.currentUserSubject.value;
    }

    hasRole(role: string): boolean {
        const user = this.currentUserSubject.value;
        return !!user?.roles?.includes(role);
    }
}
