import * as yup from "yup";

yup.addMethod(yup.string, "username", function () {
    return this.min(3, "Must be between 3 and 30 characters")
        .max(30, "Must be between 1 and 30 characters")
        .matches(
            /^[a-zA-Z0-9-_]+$/,
            "Only letters, numbers, hyphens, and underscores are allowed",
        );
});

export const UsernameSchema = yup.string().username();
