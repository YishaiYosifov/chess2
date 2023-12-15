import "@testing-library/jest-dom";

import { server } from "@/mocks/server";

jest.mock("next/navigation", () => ({
    useRouter: jest.fn(),
}));

beforeAll(() => server.listen());
afterEach(() => {
    server.resetHandlers();
    jest.clearAllMocks();
});
afterAll(() => server.close());

global.console = { log: jest.fn(), error: jest.fn(), warn: jest.fn() };
