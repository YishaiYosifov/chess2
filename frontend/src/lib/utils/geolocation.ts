import timezones from "@/json/timezones.json";

interface Timezone {
    countries?: string[];
    alias?: string;
}

const typedTimezones = new Map<string, Timezone>(Object.entries(timezones));

export function getCountryFromUserTimezone(): string | null {
    const userTimezone = Intl.DateTimeFormat().resolvedOptions().timeZone;
    return getCountryFromTimezone(userTimezone);
}

function getCountryFromTimezone(timezone: string): string | null {
    const timezoneInfo = typedTimezones.get(timezone);

    if (!timezoneInfo) return null;
    if (timezoneInfo.countries) return timezoneInfo.countries[0] ?? null;
    if (timezoneInfo.alias) return getCountryFromTimezone(timezoneInfo.alias);
    return null;
}
