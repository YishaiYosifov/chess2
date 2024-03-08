import { PrivateAuthedProfileOut } from "@/client";
import {
    AuthContext,
    AuthContextInterface,
} from "@/components/contexts/AuthContext";
import { useContext } from "react";

/**
 * Get the authed user context, or raise an error if not loaded
 *
 * @returns the auth context object
 */
export function useAuthedContext(): Required<AuthContextInterface> {
    const context = useContext(AuthContext);
    if (!context.hasAuthCookies || context.authedProfile === undefined)
        throw Error("Profile Not Loaded");

    return context as Required<AuthContextInterface>;
}

/**
 * Get the authed user profile, or raise an error if not loaded
 *
 * @returns the authed user profile
 */
export function useAuthedProfile(): PrivateAuthedProfileOut {
    return useAuthedContext().authedProfile;
}
