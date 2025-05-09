import { auth } from "./lib/auth";

export async function middleware() {
    console.log(await auth());
}
