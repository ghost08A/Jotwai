export const ROUTES = {
  home: "/",
  notes: "/notes",
  trash: "/trash",
  categories: "/categories",
  login: "/login",
  register: "/register",
  topicDetail: (topicId: string) => `/topics/${topicId}`,
  topicDetailPattern: "/topics/:topicId",
} as const;
