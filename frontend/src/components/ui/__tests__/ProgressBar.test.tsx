import { render, screen } from "@testing-library/react";
import ProgressBar from "../ProgressBar";

describe("ProgressBar", () => {
    it("should render the progress bar", () => {
        render(<ProgressBar percent={50} />);

        expect(screen.getByTestId("progressBar")).toBeInTheDocument();
        expect(screen.getByTestId("progressBarFill")).toBeInTheDocument();
    });

    it("should render the progress bar fill with correct width", () => {
        const percent = 75;
        render(<ProgressBar percent={percent} />);

        const fill = screen.getByTestId("progressBarFill");
        expect(fill).toBeInTheDocument();
        expect(fill).toHaveStyle(`width: ${percent}%`);
    });

    it("should render 0% width when percent is 0", () => {
        render(<ProgressBar percent={0} />);
        expect(screen.getByTestId("progressBarFill")).toHaveStyle("width: 0%");
    });

    it("should render 100% width when percent is 100", () => {
        render(<ProgressBar percent={100} />);
        expect(screen.getByTestId("progressBarFill")).toHaveStyle(
            "width: 100%",
        );
    });
});
