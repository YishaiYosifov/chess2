import "yup";

declare module "yup" {
    interface StringSchema {
        username(): this;
    }
}
