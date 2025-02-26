import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, of } from 'rxjs';
import { User } from '../models/user.model';
import {
    RegisterRequest,
    ApiResponse,
    LoginRequest,
    LoginResponse,
    EmailConfirmationRequest,
    PasswordResetRequest,
} from '../models/auth.model';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    private apiUrl = environment.apiUrl + '/auth';

    constructor(private http: HttpClient) {}

    register(userData: RegisterRequest): Observable<ApiResponse> {
        return this.http.post<ApiResponse>(`${this.apiUrl}/signup`, userData);
    }

    login(credentials: LoginRequest): Observable<LoginResponse> {
        // withCredentials ensures the browser sends/receives cookies.
        return this.http.post<LoginResponse>(
            `${this.apiUrl}/signin`,
            credentials,
            {
                withCredentials: true,
            }
        );
    }

    confirmEmail(data: EmailConfirmationRequest): Observable<ApiResponse> {
        return this.http.post<ApiResponse>(
            `${this.apiUrl}/confirm-email`,
            data
        );
    }

    requestPasswordReset(email: string): Observable<ApiResponse> {
        return this.http.post<ApiResponse>(
            `${this.apiUrl}/request-password-reset`,
            {
                email,
            }
        );
    }

    resetPassword(data: PasswordResetRequest): Observable<ApiResponse> {
        return this.http.post<ApiResponse>(
            `${this.apiUrl}/reset-password`,
            data
        );
    }

    currentUser(): Observable<User | null> {
        // If API is not available, return mock user
        if (environment.useMockData) {
            return of({
                id: '1',
                displayName: 'Test User',
                email: 'test@example.com',
                photoUrl: 'assets/images/default-avatar.png',
            } as User);
        }

        // Otherwise call the real API
        return this.http.get<User>(`${this.apiUrl}/current`).pipe(
            catchError((error) => {
                console.error('Error fetching current user:', error);
                return of(null);
            })
        );
    }

    logout(): Observable<ApiResponse> {
        return this.http.post<ApiResponse>(`${this.apiUrl}/signout`, null, {
            withCredentials: true,
        });
    }
}
