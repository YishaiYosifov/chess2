import CredentialsProvider from "next-auth/providers/credentials";
import NextAuth, { User } from "next-auth";

import { parseSetCookieHeader } from "@/lib/utils/requestUtils";
import { signin, refresh } from "@/lib/apiClient";
import { cookies } from "next/headers";

export const handler = NextAuth({
    providers: [
        CredentialsProvider({
            credentials: {
                usernameOrEmail: {
                    label: "username or email",
                    type: "text",
                },
                password: {
                    label: "password",
                    type: "password",
                },
            },
            async authorize(credentials) {
                if (!credentials) return null;

                const { error, response, data } = await signin({
                    body: {
                        usernameOrEmail: credentials.usernameOrEmail,
                        password: credentials.password,
                    },
                });

                if (error) throw error;
                if (!data) return null;

                const cookieStore = await cookies();
                for (const cookie of response.headers.getSetCookie()) {
                    const { name, value, options } =
                        parseSetCookieHeader(cookie);
                    cookieStore.set(name, value, options);
                }

                const user: User = {
                    id: data.user.id,
                    tokens: data.authTokens,
                };
                return user;
            },
        }),
    ],
    secret: process.env.AUTH_SECRET,
    // our custom login page
    pages: {
        signIn: "/login",
    },
    callbacks: {
        async jwt({ token, user }) {
            if (user) {
                token.accessToken = user.tokens.accessToken;
                token.refreshToken = user.tokens.refreshToken;
                token.accessTokenExpiresInSeconds =
                    user.tokens.accessTokenExpiresInSeconds;
            }

            // if our access token has not expired yet, return all information except the refresh token
            if (Date.now() < Number(token.accessTokenExpires)) return token;

            const { data, error } = await refresh();
            if (error || !data) {
                console.error("Could not refresh token:", error);
                return token;
            }

            token.accessToken = data.authTokens.accessToken;
            token.accessTokenExpiresInSeconds =
                data.authTokens.accessTokenExpiresInSeconds;
            token.refreshToken = data.authTokens.refreshToken;
            return token;
        },

        async session({ session, token }) {
            console.log("session => ", session);

            return {
                ...session,
                accessTokenExpiresInSeconds: token.accessTokenExpiresInSeconds,
            };
        },
    },
    debug: process.env.NODE_ENV === "development",
});
export { handler as GET, handler as POST };
