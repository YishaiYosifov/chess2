import { UserIcon, GlobeAltIcon } from "@heroicons/react/24/outline";

import ProfileSettings from "@/features/settings/components/ProfileSettings";
import Card from "@/components/ui/Card";
import withAuthedUser from "@/features/auth/hocs/withAuthedUser";

function Page() {
    return (
        <div className="flex w-full justify-center gap-5 p-5">
            <Card className="w-full max-w-60 gap-0 p-0">
                <button className="hover:bg-primary flex cursor-pointer items-center gap-2 p-5 text-lg transition">
                    <UserIcon className="h-8 w-8" />
                    Profile
                </button>

                <button className="hover:bg-primary flex cursor-pointer items-center gap-2 p-5 text-lg transition">
                    <GlobeAltIcon className="h-8 w-8" />
                    Social
                </button>
            </Card>

            <section className="flex w-full max-w-3xl flex-col gap-5">
                <ProfileSettings />
            </section>
        </div>
    );
}
export default withAuthedUser(Page);
