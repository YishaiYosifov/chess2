import NextAuth, { Account, NextAuthOptions, User } from "next-auth";
import CredentialsProvider from "next-auth/providers/credentials";
import type { JWT } from "next-auth/jwt";
import { cookies } from "next/headers";

import { AuthedUser, refresh, signin } from "./apiClient";

async function refreshAccessToken(token: JWT) {
    // this is our refresh token method
    console.log("Now refreshing the expired token...");
    try {
        const { data, response, error } = await refresh();
        if (!response.ok || !data) {
            console.log("The token could not be refreshed!");
            throw error;
        }

        console.log("The token has been refreshed successfully.");

        // get some data from the new access token such as exp (expiration time)
        const decodedAccessToken = JSON.parse(
            Buffer.from(data.accessToken.split(".")[1], "base64").toString(),
        );

        return {
            ...token,
            accessToken: data.authTokens.accessToken,
            refreshToken: data.authTokens.refreshToken ?? token.refreshToken,
            accessTokenExpires: decodedAccessToken["exp"] * 1000,
            error: "",
        };
    } catch (error) {
        console.log(error);

        // return an error if somethings goes wrong
        return {
            ...token,
            error: "RefreshAccessTokenError", // attention!
        };
    }
}

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

                const user: User = { user: data.user, tokens: data.authTokens };
                return;
            },
        }),
    ],
    // this is required
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
                    id: token.id as string,
                    email: token.email as string,
                    accessToken: token.accessToken as string,
                    accessTokenExpires: token.accessTokenExpires as number,
                    role: token.role as string,
                },
                error: token.error,
            };
        },
    },
    debug: process.env.NODE_ENV === "development",
};

export const { auth, handlers } = NextAuth(config);
