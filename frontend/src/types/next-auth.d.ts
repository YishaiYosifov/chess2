import type { Tokens } from "@/lib/apiClient/definition/types.gen";
import { DefaultSession } from "next-auth";

declare module "next-auth" {
    interface User {
        id?: number;
        tokens: Tokens;
    }

    interface Session {
        user: User & DefaultSession["user"];
    }
}

declare module "next-auth/jwt" {
    interface JWT {
        accessToken: string;
        accessTokenExpiresInSeconds: number;
        refreshToken: string;
    }
}
