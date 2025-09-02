import { PrivateUser } from "@/lib/apiClient";
import { Renderable, renderRenderable } from "@/lib/utils/renderable";
import { fetchAuthedUserSession } from "../lib/getLoggedIn";
import SessionProvider from "../contexts/sessionContext";

interface WithOptionalAuthedUserProps {
    user: PrivateUser | null;
}

export default async function WithAuthedUser({
    children,
}: {
    children: Renderable<WithOptionalAuthedUserProps>;
}) {
    const session = await fetchAuthedUserSession();
    const user = session?.user ?? null;

    return (
        <SessionProvider user={user} fetchAttempted>
            {renderRenderable(children, { user })}
        </SessionProvider>
    );
}
