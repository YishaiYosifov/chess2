import {
    type Config,
    type ClientOptions as DefaultClientOptions,
} from "./definition/client";
import { ClientOptions } from "./definition";

const baseClientConfig: Config<Required<DefaultClientOptions> & ClientOptions> =
    {
        baseUrl: process.env.NEXT_PUBLIC_API_URL,
        credentials: "include",
        // @ts-expect-error hey-api more like hey please add this to your docs
        security: [{ type: "apiKey" }],
    };
export default baseClientConfig;
