// eslint-disable-next-line @next/next/no-img-element, jsx-a11y/alt-text, @typescript-eslint/no-unused-vars
const Image = vi.fn(({ unoptimized, ...props }) => <img {...props} />);
export default Image;
