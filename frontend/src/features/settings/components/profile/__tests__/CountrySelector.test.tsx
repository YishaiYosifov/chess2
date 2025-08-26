import { render, screen } from "@testing-library/react";

import countries from "@public/data/countries.json";
import CountrySelector from "../profile/CountrySelector";
import { Form, Formik } from "formik";
import userEvent from "@testing-library/user-event";

describe("CountrySelector", () => {
    const renderWithFormik = (initialValues = { country: "" }) =>
        render(
            <Formik initialValues={initialValues} onSubmit={() => {}}>
                <Form>
                    <CountrySelector name="country" />
                </Form>
            </Formik>,
        );

    it("should render a select field with the correct name inside Formik", () => {
        renderWithFormik();

        const select = screen.getByTestId("countrySelector");
        expect(select).toBeInTheDocument();
        expect(select).toHaveAttribute("name", "country");
    });

    it("should render an option for each country from the JSON", () => {
        renderWithFormik();
        const options = screen.getAllByRole("option");
        expect(options).toHaveLength(Object.keys(countries).length);

        Object.entries(countries).forEach(([code, name]) => {
            const option = screen.getByRole("option", { name });
            expect(option).toHaveValue(code);
        });
    });

    it("should render options with the correct classes", () => {
        renderWithFormik();

        const firstOptionName = Object.values(countries)[0];
        const option = screen.getByRole("option", { name: firstOptionName });

        expect(option).toHaveClass("bg-white");
        expect(option).toHaveClass("text-black");
    });

    it("should update Formik state when a country is selected", async () => {
        let selectedValue = "";
        const user = userEvent.setup();
        render(
            <Formik
                initialValues={{ country: "" }}
                onSubmit={(values) => {
                    selectedValue = values.country;
                }}
            >
                <Form>
                    <CountrySelector name="country" />
                    <button type="submit">Submit</button>
                </Form>
            </Formik>,
        );

        const select = screen.getByRole("combobox");
        await user.selectOptions(select, Object.keys(countries)[0]);
        expect(select).toHaveValue(Object.keys(countries)[0]);

        await user.click(screen.getByRole("button"));
        expect(selectedValue).toBe(Object.keys(countries)[0]);
    });
});
