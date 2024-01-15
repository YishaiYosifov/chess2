import * as yup from "yup";

yup.addMethod(yup.string, "username", function () {
    return this.required("username must be between 1 and 30 characters")
        .test(
            "username-length",
            "username must be between 1 and 30 characters",
            (value) => value.length <= 30
        )
        .test(
            "username-spaces",
            "username can't include spaces",
            (value) => !value.includes(" ")
        )
        .test(
            "username-numbers",
            "username can't be just numbers",
            (value) => !+value
        );
});

yup.addMethod(yup.string, "email", function () {
    return this.required("email is required")
        .matches(/[\w\.-]+@[\w\.-]+(\.[\w]+)+/g, "Invalid Email")
        .max(256, "Email Too Long");
});

yup.addMethod(yup.string, "password", function () {
    return this.required("password is required")
        .min(8, "password must be at least 8 characters long")
        .matches(
            /^(?=.*[a-z])(?=.*[A-Z]).+$/,
            "Password must have both upper and lower case letters"
        )
        .matches(/\d/, "password must have a number");
});

export const usernameSchema = yup.string().username();
export const emailSchema = yup.string().email();
export const passwordSchema = yup.string().password();
