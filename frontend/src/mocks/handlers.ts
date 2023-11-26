import { HttpResponse, http } from "msw";

export const handlers = [
    http.get("http://127.0.0.1:3000/test", () => {
        return new Response("Hello, world!");
    }),
];
