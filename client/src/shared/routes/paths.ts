export const ROUTES = {
  home: "/",
  trash: "/trash",
  categories: "/categories",
  login: "/login",
  register: "/register",
  topicDetail: (topicId: string) => `/topics/${topicId}`,
  topicDetailPattern: "/topics/:topicId",
} as const;
