import { render, screen } from "@testing-library/react";
import GuideCard from "../GuideCard";
import Image, { StaticImageData } from "next/image";
import { mock } from "vitest-mock-extended";

vi.mock("next/image");

describe("GuideCard", () => {
    const imagesMock = [
        mock<StaticImageData>({ src: "image1.jpg" }),
        mock<StaticImageData>({ src: "image2.jpg" }),
        mock<StaticImageData>({ src: "image3.jpg" }),
    ];

    const multiplePoints = ["point 1", "point 2"];
    const singlePoint = ["single point"];
    const title = "Test Title";

    it("should render the title and all list points", () => {
        render(
            <GuideCard
                title={title}
                points={multiplePoints}
                images={imagesMock}
            />,
        );

        const titleEl = screen.getByTestId("guideCardTitle");
        expect(titleEl).toHaveTextContent(title);

        const list = screen.getByTestId("guideCardPoints");
        const items = list.querySelectorAll("li");
        expect(items.length).toBe(multiplePoints.length);
        multiplePoints.forEach((point, i) =>
            expect(items[i]).toHaveTextContent(point),
        );
    });

    it("should render a single paragraph if there is only one point", () => {
        render(
            <GuideCard
                title={title}
                points={singlePoint}
                images={imagesMock}
            />,
        );

        const paragraph = screen.getByTestId("guideCardSinglePoint");
        expect(paragraph).toHaveTextContent(singlePoint[0]);

        expect(screen.queryByTestId("guideCardPoints")).not.toBeInTheDocument();
    });

    it("should render the carousel with the correct images and alt text", () => {
        render(
            <GuideCard
                title={title}
                points={multiplePoints}
                images={imagesMock}
            />,
        );

        const carousel = screen.getByTestId("carousel");
        expect(carousel).toBeInTheDocument();

        imagesMock.forEach((_, i) => {
            const img = screen.getByTestId(`carouselImage-${i}`);
            expect(Image).toHaveBeenCalledWith(
                expect.objectContaining({ src: imagesMock[i] }),
                undefined,
            );
            expect(img).toHaveAttribute("alt", `${title} Example ${i + 1}`);
        });
    });
});
