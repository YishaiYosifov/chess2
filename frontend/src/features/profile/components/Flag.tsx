import Image from "next/image";

const Flag = ({
    countryCode,
    size,
}: {
    countryCode?: string | null;
    size: number;
}) => {
    if (!countryCode)
        return (
            <Image
                src="/assets/flags/international.svg"
                alt="flag"
                width={size}
                height={size}
            />
        );

    return (
        <Image
            src={`/assets/flags/${countryCode}.svg`}
            data-testid="flag"
            alt="flag"
            width={size}
            height={size}
        />
    );
};
export default Flag;
