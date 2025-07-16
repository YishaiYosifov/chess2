import Card from "@/components/ui/Card";
import { TimeControl } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { getTimeControlIcon } from "../utils/timeControlIcons";

const EmptyRatingCard = ({ timeControl }: { timeControl: TimeControl }) => {
    return (
        <Card className="min-w-96 flex-col gap-3">
            <section className="flex w-full justify-between">
                <span className="flex gap-2">
                    {constants.TIME_CONTROL_LABELS[timeControl]}
                    {getTimeControlIcon(timeControl)}
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
