import { HttpResponse, http } from "msw";

const API_URL = process.env.NEXT_PUBLIC_API_URL;
export const handlers = [
    http.get(`${API_URL}/profile/:username/info`, () => {
        return HttpResponse.json({});
    }),
];
