import type { FormikHelpers } from "formik";

import type { RatingOverview } from "@/client";

export type RatingMap = { [key: string]: RatingOverview };
export type FormikOnSubmit<V> = (values: V, helpers: FormikHelpers<V>) => void;
export type TypedCountries = Record<string, { name: string; flag: string }>;
