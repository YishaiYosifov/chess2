import { render, screen } from "@testing-library/react";
import Carousel from "../Carousel";
import Image, { StaticImageData } from "next/image";
import { mock } from "vitest-mock-extended";
import userEvent from "@testing-library/user-event";

vi.mock("next/image");

describe("Carousel", () => {
    const imagesMock = [
        mock<StaticImageData>({ src: "image1.jpg" }),
        mock<StaticImageData>({ src: "image2.jpg" }),
        mock<StaticImageData>({ src: "image3.jpg" }),
    ];

    const width = 200;
    const height = 100;
    const alt = "Sample image";

    it("should render all images", () => {
        render(
            <Carousel
                images={imagesMock}
                width={width}
                height={height}
                alt={alt}
            />,
        );

        imagesMock.forEach((_, i) => {
            const img = screen.getByTestId(`carouselImage-${i}`);
            expect(img).toBeInTheDocument();
            expect(Image).toHaveBeenCalledWith(
                expect.objectContaining({
                    src: imagesMock[i],
                    alt: `${alt} ${i + 1}`,
                    width,
                    height,
                }),
                undefined,
            );
        });
    });

    it("should render the correct number of navigation circles", () => {
        render(
            <Carousel
                images={imagesMock}
                width={width}
                height={height}
                alt={alt}
            />,
        );

        imagesMock.forEach((_, i) => {
            expect(
                screen.getByTestId(`carouselNavigationCircle-${i}`),
            ).toBeInTheDocument();
        });
    });

    it("should change image when clicking on the carousel", async () => {
        const user = userEvent.setup();
        render(
            <Carousel
                images={imagesMock}
                width={width}
                height={height}
                alt={alt}
            />,
        );

        const container = screen.getByTestId("carouselImageContainer");
        expect(container.style.transform).toBe("translateX(-0%)");

        await user.click(container);
        expect(container.style.transform).toBe("translateX(-100%)");

        await user.click(container);
        expect(container.style.transform).toBe("translateX(-200%)");

        await user.click(container);
        expect(container.style.transform).toBe("translateX(-0%)");
    });

    it("should set the current image when clicking a navigation circle", async () => {
        const user = userEvent.setup();
        render(
            <Carousel
                images={imagesMock}
                width={width}
                height={height}
                alt={alt}
            />,
        );

        const container = screen.getByTestId("carouselImageContainer");

        const secondCircle = screen.getByTestId("carouselNavigationCircle-1");
        await user.click(secondCircle);

        expect(container.style.transform).toBe("translateX(-100%)");
    });

    it("should not render navigation circles when there is only one image", () => {
        render(
            <Carousel
                images={[imagesMock[0]]}
                width={width}
                height={height}
                alt={alt}
            />,
        );
        expect(
            screen.queryByTestId("carouselNavigationCircle-0"),
        ).not.toBeInTheDocument();
    });
});
