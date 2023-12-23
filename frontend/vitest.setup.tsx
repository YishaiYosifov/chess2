import { server } from "@/mocks/server";
import "vitest-dom/extend-expect";

vi.mock("next/navigation", () => ({
    useRouter: vi.fn(),
}));

vi.mock("next/image", () =>
    // eslint-disable-next-line @next/next/no-img-element, jsx-a11y/alt-text
    ({ default: vi.fn((props) => <img {...props} />) })
);

beforeAll(() => server.listen());
afterEach(() => {
    server.resetHandlers();
    vi.clearAllMocks();
});
afterAll(() => server.close());

global.console = {
    ...global.console,
    error: vi.fn(),
    warn: vi.fn(),
    log: console.log,
};
