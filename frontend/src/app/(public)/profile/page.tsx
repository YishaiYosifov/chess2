import { redirect } from "next/navigation";

import WithAuthedUser from "@/features/auth/components/WithAuthedUser";

export default async function RedirectUserPage() {
    return (
        <WithAuthedUser>
            {({ user }) => {
                redirect(`/profile/${user.userName}`);
            }}
        </WithAuthedUser>
    );
}
