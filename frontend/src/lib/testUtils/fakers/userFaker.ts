import { faker } from "@faker-js/faker";

import { GuestUser, PrivateUser, PublicUser } from "@/lib/apiClient";
import constants from "@/lib/constants";

export function createFakeUser(override?: Partial<PublicUser>): PublicUser {
    return {
        userId: faker.string.uuid(),
        userName: faker.internet.username(),
        about: faker.lorem.paragraph(),
        countryCode: "XX",
        createdAt: new Date().toISOString(),
        ...override,
    };
}

export function createFakePrivateUser(
    override?: Partial<PrivateUser>,
): PrivateUser {
    return {
        ...createFakeUser(),
        usernameLastChanged: new Date(
            new Date().valueOf() - constants.USERNAME_EDIT_EVERY_MS,
        ).toISOString(),
        createdAt: new Date().toISOString(),
        type: "authed",

        ...override,
    };
}

export function createFakeGuestUser(override?: Partial<GuestUser>): GuestUser {
    return {
        userId: "guest:" + faker.string.uuid(),
        type: "guest",

        ...override,
    };
}
