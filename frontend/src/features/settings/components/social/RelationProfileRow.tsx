import { useState } from "react";
import Link from "next/link";
import clsx from "clsx";

import Button from "@/components/ui/Button";
import ProfilePicture from "@/features/profile/components/ProfilePicture";
import { MinimalProfile } from "@/lib/apiClient";

export const RelationProfileRow = ({
    index,
    profile,
    activate,
    deactivate,
    buttonIcon,
    buttonLabel,
}: {
    index: number;
    profile: MinimalProfile;
    activate: () => Promise<void>;
    deactivate: () => Promise<void>;
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
        <div
            className={clsx(
                "flex w-full items-center justify-between gap-3 rounded-md p-3",
                index % 2 === 0 ? "bg-white/5" : "bg-white/15",
            )}
        >
            <Link
                href={`/profile/${profile.userName}`}
                data-testid="relationProfileRowLink"
            >
                <div className="flex items-center gap-3">
                    <ProfilePicture
                        userId={profile.userId}
                        width={80}
                        height={80}
                    />
                    <p
                        className="text-lg"
                        data-testid="relationProfileRowUsername"
                    >
                        {profile.userName}
                    </p>
                </div>
            </Link>

            <Button
                className="flex items-center gap-1"
                onClick={handleToggle}
                data-testid="relationProfileRowToggle"
            >
                {buttonIcon?.(isActive)}
                {buttonLabel(isActive)}
            </Button>
        </div>
    );
};
