export const redirect = vi.fn((url: string) => {
    throw new Error(`redirect ${url}`);
});

export const notFound = vi.fn(() => {
    throw new Error("notFound");
});

export const useRouter = vi.fn();
export const usePathname = vi.fn();
