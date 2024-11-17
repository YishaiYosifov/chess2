import { getCountryFromUserTimezone } from "../geolocation";

describe("getCountryFromUserTimezone", () => {
    function spyOnTimezone(timezone: string): void {
        vi.spyOn(globalThis.Intl, "DateTimeFormat").mockImplementation(() => ({
            resolvedOptions: () => ({
                timeZone: timezone,
                calendar: "gregory",
                locale: "en-US",
                numberingSystem: "latin",
            }),
            format: () => "1/1/2022",
            formatRange: () => "1/1/2022 – 1/1/2023",
            formatRangeToParts: () => [],
            formatToParts: () => [],
        }));
    }

    it("should return the correct country for the user's timezone", () => {
        spyOnTimezone("Asia/Jerusalem");
        const country = getCountryFromUserTimezone();
        expect(country).toBe("IL");
    });

    it("should return null if user's timezone is not found", () => {
        spyOnTimezone("Invalid/Timezone");
        const country = getCountryFromUserTimezone();
        expect(country).toBeNull();
    });

    it("should handle aliased timezones", () => {
        spyOnTimezone("America/Yellowknife");
        const country = getCountryFromUserTimezone();
        expect(country).toBe("CA");
    });
});
