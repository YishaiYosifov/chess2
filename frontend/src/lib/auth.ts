import NextAuth, { NextAuthOptions, User } from "next-auth";
import CredentialsProvider from "next-auth/providers/credentials";

import { refresh, signin } from "./apiClient";

export const config: NextAuthOptions = {
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

                const { error, data } = await signin({
                    body: {
                        usernameOrEmail: credentials.usernameOrEmail,
                        password: credentials.password,
                    },
                });

                if (error) throw error;
                if (!data) return null;

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
                user: {
                    ...session.user,
                    tokens: token,
                },
            };
        },
    },
    debug: process.env.NODE_ENV === "development",
};

export const { auth, handlers } = NextAuth(config);
