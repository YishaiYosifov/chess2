import { ArrowPathIcon } from "@heroicons/react/24/solid";
import Button from "../helpers/Button";

const SeekingOverlay = ({ onClick }: { onClick: () => void }) => {
    return (
        <section
            className="absolute inset-0 z-20 flex flex-col items-center justify-center rounded-sm"
            data-testid="seekingOverlay"
        >
            <ArrowPathIcon
                className="w-10 animate-spin"
                data-testid="seekingSpinner"
            />
            <div className="flex flex-col">
                <span>Searching For a Match... </span>
                <Button onClick={onClick} data-testid="cancelSeekButton">
                    Cancel
                </Button>
            </div>
        </section>
    );
};
export default SeekingOverlay;
