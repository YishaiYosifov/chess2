import { fetchProfile, fetchRatings, fetchGames } from "../profileCrud";
import { getResource } from "@/lib/utils/fetchUtils";

vi.mock("@/lib/utils/fetchUtils");

function expectGetResourceCall(expectedPath: string) {
    expect(getResource).toHaveBeenCalledWith(expectedPath);
}

describe("fetchProfile", () => {
    it("should format the username in the url", async () => {
        await fetchProfile("test-user");
        expectGetResourceCall("/profile/test-user/info");
    });
});

describe("fetchRatings", () => {
    it("should format the username and the date without time in the url", async () => {
        const datetime = new Date("1/1/23 12:04:12");
        const date = "2023-01-01";

        await fetchRatings("test-user", datetime);
        expectGetResourceCall(
            `/profile/test-user/rating-history?since=${date}`
        );
    });
});

describe("fetchGames", () => {
    it("should format the username in the url", async () => {
        await fetchGames("test-user");
        expectGetResourceCall("/profile/test-user/games");
    });
});
