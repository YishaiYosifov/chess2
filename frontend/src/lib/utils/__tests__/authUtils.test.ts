import { setIsAuthed } from "@/zustand/store";
import { apiRequest } from "../fetchUtils";
import { login } from "../authUtils";
import { Mock } from "vitest";

vi.mock("@/zustand/store", () => ({ setIsAuthed: vi.fn() }));
vi.mock("../fetchUtils", () => ({ apiRequest: vi.fn() }));

describe("login", () => {
    it("should create a form and set the username and password", async () => {
        const username = "username";
        const password = "password";
        const loginForm = new FormData();
        loginForm.set("username", username);
        loginForm.set("password", password);

        await login(username, password);

        expect(apiRequest).toHaveBeenCalledWith("/auth/login", {
            method: "POST",
            body: loginForm,
        });
    });

    it("should set the isAuthed state when the response is ok", async () => {
        const apiRequestMock = apiRequest as Mock;
        apiRequestMock.mockResolvedValue({ ok: true });

        await login("username", "password");
        expect(setIsAuthed).toHaveBeenCalledWith(true);
    });
});
