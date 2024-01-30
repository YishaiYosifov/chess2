import { PublicUserOut } from "@/client";

export const profileMock: PublicUserOut = {
    userId: 1,
    username: "testuser",
    about: "test about",

    countryAlpha3: "ISR",
    pfpLastChanged: new Date("2023-01-01T12:00:00Z"),
};

export function createProfile(
    overrideAttrs: Partial<PublicUserOut> = {}
): PublicUserOut {
    return { ...profileMock, ...overrideAttrs };
}
