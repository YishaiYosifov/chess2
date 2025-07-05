import { faker } from "@faker-js/faker";

import { PrivateUser, User } from "@/lib/apiClient";

export function createUser(override?: Partial<User>): User {
    return {
        userId: faker.string.uuid(),
        userName: faker.internet.username(),
        about: faker.lorem.paragraph(),
        countryCode: "IL",
        ...override,
    };
}

export function createPrivateUser(
    override?: Partial<PrivateUser>,
): PrivateUser {
    return {
        ...createUser(),
        email: faker.internet.username(),
        ...override,
    };
}
