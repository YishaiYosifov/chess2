import TextField from "@/components/ui/TextField";
import countries from "@public/data/countries.json";

const CountrySelector = () => {
    return (
        <TextField label="Country" as="select">
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
        </TextField>
    );
};
export default CountrySelector;
