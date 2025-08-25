import { faker } from "@faker-js/faker";

import { PrivateUser, PublicUser } from "@/lib/apiClient";
import constants from "@/lib/constants";

export function createFakeUser(override?: Partial<PublicUser>): PublicUser {
    return {
        userId: faker.string.uuid(),
        userName: faker.internet.username(),
        about: faker.lorem.paragraph(),
        countryCode: faker.location.countryCode(),
        ...override,
    };
}

export function createFakePrivateUser(
    override?: Partial<PrivateUser>,
): PrivateUser {
    return {
        ...createFakeUser(),
        usernameLastChanged:
            new Date().valueOf() - constants.USERNAME_EDIT_EVERY_SECONDS,
        type: "authed",

        ...override,
    };
}
