export type TaskType =
  | 'read'
  | 'watch'
  | 'build'
  | 'debug'
  | 'refactor'
  | 'writeTests'
  | 'document'
  | 'review'

export type TopicProgressStatus = 'notStarted' | 'inProgress' | 'done'

export type TopicSearchResponse = {
  slug: string
  title: string
  summary: string | null
  moduleSlug: string
  moduleTitle: string
  status: TopicProgressStatus
  taskTypes: TaskType[]
}

export type ModuleResponse = {
  slug: string
  title: string
  description: string | null
  totalTopics: number
  doneTopics: number
  completionPercentage: number
  topics: Array<{
    slug: string
    title: string
    status: TopicProgressStatus
  }>
}

export type CatalogRollupsResponse = {
  modules: Array<{
    slug: string
    title: string
    totalTopics: number
    doneTopics: number
    inProgressTopics: number
    completionPercentage: number
  }>
  phases: Array<{
    slug: string
    title: string
    totalTopics: number
    doneTopics: number
    inProgressTopics: number
    completionPercentage: number
  }>
}

export type CurriculumSeed = {
  modules: Array<{
    id: string
    slug: string
    title: string
    description: string
    order: number
    topics: Array<{
      id: string
      slug: string
      title: string
      summary: string
      order: number
      taskTypes: TaskType[]
    }>
  }>
  phases: Array<{
    id: string
    slug: string
    title: string
    description: string
    order: number
    topics: Array<{ topicId: string; order: number }>
  }>
}
