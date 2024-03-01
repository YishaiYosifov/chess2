import { PrivateAuthedProfileOut, AuthedProfileOut, UserType } from "@/client";

export const profileMock: AuthedProfileOut = {
    userId: 1,
    userType: UserType.Authed,

    username: "testuser",
    firstName: "first",
    lastName: "last",
    about: "test about",

    countryAlpha3: "ISR",
    location: "luka st. 69",
    pfpLastChanged: new Date("2023-01-01T12:00:00Z"),
};

export const privateProfileMock: PrivateAuthedProfileOut = {
    ...profileMock,
    email: "test@example.com",
    usernameLastChanged: new Date("2023-01-01T12:00:00Z"),
};

export function createProfile(
    overrideAttrs: Partial<AuthedProfileOut> = {}
): AuthedProfileOut {
    return { ...profileMock, ...overrideAttrs };
}
