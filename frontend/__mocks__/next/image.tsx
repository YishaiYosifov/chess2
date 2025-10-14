const Image = vi.fn(({ unoptimized, priority, ...props }) => (
    // eslint-disable-next-line @next/next/no-img-element, jsx-a11y/alt-text
    <img
        unoptimized={unoptimized?.toString()}
        priority={priority?.toString()}
        {...props}
    />
));
export default Image;
