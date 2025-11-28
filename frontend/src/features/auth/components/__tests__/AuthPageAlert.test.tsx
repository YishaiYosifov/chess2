import { render, screen } from "@testing-library/react";
import Cookies from "js-cookie";

import { mockJsCookie } from "@/lib/testUtils/mocks/mockCookies";
import AuthPageAlert from "../AuthPageAlert";
import constants from "@/lib/constants";
import { ErrorCode } from "@/lib/apiClient";

vi.mock("js-cookie");

describe("AuthPageAlert", () => {
    it("should render nothing if there is no auth failure cookie", () => {
        mockJsCookie({});

        render(<AuthPageAlert />);
        expect(screen.queryByTestId("authPageAlert")).toBeNull();
    });

    it("should render a custom banned message if the user is banned", () => {
        mockJsCookie({
            [constants.COOKIES.AUTH_FAILURE]: ErrorCode.AUTH_USER_BANNED,
        });

        render(<AuthPageAlert />);

        expect(screen.getByTestId("authPageAlert")).toHaveTextContent(
            "Your account has been banned.",
        );
        expect(Cookies.remove).toHaveBeenCalledWith(
            constants.COOKIES.AUTH_FAILURE,
        );
    });

    it("should render a default message for unknown error codes", () => {
        mockJsCookie({
            [constants.COOKIES.AUTH_FAILURE]: "SOME_UNKNOWN_ERROR" as ErrorCode,
        });

        render(<AuthPageAlert />);

        expect(screen.getByTestId("authPageAlert")).toHaveTextContent(
            "Failed to log in, please try again.",
        );
        expect(Cookies.remove).toHaveBeenCalledWith(
            constants.COOKIES.AUTH_FAILURE,
        );
    });

    it("should remove the auth failure cookie after reading it", () => {
        mockJsCookie({
            [constants.COOKIES.AUTH_FAILURE]: ErrorCode.AUTH_USER_BANNED,
        });

        render(<AuthPageAlert />);

        expect(Cookies.remove).toHaveBeenCalledWith(
            constants.COOKIES.AUTH_FAILURE,
        );
    });
});
