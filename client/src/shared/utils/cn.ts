import { clsx } from "clsx";
import type { ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

// shadcn/ui's standard class-merge helper: clsx resolves conditional classes,
// twMerge resolves conflicting Tailwind utilities (e.g. a default padding
// overridden by a prop-provided one) by keeping the last one instead of both.
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
