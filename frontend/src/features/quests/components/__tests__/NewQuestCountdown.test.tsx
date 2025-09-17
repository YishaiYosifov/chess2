import { act, render, screen } from "@testing-library/react";
import NewQuestCountdown from "../NewQuestCountdown";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";

describe("NewQuestCountdown", () => {
    beforeEach(() => {
        vi.useFakeTimers();
    });

    it("should render countdown correctly", () => {
        const now = new Date(Date.UTC(2025, 8, 13, 23, 59, 50)); // 23:59:50
        vi.setSystemTime(now);

        render(<NewQuestCountdown />);

        expect(screen.getByTestId("newQuestText")).toHaveTextContent(
            "New quest in 00:00:10",
        );
    });

    it("should call router.refresh when day changes", () => {
        const now = new Date(Date.UTC(2025, 8, 13, 23, 59, 59));
        vi.setSystemTime(now);
        const routerMock = mockRouter();

        render(<NewQuestCountdown />);

        act(() => {
            vi.advanceTimersByTime(2000);
        });

        expect(routerMock.refresh).toHaveBeenCalled();
    });
});
