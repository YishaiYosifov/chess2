import { faker } from "@faker-js/faker";

import { PrivateUser, PublicUser } from "@/lib/apiClient";

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
        email: faker.internet.username(),
        type: "authed",
        ...override,
    };
}
