import { Renderable, renderRenderable } from "@/lib/utils/renderable";
import SessionProvider from "../contexts/sessionContext";
import { fetchUserSession } from "../lib/getLoggedIn";
import { SessionUser } from "@/lib/apiClient";

interface WithOptionalSessionProps {
    user: SessionUser | null;
    accessToken: string | null;
}

export default async function WithOptionalSession({
    children,
}: {
    children: Renderable<WithOptionalSessionProps>;
}) {
    const session = await fetchUserSession();
    const user = session?.user ?? null;
    const accessToken = session?.accessToken ?? null;

    return (
        <SessionProvider user={user} fetchAttempted>
            {renderRenderable(children, { user, accessToken })}
        </SessionProvider>
    );
}
