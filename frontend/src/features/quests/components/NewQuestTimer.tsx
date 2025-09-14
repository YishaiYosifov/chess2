import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";

const NewQuestTimer = () => {
    const [newQuestIn, setNewQuestIn] = useState("");
    const lastDayRef = useRef(new Date().getUTCDate());
    const router = useRouter();

    function timeUntilMidnightUTC() {
        const now = new Date();
        const midnightUTC = new Date(
            Date.UTC(
                now.getUTCFullYear(),
                now.getUTCMonth(),
                now.getUTCDate() + 1,
            ),
        );

        const diffSeconds = Math.floor(
            (midnightUTC.valueOf() - now.valueOf()) / 1000,
        );

        const hours = String(Math.floor(diffSeconds / 3600)).padStart(2, "0");
        const minutes = String(Math.floor((diffSeconds % 3600) / 60)).padStart(
            2,
            "0",
        );
        const seconds = String(diffSeconds % 60).padStart(2, "0");

        return `${hours}:${minutes}:${seconds}`;
    }

    useEffect(() => {
        setNewQuestIn(timeUntilMidnightUTC());

        const interval = setInterval(() => {
            setNewQuestIn(timeUntilMidnightUTC());

            const currentDay = new Date().getUTCDate();
            if (currentDay !== lastDayRef.current) {
                lastDayRef.current = currentDay;
                router.refresh();
            }
        }, 1000);

        return () => clearInterval(interval);
    }, [setNewQuestIn, router]);

    return (
        <p className="text-text/70" data-testid="newQuestText">
            New quest in {newQuestIn}
        </p>
    );
};
export default NewQuestTimer;
