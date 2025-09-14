import { MinimalProfile } from "@/lib/apiClient";
import MinimalProfileView from "./MinimalProfileView";
import Button from "@/components/ui/Button";
import { useState } from "react";

const MinimalProfileAction = ({
    index,
    profile,
    activate,
    deactivate,
    buttonIcon,
    buttonLabel,
}: {
    index?: number;
    profile: MinimalProfile;
    activate: () => Promise<unknown>;
    deactivate: () => Promise<unknown>;
    buttonLabel: (isActive: boolean) => string;
    buttonIcon?: (isActive: boolean) => React.ReactNode;
}) => {
    const [isLoading, setIsLoading] = useState(false);
    const [isActive, setActive] = useState(true);

    const handleToggle = async () => {
        if (isLoading) return;
        setIsLoading(true);

        try {
            if (isActive) await deactivate();
            else await activate();
            setActive((prev) => !prev);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <MinimalProfileView profile={profile} index={index}>
            <Button
                className="ml-auto flex items-center gap-1"
                onClick={handleToggle}
                data-testid="minimalProfileActionRowToggle"
            >
                {buttonIcon?.(isActive)}
                {buttonLabel(isActive)}
            </Button>
        </MinimalProfileView>
    );
};
export default MinimalProfileAction;
