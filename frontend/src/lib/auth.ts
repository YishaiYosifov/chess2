import CredentialsProvider from "next-auth/providers/credentials";
import NextAuth, { User, Session } from "next-auth";

import { signin, refresh, getAuthedUser } from "@/lib/apiClient";
import { parseSetCookieHeader } from "@/lib/utils/requestUtils";
import { cookies } from "next/headers";

async function copyCookiesFromResponse(response: Response): Promise<void> {
    const cookieStore = await cookies();
    for (const cookie of response.headers.getSetCookie()) {
        const { name, value, options } = parseSetCookieHeader(cookie);
        cookieStore.set(name, value, options);
    }
}

export const { handlers, signOut, auth } = NextAuth((req) => ({
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
                //await copyCookiesFromResponse(response);

                const user: User = {
                    id: data.user.userId,
                    data: data.user,
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
                token.accessTokenExpiresTimestamp =
                    user.tokens.accessTokenExpiresTimestamp;
            }

            // if our access token has not expired yet, return all information except the refresh token
            if (Date.now() / 1000 < token.accessTokenExpiresTimestamp)
                return token;

            const { data, error } = await refresh({
                headers: {
                    Cookie: `refreshToken=${token.refreshToken}`,
                },
            });
            if (error || !data) {
                console.error("Could not refresh token:", error);
                return token;
            }

            token.accessToken = data.authTokens.accessToken;
            token.accessTokenExpiresTimestamp =
                data.authTokens.accessTokenExpiresTimestamp;
            token.refreshToken = data.authTokens.refreshToken;
            return token;
        },

        async session({ session, token }) {
            const { data: fetchedUser, error } = await getAuthedUser({
                headers: { Cookie: `accessToken=${token.accessToken}` },
            });

            const newSession: Session = {
                ...session,
                user: fetchedUser ?? session.user,
                accessTokenExpiresTimestamp: token.accessTokenExpiresTimestamp,
                userQuerySuccessful: !error && fetchedUser !== undefined,
            };
            return newSession;
        },
    },
    debug: process.env.NODE_ENV === "development",
}));
