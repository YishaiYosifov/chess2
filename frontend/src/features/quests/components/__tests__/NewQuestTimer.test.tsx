import { act, render, screen } from "@testing-library/react";
import NewQuestTimer from "../NewQuestTimer";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";

describe("NewQuestTimer", () => {
    beforeEach(() => {
        vi.useFakeTimers();
    });

    afterEach(() => {
        vi.useRealTimers();
        vi.restoreAllMocks();
    });

    it("should render initial countdown correctly", () => {
        const now = new Date(Date.UTC(2025, 8, 13, 23, 59, 50)); // 23:59:50
        vi.setSystemTime(now);

        render(<NewQuestTimer />);

        expect(screen.getByTestId("newQuestText")).toHaveTextContent(
            "New Quest in 00:00:10",
        );
    });

    it("should update countdown every second", () => {
        const now = new Date(Date.UTC(2025, 8, 13, 23, 59, 50));
        vi.setSystemTime(now);

        render(<NewQuestTimer />);

        act(() => {
            vi.advanceTimersByTime(3000);
        });

        expect(screen.getByTestId("newQuestText")).toHaveTextContent(
            "New Quest in 00:00:07",
        );
    });

    it("should call router.refresh when day changes", () => {
        const now = new Date(Date.UTC(2025, 8, 13, 23, 59, 59));
        vi.setSystemTime(now);
        const routerMock = mockRouter();

        render(<NewQuestTimer />);

        act(() => {
            vi.advanceTimersByTime(2000);
        });

        expect(routerMock.refresh).toHaveBeenCalled();
    });
});
