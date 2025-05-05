import "next-auth/jwt";
import "next-auth";

import type { PrivateUser, Tokens } from "@/lib/apiClient/definition/types.gen";

declare module "next-auth" {
    interface User {
        id?: number;
        data: PrivateUser;
        tokens: Tokens;
    }

    interface Session {
        user: PrivateUser;
        accessTokenExpiresTimestamp: number;
        userQuerySuccessful: boolean;
    }
}

declare module "next-auth/jwt" {
    interface JWT {
        accessToken: string;
        accessTokenExpiresTimestamp: number;
        refreshToken: string;
    }
}
