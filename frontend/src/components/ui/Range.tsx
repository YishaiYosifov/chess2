import { twMerge } from "tailwind-merge";

const Range = ({
    className,
    ...inputProps
}: React.InputHTMLAttributes<HTMLInputElement>) => {
    return (
        <input
            type="range"
            className={twMerge(
                "bg-primary accent-secondary h-3 w-full cursor-pointer appearance-none rounded-lg",
                className,
            )}
            {...inputProps}
        />
    );
};
export default Range;
