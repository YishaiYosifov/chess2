import Knook from "@public/assets/pieces-svg/knook-white.svg";
import Image from "next/image";

const AuthPageImage = () => {
    return (
        <section
            className="hidden h-full min-h-0 w-full select-none border-l border-primary bg-[#151515]
                bg-checkerboard bg-[length:10rem_10rem] bg-center md:block"
        >
            <Image
                src={Knook}
                alt="knook"
                draggable={false}
                className="h-full max-h-full min-h-0 w-full"
            />
        </section>
    );
};
export default AuthPageImage;
