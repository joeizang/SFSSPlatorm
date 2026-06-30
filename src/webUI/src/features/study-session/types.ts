import type { StudyItemKind } from '../study-items/types'

export type StudyAttemptRating = 'again' | 'hard' | 'good' | 'easy'

export type StudySessionItemResponse = {
  id: number
  kind: StudyItemKind
  prompt: string
  expectedAnswer: string
  explanation: string
  sourceExcerpt: string
  startPage: number
  endPage: number
  sourceTitle: string
  sourceChunkHeading: string
  attemptCount: number
  confidenceScore: number
  lastAttemptAt: string | null
  nextReviewAt: string | null
}

export type StudySessionNextResponse = {
  item: StudySessionItemResponse | null
  dueCount: number
}

export type StudyAttemptResponse = {
  id: number
  studyItemId: number
  answer: string
  rating: StudyAttemptRating
  attemptedAt: string
  attemptCount: number
  confidenceScore: number
  nextReviewAt: string | null
}

export type StudySessionSummaryResponse = {
  activeItems: number
  dueItems: number
  answeredToday: number
  weakItems: number
}
