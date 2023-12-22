import "@testing-library/jest-dom";

import { server } from "@/mocks/server";

jest.mock("next/navigation", () => ({
    useRouter: jest.fn(),
}));

jest.mock("next/image", () =>
    // eslint-disable-next-line @next/next/no-img-element, jsx-a11y/alt-text
    jest.fn((props) => <img {...props} />)
);

beforeAll(() => server.listen());
afterEach(() => {
    server.resetHandlers();
    jest.clearAllMocks();
});
afterAll(() => server.close());

global.console = { error: jest.fn(), warn: jest.fn(), log: console.log };
