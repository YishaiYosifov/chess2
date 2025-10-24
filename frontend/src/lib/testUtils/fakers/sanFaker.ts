import { faker } from "@faker-js/faker";

export function createFakeSan(): string {
    return faker.helpers.arrayElement(["e4", "d4", "Nf3", "Nc6", "Bb5", "e5"]);
}
