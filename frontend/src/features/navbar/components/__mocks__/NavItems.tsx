export const UpperNavItems = vi.fn(
    ({ hasAccessCookie }: { hasAccessCookie: boolean }) => (
        <div
            data-testid="upperNavItems"
            data-has-access-cookie={hasAccessCookie}
        >
            UpperNavItems
        </div>
    ),
);
export const LowerNavItems = vi.fn(
    ({ hasAccessCookie }: { hasAccessCookie: boolean }) => (
        <div
            data-testid="lowerNavItems"
            data-has-access-cookie={hasAccessCookie}
        >
            LowerNavItems
        </div>
    ),
);
