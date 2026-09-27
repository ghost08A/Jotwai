// Category selection order shouldn't fragment the cache - sort before keying.
export const searchQueryKey = (q: string, categoryIds: string[]) =>
    ["search", q, [...categoryIds].sort()] as const;
