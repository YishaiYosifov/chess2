import { faker } from "@faker-js/faker";

import { PublicUser } from "@/lib/apiClient";

export function createFakeUser(override?: Partial<PublicUser>): PublicUser {
    return {
        userId: faker.string.uuid(),
        userName: faker.internet.username(),
        about: faker.lorem.paragraph(),
        countryCode: faker.location.countryCode(),
        type: "authed",

        ...override,
    };
}
