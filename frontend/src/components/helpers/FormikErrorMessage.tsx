import { useField } from "formik";

const FormikErrorMessage = ({ name }: { name: string }) => {
    const { error } = useField(name)[1];
    return <>{error && <span className="text-error">{error}</span>}</>;
};
export default FormikErrorMessage;
