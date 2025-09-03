import Card from "@/components/ui/Card";
import TimeControlIcon from "@/features/lobby/Components/TimeControlIcon";
import { TimeControl } from "@/lib/apiClient";
import constants from "@/lib/constants";

const EmptyRatingCard = ({ timeControl }: { timeControl: TimeControl }) => {
    return (
        <Card
            className="min-w-96"
            data-testid={`emptyRatingCard-${timeControl}`}
        >
            <section className="flex w-full justify-between">
                <span className="flex gap-2">
                    {constants.TIME_CONTROL_LABELS[timeControl]}
                    <TimeControlIcon
                        className="h-6 w-6"
                        timeControl={timeControl}
                    />
                </span>
                <span>—</span>
            </section>
            <div className="text-text/50 my-auto py-8 text-center italic">
                Unrated in this time control
            </div>
        </Card>
    );
};
export default EmptyRatingCard;
