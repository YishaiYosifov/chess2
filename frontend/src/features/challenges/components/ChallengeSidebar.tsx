"use client";

import { ChallengeRequest } from "@/lib/apiClient";
import ChallengeFooter from "./ChallengeFooter";
import ChallengeHeader from "./ChallengeHeader";
import Card from "@/components/ui/Card";
import ChallengeStoreContext from "../contexts/challengeContext";
import useConst from "@/hooks/useConst";
import { createChallengeStore } from "../stores/challengeStore";
import ChallengeDescription from "./challengeDescriptions/ChallengeDescription";

const ChallengeSidebar = ({ challenge }: { challenge: ChallengeRequest }) => {
    const challengeStore = useConst(() => createChallengeStore({ challenge }));
    return (
        <ChallengeStoreContext.Provider value={challengeStore}>
            <aside className="flex w-full min-w-xs flex-col gap-3 lg:max-w-sm">
                <ChallengeHeader challenge={challenge} />
                <ChallengeDescription />

                <Card className="items-center">
                    <ChallengeFooter />
                </Card>
            </aside>
        </ChallengeStoreContext.Provider>
    );
};
export default ChallengeSidebar;
