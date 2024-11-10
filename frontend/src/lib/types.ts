import type { FormikHelpers } from "formik";

import type { RatingOverview } from "@/lib/models";

export type RatingMap = { [key: string]: RatingOverview };
export type FormikOnSubmit<V> = (values: V, helpers: FormikHelpers<V>) => void;
export type TypedCountries = Record<string, { name: string; flag: string }>;
