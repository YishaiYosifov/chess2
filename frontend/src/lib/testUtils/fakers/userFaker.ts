import { faker } from "@faker-js/faker";

import { PrivateUser, User } from "@/lib/apiClient/models";

export function createUser(override?: Partial<User>): User {
    return {
        authedUserId: faker.number.int(),
        username: faker.internet.username(),
        about: faker.lorem.paragraph(),
        countryCode: "IL",
        pfpLastChanged: new Date().getTime(),
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
