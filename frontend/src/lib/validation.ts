import * as yup from "yup";

yup.addMethod(yup.string, "username", function () {
    return this.required("username must be between 1 and 30 characters")
        .test(
            "username-length",
            "username must be between 1 and 30 characters",
            (value) => value.length <= 30,
        )
        .test(
            "username-spaces",
            "username can't include spaces",
            (value) => !value.includes(" "),
        );
});

export const usernameSchema = yup.string().username();
