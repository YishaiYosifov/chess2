import { PolymorphicProps } from "@/types/polymorphicProps";
import clsx from "clsx";
import { twMerge } from "tailwind-merge";

interface InputFieldOwnProps {
    icon?: React.ReactNode;
}

type TextFieldProps<C extends React.ElementType> = PolymorphicProps<
    C,
    InputFieldOwnProps
>;

const InputField = <C extends React.ElementType = "input">({
    as,
    className,
    icon,
    ...props
}: TextFieldProps<C>) => {
    const Component = as || "input";
    return (
        <div className="flex w-full">
            <Component
                className={twMerge(
                    clsx(
                        `bg-background/50 autofill:bg-background/50 text-text w-full border
                        border-white/20 p-1 disabled:cursor-not-allowed disabled:opacity-50`,
                        icon && "rounded-l-md",
                        icon || "rounded-md",
                    ),
                    className,
                )}
                {...props}
            />
            {icon && (
                <span className="bg-background h-full w-10 cursor-pointer rounded-r-md p-1">
                    {icon}
                </span>
            )}
        </div>
    );
};
export default InputField;
