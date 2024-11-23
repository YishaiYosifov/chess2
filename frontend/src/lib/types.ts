import type { FormikHelpers } from "formik";

import type { RatingOverview } from "@/lib/apiClient/models";

export type RatingMap = { [key: string]: RatingOverview };
export type FormikOnSubmit<V> = (values: V, helpers: FormikHelpers<V>) => void;
