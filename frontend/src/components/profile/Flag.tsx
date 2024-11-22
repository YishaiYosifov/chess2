import Image from "next/image";

const Flag = ({
    countryCode,
    size,
}: {
    countryCode?: string;
    size: number;
}) => {
    countryCode ??= "globe";

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
