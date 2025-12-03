import { render, screen } from "@testing-library/react";

import { createFakeOngoingGame } from "@/lib/testUtils/fakers/ongoingGameFaker";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import OngoingGameItem from "../OngoingGameItem";
import { PoolType, TimeControl } from "@/lib/apiClient";
import { createFakePoolKey } from "@/lib/testUtils/fakers/poolKeyFaker";
import userEvent from "@testing-library/user-event";
import constants from "@/lib/constants";

describe("OngoingGameItem", () => {
    it("should render the opponent username", () => {
        const game = createFakeOngoingGame();
        render(<OngoingGameItem game={game} />);

        expect(screen.getByTestId("ongoingGameUsername").textContent).toBe(
            game.opponent.userName,
        );
    });

    it("should render the correct time control", () => {
        const game = createFakeOngoingGame({
            pool: {
                ...createFakeOngoingGame().pool,
                timeControl: {
                    type: TimeControl.BLITZ,
                    baseSeconds: 300,
                    incrementSeconds: 5,
                },
            },
        });

        render(<OngoingGameItem game={game} />);

        expect(screen.getByTestId("ongoingGameTimeControl").textContent).toBe(
            "5+5",
        );
    });

    it.each([
        { poolType: PoolType.RATED, label: "rated" },
        { poolType: PoolType.CASUAL, label: "casual" },
    ])("should render the correct pool type label", ({ poolType, label }) => {
        const game = createFakeOngoingGame({
            pool: createFakePoolKey({ poolType }),
        });

        render(<OngoingGameItem game={game} />);

        expect(screen.getByTestId("ongoingGamePoolType").textContent).toBe(
            label,
        );
    });

    it("should navigate to the ongoing game when clicked", async () => {
        const game = createFakeOngoingGame();
        const user = userEvent.setup();
        const routerMock = mockRouter();

        render(<OngoingGameItem game={game} />);

        await user.click(
            screen.getByTestId(`ongoingGameItem-${game.gameToken}`),
        );

        expect(routerMock.push).toHaveBeenCalledWith(
            `${constants.PATHS.GAME}/${game.gameToken}`,
        );
    });
});
