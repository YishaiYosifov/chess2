import { headers } from "next/headers";

import RefreshRedirect from "@/features/auth/components/RefreshRedirect";
import constants from "@/lib/constants";

const page = async () => {
    const headerStore = await headers();
    let redirectTo =
        headerStore.get(constants.HEADERS.REDIRECT_AFTER_AUTH) ?? "/";
    if (redirectTo === constants.PATHS.REFRESH) redirectTo = "/";

    return <RefreshRedirect redirectTo={redirectTo} />;
};
export default page;
