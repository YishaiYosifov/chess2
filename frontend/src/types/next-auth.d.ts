import type { Tokens } from "@/lib/apiClient/definition/types.gen";

declare module "next-auth" {
    interface User {
        id?: number;
        tokens: Tokens;
    }

    interface Session {
        userId?: int;
        accessTokenExpiresTimestamp: number;
    }
}

declare module "next-auth/jwt" {
    interface JWT {
        accessToken: string;
        accessTokenExpiresTimestamp: number;
        refreshToken: string;
    }
}
