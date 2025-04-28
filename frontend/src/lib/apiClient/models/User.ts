export enum UserType {
    Authed = "authed",
    Guest = "guest",
}

export interface UserIn {
    userName: string;
    email: string;
    password: string;
    countryCode?: string;
}

export interface UserLogin {
    usernameOrEmail: string;
    password: string;
}

export interface User {
    userId: number;
    userName: string;
    about: string;
    countryCode?: string;
    pfpLastChanged: number;
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
