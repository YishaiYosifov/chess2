import CountdownText from "@/components/CountdownText";
import { useRouter } from "next/navigation";

const NewQuestCountdown = () => {
    const router = useRouter();

    return (
        <CountdownText
            getTimeUntil={() => {
                const now = new Date();
                return new Date(
                    Date.UTC(
                        now.getUTCFullYear(),
                        now.getUTCMonth(),
                        now.getUTCDate() + 1,
                    ),
                );
            }}
            onDateReached={() => router.refresh()}
        >
            {({ countdown }) => (
                <p className="text-text/70" data-testid="newQuestText">
                    New quest in {countdown}
                </p>
            )}
        </CountdownText>
    );
};
export default NewQuestCountdown;
