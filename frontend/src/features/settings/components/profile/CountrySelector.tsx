import FormikTextField from "@/components/ui/FormikField";
import countries from "@public/data/countries.json";

const CountrySelector = ({ name }: { name: string }) => {
    return (
        <FormikTextField
            label="Country"
            as="select"
            name={name}
            data-testid="countrySelector"
        >
            {Object.entries(countries as Record<string, string>).map(
                ([code, name]) => (
                    <option
                        key={code}
                        value={code}
                        className="bg-white text-black"
                    >
                        {name}
                    </option>
                ),
            )}
        </FormikTextField>
    );
};
export default CountrySelector;
