import type { AuthedUser, Tokens } from "@/lib/apiClient/definition/types.gen";
import { DefaultSession } from "next-auth";

declare module "next-auth" {
    interface User {
        user: AuthedUser;
        tokens: Tokens;
    }

    interface Session {
        user: User & DefaultSession["user"];
        expires: string;
        error: string;
    }
}

declare module "next-auth/jwt" {
    interface JWT {
        accessToken: string;
        accessTokenExpiresInSeconds: number;
        refreshToken: string;
    }
}
