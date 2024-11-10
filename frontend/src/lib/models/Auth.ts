export interface AccessToken {
    tokenType?: string;
    accessToken?: string | null;
}

export interface AuthTokens extends AccessToken {
    refreshToken?: string | null;
}
