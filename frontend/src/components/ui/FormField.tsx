"use client";

import React, { cloneElement, JSX, useId } from "react";
import { useField } from "formik";

const FormField = ({
    label,
    name,
    children,
}: {
    label?: string;
    name: string;
    children: JSX.Element;
}) => {
    const [field, meta] = useField(name);
    const id = useId();

    return (
        <div className="w-full">
            {label && (
                <label
                    className="text-text/90 font-medium"
                    htmlFor={id}
                    data-testid="formFieldLabel"
                >
                    {label}
                </label>
            )}
            {cloneElement(children, { id, ...field })}

            {meta.error && (
                <span className="text-error" data-testid="fieldError">
                    {meta.error}
                </span>
            )}
        </div>
    );
};
export default FormField;
