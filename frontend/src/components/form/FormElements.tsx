import { BsEyeFill, BsEyeSlashFill } from "react-icons/bs";
import { InputGroup, Form, Button } from "react-bootstrap";
import { ButtonProps } from "react-bootstrap/esm/Button";
import { useField, useFormikContext } from "formik";

import {
    ComponentProps,
    ComponentType,
    ReactNode,
    useState,
    createElement,
} from "react";

interface HOCProps {
    name?: string;
    icon?: ReactNode;
}

/**
 * Higher order component that adds optional formim functionality to form components
 * as well as adds additional features
 */
const withFormElement = <P,>(WrappedComponent: ComponentType<P>) => {
    const FormikComponent = (props: P & HOCProps) => {
        return (
            <>
                <WrappedComponent {...props} />
                {props.icon && <InputGroup.Text>{props.icon}</InputGroup.Text>}
            </>
        );
    };
    return FormikComponent;
};

export const FormInput = withFormElement<ComponentProps<typeof Form.Control>>(
    ({ children, ...inputProps }) => {
        return (
            <>
                <Form.Control {...inputProps} aria-label={inputProps.name} />
                {children}
            </>
        );
    }
);

export const FormSelect = withFormElement<ComponentProps<typeof Form.Select>>(
    ({ children, ...inputProps }) => {
        return (
            <Form.Select {...inputProps} aria-label={inputProps.name}>
                {children}
            </Form.Select>
        );
    }
);

export const PasswordInput = withFormElement<
    ComponentProps<typeof Form.Control>
>(({ children, ...inputProps }) => {
    const [isShowingPassword, setIsShowingPassword] = useState(false);
    const EyeToggle = isShowingPassword ? BsEyeFill : BsEyeSlashFill;

    return (
        <>
            <Form.Control
                type={isShowingPassword ? "text" : "password"}
                {...inputProps}
                aria-label={inputProps.name}
            />

            <InputGroup.Text>
                <EyeToggle
                    onClick={() => setIsShowingPassword(!isShowingPassword)}
                    role="button"
                />
            </InputGroup.Text>
        </>
    );
});

export const SubmitButton = ({ children, ...buttonProps }: ButtonProps) => {
    const { dirty, isValid, isSubmitting } = useFormikContext();

    return (
        <Button
            type="submit"
            disabled={isSubmitting || !dirty || !isValid}
            data-testid="submitFormButton"
            {...buttonProps}
        >
            {children}
        </Button>
    );
};

type FormikFieldProps<P extends ComponentType> = {
    asInput: P;
    name: string;
} & ComponentProps<P>;

/**
 * Render a regular field as a formik field.
 * Adds error handling and formik field props.
 */
export const FormikField = <P extends ComponentType>({
    asInput,
    name,
    ...props
}: FormikFieldProps<P>) => {
    const [field, meta] = useField(name);

    return (
        <>
            {createElement(asInput as any, {
                ...field,
                ...props,
                isInvalid: meta.error,
            })}

            <Form.Control.Feedback type="invalid">
                {meta.error}
            </Form.Control.Feedback>
        </>
    );
};
