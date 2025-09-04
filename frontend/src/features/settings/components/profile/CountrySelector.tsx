import InputField from "@/components/ui/InputField";
import countries from "@public/data/countries.json";
import FormField from "@/components/ui/FormField";

const CountrySelector = ({ name }: { name: string }) => {
    return (
        <FormField label="Country" name={name}>
            <InputField as="select" data-testid="countrySelector">
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
            </InputField>
        </FormField>
    );
};
export default CountrySelector;
