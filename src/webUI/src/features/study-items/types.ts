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
  sourceDocumentChunkId: number
  sourceMaterialId: number
  sourceTitle: string
  startPage: number
  endPage: number
  items: StudyItemDraftResponse[]
}

export type StudyItemResponse = StudyItemDraftResponse & {
  id: number
  sourceMaterialId: number
  sourceDocumentChunkId: number
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
