import { ClipboardIcon } from "@heroicons/react/24/outline";
import InputField from "@/components/ui/InputField";
import { useEffect, useState } from "react";
import Image from "next/image";
import QRCode from "qrcode";

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
export default OpenChallengeDescription;
