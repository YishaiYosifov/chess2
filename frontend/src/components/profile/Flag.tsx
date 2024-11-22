import Image from "next/image";

const Flag = ({
    countryCode,
    size,
}: {
    countryCode?: string;
    size: number;
}) => {
    if (!countryCode) return;

    return (
        <Image
            src={`/assets/flags/${countryCode}.svg`}
            alt="flag"
            width={size}
            height={size}
        />
    );
};
export default Flag;
