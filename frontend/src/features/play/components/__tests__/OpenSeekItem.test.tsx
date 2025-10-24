import { render, screen } from "@testing-library/react";

import { OpenSeek } from "@/features/lobby/lib/types";
import { PoolType, TimeControl } from "@/lib/apiClient";
import OpenSeekItem from "../OpenSeekItem";
import createFakeOpenSeek from "@/lib/testUtils/fakers/openSeekerFaker";
import userEvent from "@testing-library/user-event";
import useLobbyStore from "@/features/lobby/stores/lobbyStore";
import { useLobbyEmitter } from "@/features/lobby/hooks/useLobbyHub";

vi.mock("@/features/lobby/hooks/useLobbyHub");

vi.mock("@/features/lobby/Components/TimeControlIcon", () => ({
    default: ({ timeControl }: { timeControl: TimeControl }) => (
        <div data-testid="timeControlIcon">{timeControl}</div>
    ),
}));

vi.mock("@heroicons/react/24/outline", () => ({
    FireIcon: () => <div data-testid="fireIcon" />,
}));

describe("OpenSeekItem", () => {
    let seek: OpenSeek;
    const sendLobbyEventsMock = vi.fn();

    beforeEach(() => {
        seek = createFakeOpenSeek();
        vi.mocked(useLobbyEmitter).mockReturnValue(sendLobbyEventsMock);
        useLobbyStore.setState(useLobbyStore.getInitialState());
    });

    it("should display the user name", () => {
        render(<OpenSeekItem seek={seek} />);
        expect(screen.getByTestId("openSeekUsername")).toHaveTextContent(
            seek.userName,
        );
    });

    it("should display the correct time control", () => {
        seek.pool.timeControl = { baseSeconds: 300, incrementSeconds: 5 };
        render(<OpenSeekItem seek={seek} />);
        expect(screen.getByTestId("openSeekTimeControl")).toHaveTextContent(
            "5+5",
        );
    });

    it("should display 'casual' for casual pool", () => {
        render(<OpenSeekItem seek={seek} />);
        expect(screen.getByTestId("openSeekPoolType")).toHaveTextContent(
            "casual",
        );
    });

    it("should display 'rated' and rating for rated pool", () => {
        seek.rating = 1200;
        seek.pool.poolType = PoolType.RATED;
        render(<OpenSeekItem seek={seek} />);

        expect(screen.getByTestId("openSeekPoolType")).toHaveTextContent(
            "rated - 1200",
        );
    });

    it("should render the TimeControlIcon and FireIcon", () => {
        render(<OpenSeekItem seek={seek} />);
        expect(screen.getByTestId("timeControlIcon")).toBeInTheDocument();
        expect(screen.getByTestId("fireIcon")).toBeInTheDocument();
    });

    it("should send match request when clicked", async () => {
        const user = userEvent.setup();
        render(<OpenSeekItem seek={seek} />);

        await user.click(screen.getByTestId("openSeek"));

        expect(sendLobbyEventsMock).toHaveBeenCalledExactlyOnceWith(
            "MatchWithOpenSeekAsync",
            seek.userId,
            seek.pool,
        );
    });

    it("should mark as requesting open seek when clicked", async () => {
        const user = userEvent.setup();

        render(<OpenSeekItem seek={seek} />);

        expect(useLobbyStore.getState().requestedOpenSeek).toBe(false);
        await user.click(screen.getByTestId("openSeek"));
        expect(useLobbyStore.getState().requestedOpenSeek).toBe(true);
    });
});
