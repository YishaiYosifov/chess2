import Link from "next/link";

const notFound = (props) => {
    return (
        <div className="container">
            <div className="row justify-content-center">
                <div className="col-auto text-center">
                    <h1>Not found - 404!</h1>
                    <div>
                        <Link href="/">Go back to Home</Link>
                    </div>
                </div>
            </div>
        </div>
    );
};
export default notFound;
