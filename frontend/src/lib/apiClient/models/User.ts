export enum UserType {
    Authed = "authed",
    Guest = "guest",
}

export interface UserIn {
    username: string;
    email: string;
    password: string;
    countryCode: string | null;
}

export interface UserLogin {
    usernameOrEmail: string;
    password: string;
}

export interface User {
    userId: number;
    username: string;
    about: string;
    countryCode?: string;
}

export interface PrivateUser extends User {
    email: string;
}

export interface EditableProfile {
    firstName: string;
    lastName: string;
    about: string;

    countryAlpha3: string;
    location: string;
}
