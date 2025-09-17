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
                <p data-testid="newQuestText">New Quest in {countdown}</p>
            )}
        </CountdownText>
    );
};
export default NewQuestCountdown;
