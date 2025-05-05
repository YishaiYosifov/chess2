import { SerializeOptions } from "cookie";

interface ParsedCookie {
    name: string;
    value: string;
    options: SerializeOptions;
}

export function parseSetCookieHeader(header: string): ParsedCookie {
    const parts = header.split(/\s*;\s*/);
    const [nameValue, ...attrPairs] = parts;
    const [name, ...valParts] = nameValue.split("=");
    const value = valParts.join("=");

    const options: SerializeOptions = {};
    for (const attr of attrPairs) {
        const [keyRaw, ...attrValParts] = attr.split("=");
        const key = keyRaw.toLowerCase();
        const val = attrValParts.join("=");

        switch (key) {
            case "expires":
                const date = new Date(val);
                if (!isNaN(date.valueOf())) options.expires = date;
                break;
            case "max-age":
                const n = Number(val);
                if (!isNaN(n)) options.maxAge = n;
                break;
            case "domain":
                options.domain = val;
                break;
            case "path":
                options.path = val;
                break;
            case "httponly":
                options.httpOnly = true;
                break;
            case "secure":
                options.secure = true;
                break;
            case "samesite":
                // Can be Strict, Lax, None (case-insensitive)
                const ss = val.toLowerCase();
                if (ss === "lax" || ss === "strict" || ss === "none") {
                    options.sameSite = ss;
                }
                break;
            case "priority":
                const pr = val.toLowerCase();
                if (pr === "low" || pr === "medium" || pr === "high") {
                    options.priority = pr;
                }
                break;
            default:
                console.warn("Received unknown cookie key:", key);
                break;
        }
    }

    return { name, value, options };
}
