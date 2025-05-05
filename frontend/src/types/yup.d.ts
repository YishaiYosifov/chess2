import "yup";

declare module "yup" {
    interface StringSchema {
        username(): this;
        email(): this;
        password(): this;
    }
}
