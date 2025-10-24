import Image, { StaticImageData } from "next/image";
import { useState } from "react";
import clsx from "clsx";

const Carousel = ({
    images,
    width,
    height,
    alt,
}: {
    images: StaticImageData[];
    width: number;
    height: number;
    alt: string;
}) => {
    const [current, setCurrent] = useState(0);

    const next = () => setCurrent((prev) => (prev + 1) % images.length);

    return (
        <div
            className={clsx(
                "relative mx-auto overflow-hidden rounded-md shadow-lg",
                images.length > 1 && "cursor-pointer",
            )}
            style={{ width, height }}
            onClick={next}
            data-testid="carousel"
        >
            <div
                className="flex transition-transform duration-200 ease-in-out"
                style={{ transform: `translateX(-${current * 100}%)` }}
                data-testid="carouselImageContainer"
            >
                {images.map((src, i) => (
                    <Image
                        key={i}
                        src={src}
                        width={width}
                        height={height}
                        alt={`${alt} ${i + 1}`}
                        className="w-full flex-shrink-0 object-cover"
                        data-testid={`carouselImage-${i}`}
                    />
                ))}
            </div>

            {images.length > 1 && (
                <div className="absolute bottom-3 left-1/2 flex -translate-x-1/2 gap-2">
                    {images.map((_, i) => (
                        <button
                            key={i}
                            onClick={(e) => {
                                e.stopPropagation();
                                setCurrent(i);
                            }}
                            className={clsx(
                                "h-4 w-4 cursor-pointer rounded-full transition-all",
                                current === i
                                    ? "bg-primary scale-110"
                                    : "bg-primary/50",
                            )}
                            data-testid={`carouselNavigationCircle-${i}`}
                        />
                    ))}
                </div>
            )}
        </div>
    );
};
export default Carousel;
