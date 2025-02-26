import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class ApiService {
    private baseUrl = 'https://localhost:8443/api';

    constructor(private http: HttpClient) {}

    get<T>(endpoint: string, params?: HttpParams): Observable<T> {
        return this.http.get<T>(`${this.baseUrl}/${endpoint}`, {
            params,
            withCredentials: true,
        });
    }

    post<T>(endpoint: string, data: unknown): Observable<T> {
        return this.http.post<T>(`${this.baseUrl}/${endpoint}`, data, {
            withCredentials: true,
        });
    }

    put<T>(endpoint: string, data: unknown): Observable<T> {
        return this.http.put<T>(`${this.baseUrl}/${endpoint}`, data, {
            withCredentials: true,
        });
    }

    delete<T>(endpoint: string): Observable<T> {
        return this.http.delete<T>(`${this.baseUrl}/${endpoint}`, {
            withCredentials: true,
        });
    }
}
