"use client";

import ChallengeDescription from "./ChallengeDescription";
import { ChallengeRequest } from "@/lib/apiClient";
import ChallengeFooter from "./ChallengeFooter";
import ChallengeHeader from "./ChallengeHeader";
import Card from "@/components/ui/Card";

const ChallengeSidebar = ({ challenge }: { challenge: ChallengeRequest }) => {
    return (
        <aside className="flex w-full min-w-xs flex-col gap-3 lg:max-w-sm">
            <Card className="items-center">
                <ChallengeHeader challenge={challenge} />
            </Card>

            <Card className="flex-col items-center justify-center gap-5">
                <ChallengeDescription challenge={challenge} />
            </Card>

            <Card className="items-center">
                <ChallengeFooter challenge={challenge} />
            </Card>
        </aside>
    );
};
export default ChallengeSidebar;
