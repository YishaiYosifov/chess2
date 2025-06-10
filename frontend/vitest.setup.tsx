import { setWindowInnerWidth } from "@/lib/testUtils/mocks/mockWindow";
import "vitest-dom/extend-expect";

vi.mock("next/navigation", () => ({
    useRouter: vi.fn(),
}));

vi.mock("next/image", () =>
    // eslint-disable-next-line @next/next/no-img-element, jsx-a11y/alt-text
    ({ default: vi.fn((props) => <img {...props} />) }),
);

beforeEach(() => {
    setWindowInnerWidth(1920);
});

afterEach(() => {
    vi.useRealTimers();
    vi.restoreAllMocks();
    localStorage.clear();
});
