export type SourceAccess = 'open' | 'purchasedLocal' | 'unknownLocal'
export type ExtractionStatus = 'notStarted' | 'completed' | 'failed'

export type SourceMaterialResponse = {
  id: number
  title: string
  author: string | null
  fileName: string
  access: SourceAccess
  fileSizeBytes: number
  pageCount: number | null
  extractionStatus: ExtractionStatus
  extractionError: string | null
  extractedAt: string | null
  chunkCount: number
  characterCount: number
}

export type SourceChunkSummaryResponse = {
  id: number
  order: number
  startPage: number
  endPage: number
  heading: string
  characterCount: number
}

export type SourceChunkResponse = SourceChunkSummaryResponse & {
  sourceMaterialId: number
  sourceTitle: string
  text: string
}

export type LocalPdfIngestionResult = {
  sourcesDiscovered: number
  sourcesIngested: number
  sourcesSkipped: number
  sourcesFailed: number
  chunksCreated: number
}

export type ReaderBlock =
  | { kind: 'prose'; text: string }
  | { kind: 'code'; text: string; language: string; label: string }
