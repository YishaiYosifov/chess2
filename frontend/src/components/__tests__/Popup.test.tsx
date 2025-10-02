import { render, screen } from "@testing-library/react";
import Popup from "../Popup";
import userEvent from "@testing-library/user-event";

describe("Popup", () => {
    it("should render children correctly", () => {
        render(
            <Popup closePopup={() => {}}>
                <div data-testid="child">Hello</div>
            </Popup>,
        );
        expect(screen.getByTestId("child")).toBeInTheDocument();
        expect(screen.getByText("Hello")).toBeVisible();
    });

    it("should call closePopup when clicking the background", async () => {
        const user = userEvent.setup();
        const closePopup = vi.fn();
        render(<Popup closePopup={closePopup}>Content</Popup>);

        await user.click(screen.getByTestId("popupBackground"));
        expect(closePopup).toHaveBeenCalledTimes(1);
    });

    it("should call closePopup when clicking the close button", async () => {
        const user = userEvent.setup();
        const closePopup = vi.fn();
        render(<Popup closePopup={closePopup}>Content</Popup>);

        await user.click(screen.getByTestId("closePopup"));
        expect(closePopup).toHaveBeenCalledTimes(1);
    });

    it("should not call closePopup when clicking inside the popup content", async () => {
        const user = userEvent.setup();
        const closePopup = vi.fn();
        render(
            <Popup closePopup={closePopup}>
                <div data-testid="insidePopup">Inside</div>
            </Popup>,
        );

        await user.click(screen.getByTestId("insidePopup"));
        expect(closePopup).not.toHaveBeenCalled();
    });

    it("should apply custom className to the popup content", () => {
        render(
            <Popup closePopup={() => {}} className="custom-popup-class">
                Content
            </Popup>,
        );
        const popupContent = screen.getByTestId("popup");
        expect(popupContent).toHaveClass("custom-popup-class");
    });
});
