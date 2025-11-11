import Knook from "@public/assets/pieces-svg/knook-white.svg";
import Image from "next/image";

const AuthPageImage = () => {
    return (
        <section
            className="border-secondary/50 bg-checkerboard sticky hidden h-screen w-full border-l
                bg-center select-none md:block"
        >
            <Image
                src={Knook}
                alt="knook"
                draggable={false}
                className="h-full w-full"
                priority
            />
        </section>
    );
};
export default AuthPageImage;
