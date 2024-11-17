import { faker } from "@faker-js/faker";

import { PrivateUser, User } from "@/lib/apiClient/models";

export function createUser(): User {
    return {
        userId: faker.number.int(),
        username: faker.internet.username(),
        about: faker.lorem.paragraph(),
        countryCode: "IL",
    };
}

export function createPrivateUser(): PrivateUser {
    return {
        ...createUser(),
        email: faker.internet.username(),
    };
}
