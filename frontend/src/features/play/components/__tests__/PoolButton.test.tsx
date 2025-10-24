import { render, screen } from "@testing-library/react";
import PoolButton from "../PoolButton";
import userEvent from "@testing-library/user-event";
import { PoolType } from "@/lib/apiClient";
import useMatchmaking from "@/features/lobby/hooks/useMatchmaking";

vi.mock("@/features/lobby/hooks/useMatchmaking");

describe("PoolButton", () => {
    const useMatchmakingMock = vi.mocked(useMatchmaking);

    function setupUseMatchmakingMock(
        overrides?: Partial<ReturnType<typeof useMatchmaking>>,
    ) {
        const defaultMock = {
            isSeeking: false,
            toggleSeek: vi.fn().mockResolvedValue(undefined),
            createSeek: vi.fn().mockResolvedValue(undefined),
            cancelSeek: vi.fn().mockResolvedValue(undefined),
        };
        useMatchmakingMock.mockReturnValue({ ...defaultMock, ...overrides });
        return defaultMock;
    }

    it("should render the formatted time control and label", () => {
        setupUseMatchmakingMock();

        render(
            <PoolButton
                timeControl={{ baseSeconds: 300, incrementSeconds: 3 }}
                label="Rapid"
                poolType={PoolType.RATED}
            />,
        );

        expect(screen.getByText("5 + 3")).toBeInTheDocument();
        expect(screen.getByText("Rapid")).toBeInTheDocument();
    });

    it("should show 'Most Popular' label and apply border if isMostPopular is true", () => {
        setupUseMatchmakingMock();

        render(
            <PoolButton
                timeControl={{ baseSeconds: 180, incrementSeconds: 2 }}
                label="Blitz"
                poolType={PoolType.CASUAL}
                isMostPopular
            />,
        );

        expect(screen.getByText("Most Popular")).toBeInTheDocument();
        const button = screen.getByRole("button");
        expect(button.className).toMatch(/border-amber-300/);
    });

    it("should show 'searching...' text and animate ping when isSeeking is true", () => {
        setupUseMatchmakingMock({ isSeeking: true });

        const { container } = render(
            <PoolButton
                timeControl={{ baseSeconds: 600, incrementSeconds: 5 }}
                label="Classic"
                poolType={PoolType.RATED}
            />,
        );

        expect(screen.getByText("searching...")).toBeInTheDocument();
        const wrapperDiv = container.querySelector("div");
        expect(wrapperDiv?.className).toMatch(/animate-subtle-ping/);
    });

    it("should call toggleSeek when the button is clicked", async () => {
        const user = userEvent.setup();
        const { toggleSeek } = setupUseMatchmakingMock();

        render(
            <PoolButton
                timeControl={{ baseSeconds: 60, incrementSeconds: 5 }}
                label="Bullet"
                poolType={PoolType.RATED}
            />,
        );

        const button = screen.getByRole("button");
        await user.click(button);

        expect(toggleSeek).toHaveBeenCalled();
    });

    it("should call useMatchmaking with the correct pool object", () => {
        const timeControl = { baseSeconds: 120, incrementSeconds: 5 };
        const poolType = PoolType.CASUAL;

        setupUseMatchmakingMock();

        render(
            <PoolButton
                timeControl={timeControl}
                label="Blitz"
                poolType={poolType}
            />,
        );

        expect(useMatchmakingMock).toHaveBeenCalledWith({
            poolType,
            timeControl,
        });
    });
});
