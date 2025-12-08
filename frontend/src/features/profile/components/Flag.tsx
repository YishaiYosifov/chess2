import Image from "next/image";

const Flag = ({ countryCode, size }: { countryCode: string; size: number }) => {
    return (
        <Image
            src={`/assets/flags/${countryCode.toLowerCase()}.svg`}
            alt="flag"
            width={size}
            height={size}
        />
    );
};
export default Flag;
