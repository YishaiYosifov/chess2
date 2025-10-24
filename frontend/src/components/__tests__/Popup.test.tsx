import { render, screen } from "@testing-library/react";
import Popup, { PopupRef } from "../Popup";
import userEvent from "@testing-library/user-event";
import React, { act } from "react";

describe("Popup", () => {
    const ref = React.createRef<PopupRef>();

    it("should not render before opening", () => {
        render(<Popup ref={ref}>Content</Popup>);
        expect(screen.queryByTestId("popup")).not.toBeInTheDocument();
    });

    it("should render correctly", () => {
        render(
            <Popup ref={ref}>
                <div data-testid="child">Hello</div>
            </Popup>,
        );
        act(() => ref.current?.open());

        expect(screen.getByTestId("child")).toBeInTheDocument();
        expect(screen.getByText("Hello")).toBeVisible();
        expect(screen.getByTestId("popup")).toBeInTheDocument();
    });

    it("should close when clicking the background", async () => {
        const user = userEvent.setup();
        render(<Popup ref={ref}>Content</Popup>);
        act(() => ref.current?.open());

        await user.click(screen.getByTestId("popupBackground"));
        expect(screen.queryByTestId("popup")).not.toBeInTheDocument();
    });

    it("should close clicking the close button", async () => {
        const user = userEvent.setup();
        render(<Popup ref={ref}>Content</Popup>);
        act(() => ref.current?.open());

        await user.click(screen.getByTestId("closePopup"));
        expect(screen.queryByTestId("popup")).not.toBeInTheDocument();
    });

    it("should not close when clicking inside the popup content", async () => {
        const user = userEvent.setup();
        render(
            <Popup ref={ref}>
                <div data-testid="insidePopup">Inside</div>
            </Popup>,
        );
        act(() => ref.current?.open());

        await user.click(screen.getByTestId("insidePopup"));
        expect(screen.getByTestId("popup")).toBeInTheDocument();
    });

    it("should apply custom className to the popup content", () => {
        render(
            <Popup ref={ref} className="custom-popup-class">
                Content
            </Popup>,
        );
        act(() => ref.current?.open());
        expect(screen.getByTestId("popup")).toHaveClass("custom-popup-class");
    });
});
