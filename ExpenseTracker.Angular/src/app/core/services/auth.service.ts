import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, map, tap } from 'rxjs/operators';
import { LoggerService } from './logger.service';
import { environment } from '../../../environments/environment';

export interface User {
    id: string;
    email: string;
    displayName?: string;
    photoUrl?: string;
    roles?: string[];
    // Add other user properties as needed
}

export interface AuthResponse {
    success: boolean;
    user?: User;
    message?: string;
}

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
        private logger: LoggerService
    ) {
        this.checkAuthStatus();
    }

    // Check if user is already authenticated (cookies will be sent automatically)
    checkAuthStatus(): Observable<boolean> {
        return this.http.get<AuthResponse>(`${this.apiUrl}/auth/status`)
            .pipe(
                tap(response => {
                    if (response.success && response.user) {
                        this.currentUserSubject.next(response.user);
                        this.logger.info('User authenticated', response.user);
                    } else {
                        this.currentUserSubject.next(null);
                    }
                }),
                map(response => response.success),
                catchError(error => {
                    this.logger.error('Auth status check failed', error);
                    this.currentUserSubject.next(null);
                    return of(false);
                })
            );
    }

    // Login with email/password
    login(email: string, password: string): Observable<User> {
        return this.http.post<AuthResponse>(`${this.apiUrl}/auth/login`, { email, password })
            .pipe(
                map(response => {
                    if (response.success && response.user) {
                        this.currentUserSubject.next(response.user);
                        this.logger.info('User logged in', response.user);
                        return response.user;
                    } else {
                        throw new Error(response.message || 'Login failed');
                    }
                }),
                catchError(error => {
                    this.logger.error('Login failed', error);
                    return throwError(() => error);
                })
            );
    }

    // Logout
    logout(): Observable<boolean> {
        return this.http.post<AuthResponse>(`${this.apiUrl}/auth/logout`, {})
            .pipe(
                tap(() => {
                    this.currentUserSubject.next(null);
                    this.router.navigate(['/auth/login']);
                    this.logger.info('User logged out');
                }),
                map(response => response.success),
                catchError(error => {
                    this.logger.error('Logout failed', error);
                    // Even if server logout fails, clear local state
                    this.currentUserSubject.next(null);
                    return of(false);
                })
            );
    }

    // Register
    register(userData: Partial<User>): Observable<User> {
        return this.http.post<AuthResponse>(`${this.apiUrl}/auth/register`, userData)
            .pipe(
                map(response => {
                    if (response.success && response.user) {
                        this.currentUserSubject.next(response.user);
                        this.logger.info('User registered and logged in', response.user);
                        return response.user;
                    } else {
                        throw new Error(response.message || 'Registration failed');
                    }
                }),
                catchError(error => {
                    this.logger.error('Registration failed', error);
                    return throwError(() => error);
                })
            );
    }

    // Initiate GitHub OAuth login
    loginWithGithub(): void {
        window.location.href = `${this.apiUrl}/auth/github`;
    }

    // Initiate Microsoft Entra ID login
    loginWithMicrosoft(): void {
        window.location.href = `${this.apiUrl}/auth/microsoft`;
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
