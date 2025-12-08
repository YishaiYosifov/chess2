import { render, screen } from "@testing-library/react";
import Flag from "../Flag";

describe("Flag component", () => {
    it("should render the flag image", () => {
        render(<Flag countryCode="US" size={32} />);
        const img = screen.getByAltText("flag");
        expect(img).toBeInTheDocument();
    });

    it("should use the lowercase country code in the src", () => {
        render(<Flag countryCode="GB" size={24} />);
        const img = screen.getByAltText("flag");
        expect(img.getAttribute("src")).toBe("/assets/flags/gb.svg");
    });

    it("should apply width and height based on size prop", () => {
        render(<Flag countryCode="FR" size={48} />);

        const img = screen.getByAltText("flag");

        expect(img.getAttribute("width")).toBe("48");
        expect(img.getAttribute("height")).toBe("48");
    });
});
