import { setWindowInnerWidth } from "@/lib/testUtils/mocks/mockDom";
import "vitest-dom/extend-expect";

vi.mock("next/navigation", () => ({
    useRouter: vi.fn(),
}));

beforeEach(() => {
    setWindowInnerWidth(1920);
});

afterEach(() => {
    vi.useRealTimers();
    vi.restoreAllMocks();
    localStorage.clear();
});
