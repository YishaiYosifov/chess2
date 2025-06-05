import { render, screen } from "@testing-library/react";
import SeekingOverlay from "../SeekingOverlay";
import userEvent from "@testing-library/user-event";

describe("SeekingOverlay", () => {
    it("should render overlay with spinner, message, and cancel button", () => {
        const onClick = vi.fn();
        render(<SeekingOverlay onClick={onClick} />);

        expect(screen.getByTestId("seekingOverlay")).toBeInTheDocument();
        expect(screen.getByTestId("seekingSpinner")).toBeInTheDocument();
        expect(screen.getByText(/searching for a match/i)).toBeInTheDocument();

        const cancelButton = screen.getByTestId("cancelSeekButton");
        expect(cancelButton).toBeInTheDocument();
        expect(cancelButton).toHaveTextContent(/cancel/i);
    });

    it("should call onClick when cancel button is clicked", async () => {
        const user = userEvent.setup();
        const onClick = vi.fn();
        render(<SeekingOverlay onClick={onClick} />);

        const button = screen.getByTestId("cancelSeekButton");
        await user.click(button);

        expect(onClick).toHaveBeenCalledTimes(1);
    });
});
