export interface User {
    id: string;
    email: string;
    displayName?: string;
    photoUrl?: string;
    roles?: string[];
}

export interface LoginCredentials {
    email: string;
    password: string;
    rememberMe?: boolean;
}

export interface RegisterCredentials {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
}

export interface AuthResponse {
    success: boolean;
    user?: User;
    message?: string;
    token?: string; // For potential JWT support in the future
}