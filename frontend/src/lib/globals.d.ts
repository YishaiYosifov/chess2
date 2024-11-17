// eslint-disable-next-line @typescript-eslint/no-unused-vars
import * as yup from "yup";

declare module "yup" {
    interface StringSchema {
        username(): this;
        email(): this;
        password(): this;
    }
}
