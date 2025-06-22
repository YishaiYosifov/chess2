import { useContext } from "react";

import { AuthContextInterface, AuthContext } from "@/contexts/authContext";

/**
 * Get the authed user context, or raise an error if not loaded
 *
 * @returns the auth context object
 */

export function useAuthedContext(): Required<AuthContextInterface> {
    const context = useContext(AuthContext);
    if (!context.user) throw Error("Profile Not Loaded");

    return context as Required<AuthContextInterface>;
}
