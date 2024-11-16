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
        <>
            <Button
                type={type ?? "submit"}
                disabled={disabled || isSubmitting || !dirty || !isValid}
                data-testid="submitFormButton"
                {...buttonProps}
            >
                {children}
            </Button>
            {status && <span className="text-error">{status}</span>}
        </>
    );
};
export default FormikSubmitButton;
