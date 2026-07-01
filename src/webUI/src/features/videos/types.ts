export type LearningResourceProvider = 'youTube'

export type LearningResourceResponse = {
  id: number
  externalId: string
  provider: LearningResourceProvider
  title: string
  creator: string
  url: string
  embedUrl: string
  summary: string
  tags: string[]
  durationSeconds: number | null
  topicId: number | null
  sourceMaterialId: number | null
  sourceDocumentChunkId: number | null
  isWatched: boolean
  watchedAt: string | null
  watchProgressSeconds: number
  notes: string
  createdAt: string
  updatedAt: string
}

export type LearningResourceImportResult = {
  resourcesDiscovered: number
  resourcesCreated: number
  resourcesUpdated: number
}

export type UpdateLearningResourceWatchStateRequest = {
  isWatched: boolean
  watchProgressSeconds: number
  notes: string
}

export type TrustedChannelPriority = 'low' | 'medium' | 'high'

export type TrustedYouTubeChannelResponse = {
  id: number
  name: string
  url: string
  tags: string[]
  priority: TrustedChannelPriority
  notes: string
  createdAt: string
  updatedAt: string
}

export type TrustedYouTubeChannelImportResult = {
  sourceFile: string
  channelsDiscovered: number
  channelsCreated: number
  channelsUpdated: number
  channelsSkipped: number
}

export type VideoCandidateDifficulty = 'fundamentals' | 'intermediate' | 'advanced' | 'expert'

export type VideoCandidateStatus = 'candidate' | 'accepted' | 'rejected'

export type VideoCandidateResponse = {
  id: number
  externalId: string
  title: string
  channelName: string
  channelUrl: string
  url: string
  embedUrl: string
  summary: string
  tags: string[]
  difficulty: VideoCandidateDifficulty
  status: VideoCandidateStatus
  notes: string
  rejectionReason: string
  learningResourceId: number | null
  acceptedAt: string | null
  rejectedAt: string | null
  createdAt: string
  updatedAt: string
}

export type VideoCandidateImportResult = {
  sourceFile: string
  candidatesDiscovered: number
  candidatesCreated: number
  candidatesUpdated: number
  candidatesSkipped: number
}

export type AcceptVideoCandidateResponse = {
  candidate: VideoCandidateResponse
  resource: LearningResourceResponse
}

export type TopicResourceLinkResponse = {
  id: number
  topicSlug: string
  topicTitle: string
  moduleSlug: string
  moduleTitle: string
  learningResourceId: number | null
  videoCandidateId: number | null
  resourceKind: 'learningResource' | 'videoCandidate'
  title: string
  creator: string
  url: string
  embedUrl: string
  tags: string[]
  priority: number
  notes: string
  createdAt: string
  updatedAt: string
}

export type UpsertTopicResourceLinkRequest = {
  topicSlug: string
  learningResourceId: number | null
  videoCandidateId: number | null
  priority: number
  notes: string
}
