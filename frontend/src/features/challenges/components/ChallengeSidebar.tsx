"use client";

import { ClipboardIcon } from "@heroicons/react/24/outline";
import { useEffect, useState } from "react";
import Link from "next/link";
import QRCode from "qrcode";

import CountdownText from "@/components/CountdownText";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import InputField from "@/components/ui/InputField";
import TimeControlIcon from "@/features/lobby/Components/TimeControlIcon";
import { ChallengeRequest, PoolType } from "@/lib/apiClient";
import constants from "@/lib/constants";
import Image from "next/image";

const ChallengeSidebar = ({ challenge }: { challenge: ChallengeRequest }) => {
    const [isExpired, setIsExpired] = useState(false);
    const [currentUrl, setCurrentUrl] = useState<string>();
    const [qrCodeB64, setQrCodeB64] = useState<string>();

    const isRated = challenge.pool.poolType == PoolType.RATED;
    const expiresAt = new Date(challenge.expiresAt + "Z");

    async function copyChallengeLink() {
        if (currentUrl) navigator.clipboard.writeText(currentUrl);
    }

    useEffect(() => {
        if (typeof window === "undefined") return;

        setCurrentUrl(window.location.href);
        QRCode.toDataURL(window.location.href).then(setQrCodeB64);
    }, []);

    return (
        <Card
            className="h-full w-full max-w-xl min-w-xs items-center gap-10 self-center pt-10
                lg:max-w-lg"
        >
            <h1 className="text-5xl">Challenge</h1>

            <div className="flex flex-col items-center gap-1">
                <div className="flex items-center gap-3">
                    <TimeControlIcon
                        timeControl={challenge.timeControl}
                        className="h-10 w-10"
                    />
                    <p className="text-3xl font-semibold">
                        {challenge.pool.timeControl.baseSeconds / 60}+
                        {challenge.pool.timeControl.incrementSeconds}
                    </p>
                </div>
                <p className="text-text/70 text-2xl">
                    {isRated ? "Rated" : "Casual"}
                </p>
            </div>

            <div className="flex w-full flex-col items-center gap-3">
                <p className="text-text/70">Invite someone to play via:</p>

                <InputField
                    defaultValue={currentUrl}
                    readOnly
                    icon={<ClipboardIcon onClick={copyChallengeLink} />}
                />

                {qrCodeB64 && (
                    <Image
                        src={qrCodeB64}
                        alt="challenge qr code"
                        width={200}
                        height={200}
                    />
                )}
            </div>

            <div className="mt-auto flex w-full flex-col items-center gap-2">
                {isExpired ? (
                    <>
                        <p className="text-error text-2xl">
                            Challenge Expired!
                        </p>
                        <Link href={constants.PATHS.PLAY} className="w-full">
                            <Button className="w-full">New Opponent</Button>
                        </Link>
                    </>
                ) : (
                    <>
                        <p className="text-2xl">Waiting for opponent...</p>
                        <Button className="w-full">Cancel</Button>
                        <CountdownText
                            getTimeUntil={() => expiresAt}
                            onDateReached={() => setIsExpired(true)}
                        >
                            {({ countdown }) => <p>Expires in {countdown}</p>}
                        </CountdownText>
                    </>
                )}
            </div>
        </Card>
    );
};
export default ChallengeSidebar;
