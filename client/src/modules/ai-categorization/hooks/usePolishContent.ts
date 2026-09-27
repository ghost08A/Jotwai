import { useMutation } from "@tanstack/react-query";
import { polishContent } from "../services/contentPolishService";

export function usePolishContent() {
    return useMutation({
        mutationFn: (content: string) => polishContent(content),
    });
}
