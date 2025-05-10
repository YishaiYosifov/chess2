import { headers } from "next/headers";

import Refresh from "@/components/Refresh";
import constants from "@/lib/constants";

const page = async () => {
    const headerStore = await headers();
    let redirectTo = headerStore.get(constants.HEADERS.REFRESH_REDIRECT) ?? "/";

    // doesn't really matter because after the first refresh the header will be gone
    // but just being safe :)
    if (redirectTo === constants.PATHS.REFRESH) redirectTo = "/";

    return <Refresh redirectTo={redirectTo} />;
};
export default page;
