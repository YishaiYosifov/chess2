import {
    BoltIcon,
    ClockIcon,
    BellAlertIcon,
    AcademicCapIcon,
} from "@heroicons/react/24/outline";

import { TimeControl } from "@/lib/apiClient";
import clsx from "clsx";

const TimeControlIcon = ({
    timeControl,
    className,
}: {
    timeControl: TimeControl;
    className?: string;
}) => {
    switch (timeControl) {
        case TimeControl.BULLET:
            return <BoltIcon className={clsx("text-yellow-400", className)} />;
        case TimeControl.BLITZ:
            return (
                <BellAlertIcon className={clsx("text-yellow-600", className)} />
            );
        case TimeControl.RAPID:
            return <ClockIcon className={clsx("text-lime-500", className)} />;
        case TimeControl.CLASSICAL:
            return (
                <AcademicCapIcon
                    className={clsx("text-amber-600", className)}
                />
            );
    }
};
export default TimeControlIcon;
