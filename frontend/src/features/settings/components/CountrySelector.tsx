import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";

import TextField from "@/components/ui/TextField";
import countries from "@public/data/countries.json";

const CountrySelector = () => {
    const user = useAuthedUser();
    if (!user) return null;

    return (
        <TextField label="Country" as="select" defaultValue={user.countryCode}>
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
