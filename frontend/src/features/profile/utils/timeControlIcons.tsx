import {
    BoltIcon,
    ClockIcon,
    BellAlertIcon,
    AcademicCapIcon,
} from "@heroicons/react/24/outline";

import { TimeControl } from "@/lib/apiClient";

export const getTimeControlIcon = (control: TimeControl) => {
    switch (control) {
        case TimeControl.BULLET:
            return <BoltIcon className="h-6 w-6 text-yellow-400" />;
        case TimeControl.BLITZ:
            return <BellAlertIcon className="h-6 w-6 text-yellow-600" />;
        case TimeControl.RAPID:
            return <ClockIcon className="h-6 w-6 text-green-400" />;
        case TimeControl.CLASSICAL:
            return <AcademicCapIcon className="h-6 w-6 text-amber-600" />;
    }
};
