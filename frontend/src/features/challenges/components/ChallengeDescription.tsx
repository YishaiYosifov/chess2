import { ClipboardIcon } from "@heroicons/react/24/outline";
import { useEffect, useState } from "react";
import Image from "next/image";
import QRCode from "qrcode";

import ProfilePicture from "@/features/profile/components/ProfilePicture";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import { ChallengeRequest, MinimalProfile } from "@/lib/apiClient";
import InputField from "@/components/ui/InputField";

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
}) => {
    return (
        <>
            <p className="text-2xl">Challenged By</p>
            <ProfilePicture
                userId={requester.userId}
                width={200}
                height={200}
            />
            <p className="w-full truncate text-center text-xl">
                {requester.userName}
            </p>
        </>
    );
};

const DirectChallengeDescription = ({
    recipient,
}: {
    recipient: MinimalProfile;
}) => {
    return (
        <>
            <p className="text-2xl">Waiting For</p>
            <ProfilePicture
                userId={recipient.userId}
                className="animate-subtle-ping"
                width={200}
                height={200}
            />
            <p className="w-full truncate text-center text-xl">
                {recipient.userName}
            </p>
        </>
    );
};

const OpenChallengeDescription = () => {
    const [currentUrl, setCurrentUrl] = useState<string>();
    const [qrCodeB64, setQrCodeB64] = useState<string>();

    async function copyChallengeLink() {
        if (currentUrl) navigator.clipboard.writeText(currentUrl);
    }

    useEffect(() => {
        if (typeof window === "undefined") return;

        setCurrentUrl(window.location.href);
        QRCode.toDataURL(window.location.href).then(setQrCodeB64);
    }, []);

    return (
        <div className="flex w-full flex-col items-center gap-3">
            <p className="text-text/70 text-lg">Invite someone to play via:</p>

            <InputField
                defaultValue={currentUrl}
                readOnly
                icon={<ClipboardIcon onClick={copyChallengeLink} />}
                className="flex-1"
            />
            {qrCodeB64 && (
                <div className="bg-background flex w-full justify-center rounded-md border border-white/20 p-3">
                    <Image
                        src={qrCodeB64}
                        alt="challenge qr code"
                        width="150"
                        height="150"
                    />
                </div>
            )}
        </div>
    );
};
