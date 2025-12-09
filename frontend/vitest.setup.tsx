import { setWindowInnerWidth } from "@/lib/testUtils/mocks/mockDom";
import { loadEnvFile } from "node:process";
import "vitest-dom/extend-expect";

import { mockAudio } from "@/lib/testUtils/mocks/mockAudio";

vi.mock("next/navigation");
vi.mock("@microsoft/signalr");

loadEnvFile();

beforeEach(() => {
    setWindowInnerWidth(1920);
    mockAudio();
});

afterEach(() => {
    vi.useRealTimers();
    vi.resetAllMocks();
    localStorage.clear();
});
