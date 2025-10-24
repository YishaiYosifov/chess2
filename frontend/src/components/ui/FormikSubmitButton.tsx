import { useFormikContext } from "formik";
import Button from "./Button";

const FormikSubmitButton = ({
    children,
    disabled,
    type,
    ...buttonProps
}: React.ButtonHTMLAttributes<HTMLButtonElement>) => {
    const { dirty, isValid, isSubmitting, status } = useFormikContext();

    return (
        <div className="flex flex-col">
            <Button
                type={type ?? "submit"}
                disabled={disabled || isSubmitting || !dirty || !isValid}
                data-testid="submitFormButton"
                {...buttonProps}
            >
                {children}
            </Button>
            {status && (
                <span className="text-error" data-testid="formStatus">
                    {status}
                </span>
            )}
        </div>
    );
};
export default FormikSubmitButton;
