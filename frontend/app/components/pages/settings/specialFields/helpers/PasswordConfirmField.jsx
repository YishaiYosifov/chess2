import { useFormikContext } from "formik";
import { Form } from "react-bootstrap";

const PasswordConfirmField = () => {
    const { values, handleChange, errors } = useFormikContext();
    return (
        <Form.Group className="mt-1">
            <Form.Label>Confirm Password</Form.Label>
            <Form.Control
                aria-label="confirm password"
                name="password_confirm"
                type="password"
                value={values.password_confirm}
                onChange={handleChange}
                isInvalid={errors.password_confirm}
            />

            <Form.Control.Feedback type="invalid">
                {errors.password_confirm}
            </Form.Control.Feedback>
        </Form.Group>
    );
};
export default PasswordConfirmField;
