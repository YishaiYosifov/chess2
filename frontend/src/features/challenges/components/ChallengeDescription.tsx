import { ClipboardIcon } from "@heroicons/react/24/outline";
import { useEffect, useState } from "react";
import Image from "next/image";
import QRCode from "qrcode";

import ProfilePicture from "@/features/profile/components/ProfilePicture";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import { ChallengeRequest, MinimalProfile } from "@/lib/apiClient";
import InputField from "@/components/ui/InputField";
import UserProfileTooltip from "@/features/profile/components/UserProfileTooltip";

const ChallengeDescription = ({
    challenge,
}: {
    challenge: ChallengeRequest;
}) => {
    const user = useSessionUser();
    const isRequester = challenge.requester.userId === user?.userId;

    if (!isRequester)
        return (
            <ChallengeRecipientDescription requester={challenge.requester} />
        );

    return challenge.recipient ? (
        <DirectChallengeDescription recipient={challenge.recipient} />
    ) : (
        <OpenChallengeDescription />
    );
};
export default ChallengeDescription;
const ChallengeRecipientDescription = ({
    requester,
}: {
    requester: MinimalProfile;
}) => (
    <>
        <p
            data-testid="challengeRecipientDescriptionTitle"
            className="text-2xl"
        >
            Challenged By
        </p>
        <ProfilePicture
            data-testid="challengeRecipientDescriptionProfilePicture"
            userId={requester.userId}
            size={200}
        />
        <UserProfileTooltip username={requester.userName}>
            <p
                data-testid="challengeRecipientDescriptionUserName"
                className="text-lg"
            >
                {requester.userName}
            </p>
        </UserProfileTooltip>
    </>
);

const DirectChallengeDescription = ({
    recipient,
}: {
    recipient: MinimalProfile;
}) => (
    <>
        <p data-testid="directChallengeDescriptionTitle" className="text-2xl">
            Waiting For
        </p>
        <ProfilePicture
            data-testid="directChallengeDescriptionProfilePicture"
            userId={recipient.userId}
            className="animate-subtle-ping"
            size={200}
        />
        <UserProfileTooltip username={recipient.userName}>
            <p
                data-testid="directChallengeDescriptionUserName"
                className="text-lg"
            >
                {recipient.userName}
            </p>
        </UserProfileTooltip>
    </>
);

const OpenChallengeDescription = () => {
    const [currentUrl, setCurrentUrl] = useState<string>();
    const [qrCodeB64, setQrCodeB64] = useState<string>();

    async function copyChallengeLink() {
        if (currentUrl) await navigator.clipboard.writeText(currentUrl);
    }

    useEffect(() => {
        if (typeof window === "undefined") return;

        setCurrentUrl(window.location.href);
        QRCode.toDataURL(window.location.href).then(setQrCodeB64);
    }, []);

    return (
        <div
            data-testid="openChallengeDescriptionContainer"
            className="flex w-full flex-col items-center gap-3"
        >
            <p className="text-text/70 text-lg">Invite someone to play via:</p>

            <InputField
                data-testid="openChallengeDescriptionInput"
                defaultValue={currentUrl}
                readOnly
                icon={
                    <ClipboardIcon
                        onClick={copyChallengeLink}
                        data-testid="openChallengeDescriptionCopy"
                    />
                }
                className="flex-1"
            />
            {qrCodeB64 && (
                <div className="bg-background flex w-full justify-center rounded-md border border-white/20 p-3">
                    <Image
                        src={qrCodeB64}
                        data-testid="openChallengeDescriptionQRCode"
                        alt="challenge qr code"
                        width={150}
                        height={150}
                    />
                </div>
            )}
        </div>
    );
};
