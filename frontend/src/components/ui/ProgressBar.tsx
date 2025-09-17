const ProgressBar = ({ percent }: { percent: number }) => {
    return (
        <div
            className="bg-primary h-4 flex-1 overflow-hidden rounded-full"
            data-testid="progressBar"
        >
            <div
                className="bg-secondary h-4 rounded-full"
                style={{ width: `${percent}%` }}
                data-testid="progressBarFill"
            />
        </div>
    );
};
export default ProgressBar;
