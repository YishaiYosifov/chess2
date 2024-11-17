export enum UserType {
    Authed = "authed",
    Guest = "guest",
}

export interface UserIn {
    username: string;
    email: string;
    password: string;
    countryCode: string;
}

export interface UserLogin {
    usernameOrEmail: string;
    Password: string;
}

export interface UnauthedProfileOut {
    userId: number;
    userType: UserType;
    username: string;
}

export interface AuthedProfileOut {
    userId: number;
    userType: UserType;

    username: string;
    firstName: string;
    lastName: string;
    location: string;
    about: string;

    countryAlpha3: string;
    pfpLastChanged: Date;
}

export interface PrivateAuthedProfileOut extends AuthedProfileOut {
    email: string;
    usernameLastChanged: Date | null;
}

export interface EditableProfile {
    firstName: string;
    lastName: string;
    about: string;

    countryAlpha3: string;
    location: string;
}
