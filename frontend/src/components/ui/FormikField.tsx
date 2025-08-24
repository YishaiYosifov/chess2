import { useField } from "formik";
import React from "react";
import TextField, { TextFieldProps } from "./TextField";

const FormikTextField = <C extends React.ElementType = "input">(
    props: TextFieldProps<C> & { name: string },
) => {
    const [field, meta] = useField(props.name);

    return (
        <div>
            <TextField {...props} {...field} />
            {meta.error && (
                <span className="text-error" data-testid="fieldError">
                    {meta.error}
                </span>
            )}
        </div>
    );
};
export default FormikTextField;
