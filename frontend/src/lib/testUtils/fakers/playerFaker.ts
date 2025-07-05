import { GameColor, GamePlayer } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createPlayer(
    color: GameColor,
    override?: Partial<GamePlayer>,
): GamePlayer {
    return {
        userId: faker.string.uuid(),
        color,
        userName: faker.internet.username(),
        countryCode: faker.location.countryCode(),
        rating: faker.number.int({ min: 1200, max: 2400 }),
        ...override,
    };
}
