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
    email: string;
    password: string;
    displayName?: string;
}

export interface AuthResponse {
    success: boolean;
    user?: User;
    message?: string;
    token?: string; // For potential JWT support in the future
}
