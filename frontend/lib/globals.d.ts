import * as yup from "yup";

declare module "yup" {
    interface StringSchema<TType, TContext, TDefault, TFlags> {
        username(): this;
        email(): this;
        password(): this;
    }
}
