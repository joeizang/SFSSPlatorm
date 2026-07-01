export type StudyItemKind = 'concept' | 'flashcard' | 'shortAnswer' | 'codeReading' | 'codingExercise'
export type StudyItemStatus = 'draft' | 'active' | 'archived'

export type StudyItemDraftResponse = {
  kind: StudyItemKind
  prompt: string
  expectedAnswer: string
  explanation: string
  sourceExcerpt: string
}

export type GeneratedStudyItemsResponse = {
  sourceDocumentChunkId: number | null
  sourceMaterialId: number | null
  topicSlug: string | null
  topicTitle: string | null
  sourceTitle: string
  startPage: number
  endPage: number
  items: StudyItemDraftResponse[]
}

export type StudyItemResponse = StudyItemDraftResponse & {
  id: number
  sourceMaterialId: number | null
  sourceDocumentChunkId: number | null
  topicSlug: string | null
  topicTitle: string | null
  startPage: number
  endPage: number
  status: StudyItemStatus
  attemptCount: number
  confidenceScore: number
  lastAttemptAt: string | null
  nextReviewAt: string | null
  createdAt: string
  updatedAt: string
}

export type CodingExerciseDifficulty = 'fundamentals' | 'intermediate' | 'advanced' | 'expert'

export type CodingExerciseResponse = {
  id: number
  topicSlug: string
  title: string
  prompt: string
  difficulty: CodingExerciseDifficulty
  language: string
  starterCode: string
  packageRequirements: string
  successCriteria: string
  hints: string
  checkDefinitionJson: string
  createdAt: string
  updatedAt: string
}

export type CodingExerciseSolutionResponse = {
  exerciseId: number
  topicSlug: string
  code: string
  createdAt: string
  updatedAt: string
  lastCheckedAt: string | null
}

export type ExerciseDiagnosticResponse = {
  id: string
  severity: 'error' | 'warning'
  message: string
  startLine: number
  startColumn: number
  endLine: number
  endColumn: number
}

export type PackageRequirementResponse = {
  manager: 'nuget' | 'npm'
  name: string
  status: 'declared' | 'notRequired'
}

export type ExerciseSampleCaseResponse = {
  name: string
  input: string
  expected: string | null
  expectedException: string | null
}

export type ExerciseRuleCheckResponse = {
  id: string
  title: string
  description: string
  status: 'pass' | 'fail'
  guidance: string
}

export type ExerciseGradingResponse = {
  status: 'passed' | 'needsWork' | 'blocked' | 'unsupported'
  passed: number
  requiredFailed: number
  checks: ExerciseRuleCheckResponse[]
  sampleCases: ExerciseSampleCaseResponse[]
}

export type ExerciseCheckResponse = {
  succeeded: boolean
  diagnostics: ExerciseDiagnosticResponse[]
  packageRequirements: PackageRequirementResponse[]
  grading: ExerciseGradingResponse
  checkedAt: string
}
