export interface User {
    id: string;
    email: string;
    firstName?: string;
    lastName?: string;
    displayName?: string;
    photoUrl?: string;
    roles?: string[];
}
