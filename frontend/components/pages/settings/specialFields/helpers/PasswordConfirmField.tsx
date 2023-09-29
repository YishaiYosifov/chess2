import { useFormikContext } from "formik";
import { Form } from "react-bootstrap";

interface PasswordConfirmValues {
    password_confirm: string;
}

const PasswordConfirmField = () => {
    const { values, handleChange, errors } = useFormikContext();
    const valuesType = values as PasswordConfirmValues;
    const errorsType = errors as PasswordConfirmValues;

    return (
        <Form.Group className="mt-1">
            <Form.Label>Confirm Password</Form.Label>
            <Form.Control
                aria-label="confirm password"
                name="password_confirm"
                type="password"
                value={valuesType.password_confirm}
                onChange={handleChange}
                isInvalid={Boolean(errorsType.password_confirm)}
            />

            <Form.Control.Feedback type="invalid">
                {errorsType.password_confirm}
            </Form.Control.Feedback>
        </Form.Group>
    );
};
export default PasswordConfirmField;
