import type { StudyItemKind } from './types'

export function studyItemKindLabel(kind: StudyItemKind) {
  const labels: Record<StudyItemKind, string> = {
    codeReading: 'Code reading',
    codingExercise: 'Coding exercise',
    concept: 'Concept',
    flashcard: 'Flashcard',
    shortAnswer: 'Short answer',
  }

  return labels[kind]
}
