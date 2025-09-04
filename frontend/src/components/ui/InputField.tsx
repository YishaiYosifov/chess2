import { PolymorphicProps } from "@/types/polymorphicProps";
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
        <div className="relative">
            <Component
                className={twMerge(
                    `bg-background/50 autofill:bg-background/50 text-text w-full rounded-md border
                    border-white/20 p-1 disabled:cursor-not-allowed disabled:opacity-50`,
                    className,
                )}
                {...props}
            />
            {icon && (
                <span className="text-text absolute top-1/2 right-2 size-7 -translate-y-1/2 cursor-pointer">
                    {icon}
                </span>
            )}
        </div>
    );
};
export default InputField;
