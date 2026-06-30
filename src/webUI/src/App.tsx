import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from './api/http'
import { AppSidebar, type WorkspaceView } from './components/AppSidebar'
import { CatalogView } from './features/catalog/CatalogView'
import { previewSeed } from './features/catalog/previewSeed'
import type { CatalogRollupsResponse, ModuleResponse, TopicProgressStatus, TopicSearchResponse } from './features/catalog/types'
import { SourceLibraryView } from './features/sources/SourceLibraryView'
import type {
  LocalPdfIngestionResult,
  SourceChunkResponse,
  SourceChunkSummaryResponse,
  SourceMaterialResponse,
} from './features/sources/types'
import type { GeneratedStudyItemsResponse, StudyItemDraftResponse, StudyItemResponse } from './features/study-items/types'
import { StudySessionView } from './features/study-session/StudySessionView'
import type { StudyAttemptRating, StudyAttemptResponse, StudySessionNextResponse, StudySessionSummaryResponse } from './features/study-session/types'
import { VideoLibraryView } from './features/videos/VideoLibraryView'
import type {
  AcceptVideoCandidateResponse,
  LearningResourceImportResult,
  LearningResourceResponse,
  TrustedYouTubeChannelImportResult,
  TrustedYouTubeChannelResponse,
  VideoCandidateImportResult,
  VideoCandidateResponse,
  VideoCandidateStatus,
} from './features/videos/types'
import './App.css'

const emptyModules: ModuleResponse[] = []
const emptyTopics: TopicSearchResponse[] = []
const emptySources: SourceMaterialResponse[] = []
const emptySourceChunks: SourceChunkSummaryResponse[] = []
const emptyStudyItems: StudyItemResponse[] = []
const emptyVideos: LearningResourceResponse[] = []
const emptyChannels: TrustedYouTubeChannelResponse[] = []
const emptyCandidates: VideoCandidateResponse[] = []

function App() {
  const queryClient = useQueryClient()
  const [activeView, setActiveView] = useState<WorkspaceView>('sources')
  const [catalogSearch, setCatalogSearch] = useState('')
  const [sourceSearch, setSourceSearch] = useState('')
  const [channelSearch, setChannelSearch] = useState('')
  const [candidateSearch, setCandidateSearch] = useState('')
  const [candidateStatus, setCandidateStatus] = useState<VideoCandidateStatus | 'all'>('candidate')
  const [selectedModule, setSelectedModule] = useState<string>('all')
  const [selectedSourceId, setSelectedSourceId] = useState<number | null>(null)
  const [selectedChunkId, setSelectedChunkId] = useState<number | null>(null)
  const [lastIngestion, setLastIngestion] = useState<LocalPdfIngestionResult | null>(null)
  const [lastVideoImport, setLastVideoImport] = useState<LearningResourceImportResult | null>(null)
  const [lastChannelImport, setLastChannelImport] = useState<TrustedYouTubeChannelImportResult | null>(null)
  const [lastCandidateImport, setLastCandidateImport] = useState<VideoCandidateImportResult | null>(null)
  const [generatedStudyItems, setGeneratedStudyItems] = useState<StudyItemDraftResponse[]>([])

  const modulesQuery = useQuery({
    queryKey: ['modules'],
    queryFn: () => api<ModuleResponse[]>('/api/catalog/modules'),
  })

  const topicsQuery = useQuery({
    queryKey: ['topics', catalogSearch, selectedModule],
    queryFn: () => {
      const params = new URLSearchParams()
      if (catalogSearch.trim()) params.set('search', catalogSearch.trim())
      if (selectedModule !== 'all') params.set('moduleSlug', selectedModule)
      const query = params.toString()
      return api<TopicSearchResponse[]>(`/api/catalog/topics${query ? `?${query}` : ''}`)
    },
  })

  const rollupsQuery = useQuery({
    queryKey: ['rollups'],
    queryFn: () => api<CatalogRollupsResponse>('/api/catalog/rollups'),
  })

  const sourcesQuery = useQuery({
    queryKey: ['sources'],
    queryFn: () => api<SourceMaterialResponse[]>('/api/sources/'),
  })

  const chunksQuery = useQuery({
    queryKey: ['source-chunks', selectedSourceId],
    queryFn: () => api<SourceChunkSummaryResponse[]>(`/api/sources/${selectedSourceId}/chunks`),
    enabled: selectedSourceId !== null,
  })

  const chunkQuery = useQuery({
    queryKey: ['source-chunk', selectedChunkId],
    queryFn: () => api<SourceChunkResponse>(`/api/sources/chunks/${selectedChunkId}`),
    enabled: selectedChunkId !== null,
  })

  const studyItemsQuery = useQuery({
    queryKey: ['study-items', selectedChunkId],
    queryFn: () => api<StudyItemResponse[]>(`/api/study-items/?sourceDocumentChunkId=${selectedChunkId}`),
    enabled: selectedChunkId !== null,
  })

  const studySessionQuery = useQuery({
    queryKey: ['study-session-next'],
    queryFn: () => api<StudySessionNextResponse>('/api/study-session/next'),
    enabled: activeView === 'study',
  })

  const studySessionSummaryQuery = useQuery({
    queryKey: ['study-session-summary'],
    queryFn: () => api<StudySessionSummaryResponse>('/api/study-session/summary'),
    enabled: activeView === 'study',
  })

  const videosQuery = useQuery({
    queryKey: ['learning-resources'],
    queryFn: () => api<LearningResourceResponse[]>('/api/learning-resources/'),
    enabled: activeView === 'videos',
  })

  const channelsQuery = useQuery({
    queryKey: ['trusted-youtube-channels', channelSearch],
    queryFn: () => {
      const params = new URLSearchParams()
      if (channelSearch.trim()) params.set('search', channelSearch.trim())
      const query = params.toString()
      return api<TrustedYouTubeChannelResponse[]>(`/api/learning-resources/channels/${query ? `?${query}` : ''}`)
    },
    enabled: activeView === 'videos',
  })

  const channelSourceFileQuery = useQuery({
    queryKey: ['trusted-youtube-channel-source-file'],
    queryFn: () => api<{ sourceFile: string }>('/api/learning-resources/channels/source-file'),
    enabled: activeView === 'videos',
  })

  const videoCandidatesQuery = useQuery({
    queryKey: ['video-candidates', candidateSearch, candidateStatus],
    queryFn: () => {
      const params = new URLSearchParams()
      if (candidateSearch.trim()) params.set('search', candidateSearch.trim())
      if (candidateStatus !== 'all') params.set('status', candidateStatus)
      const query = params.toString()
      return api<VideoCandidateResponse[]>(`/api/learning-resources/candidates/${query ? `?${query}` : ''}`)
    },
    enabled: activeView === 'videos',
  })

  const videoCandidateSourceFileQuery = useQuery({
    queryKey: ['video-candidate-source-file'],
    queryFn: () => api<{ sourceFile: string }>('/api/learning-resources/candidates/source-file'),
    enabled: activeView === 'videos',
  })

  const importMutation = useMutation({
    mutationFn: () =>
      api('/api/curriculum/import', {
        method: 'POST',
        body: JSON.stringify(previewSeed),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries()
    },
  })

  const sourceIngestionMutation = useMutation({
    mutationFn: () => api<LocalPdfIngestionResult>('/api/sources/ingest-local', { method: 'POST' }),
    onSuccess: async (result) => {
      setLastIngestion(result)
      await queryClient.invalidateQueries({ queryKey: ['sources'] })
      await queryClient.invalidateQueries({ queryKey: ['source-chunks'] })
      await queryClient.invalidateQueries({ queryKey: ['source-chunk'] })
    },
  })

  const videoImportMutation = useMutation({
    mutationFn: () => api<LearningResourceImportResult>('/api/learning-resources/import-seed', { method: 'POST' }),
    onSuccess: async (result) => {
      setLastVideoImport(result)
      await queryClient.invalidateQueries({ queryKey: ['learning-resources'] })
    },
  })

  const channelImportMutation = useMutation({
    mutationFn: () =>
      api<TrustedYouTubeChannelImportResult>('/api/learning-resources/channels/import-local', { method: 'POST' }),
    onSuccess: async (result) => {
      setLastChannelImport(result)
      await queryClient.invalidateQueries({ queryKey: ['trusted-youtube-channels'] })
      await queryClient.invalidateQueries({ queryKey: ['trusted-youtube-channel-source-file'] })
    },
  })

  const candidateImportMutation = useMutation({
    mutationFn: () => api<VideoCandidateImportResult>('/api/learning-resources/candidates/import-local', { method: 'POST' }),
    onSuccess: async (result) => {
      setLastCandidateImport(result)
      await queryClient.invalidateQueries({ queryKey: ['video-candidates'] })
      await queryClient.invalidateQueries({ queryKey: ['video-candidate-source-file'] })
    },
  })

  const progressMutation = useMutation({
    mutationFn: ({ slug, status }: { slug: string; status: TopicProgressStatus }) =>
      api(`/api/catalog/topics/${slug}/progress`, {
        method: 'PUT',
        body: JSON.stringify({ status }),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['topics'] })
      await queryClient.invalidateQueries({ queryKey: ['modules'] })
      await queryClient.invalidateQueries({ queryKey: ['rollups'] })
    },
  })

  const generateStudyItemsMutation = useMutation({
    mutationFn: (sourceDocumentChunkId: number) =>
      api<GeneratedStudyItemsResponse>('/api/study-items/generate', {
        method: 'POST',
        body: JSON.stringify({ sourceDocumentChunkId }),
      }),
    onSuccess: (result) => {
      setGeneratedStudyItems(result.items)
    },
  })

  const createStudyItemsMutation = useMutation({
    mutationFn: ({ sourceDocumentChunkId, items }: { sourceDocumentChunkId: number; items: StudyItemDraftResponse[] }) =>
      api<StudyItemResponse[]>('/api/study-items/', {
        method: 'POST',
        body: JSON.stringify({ sourceDocumentChunkId, items }),
      }),
    onSuccess: async () => {
      setGeneratedStudyItems([])
      await queryClient.invalidateQueries({ queryKey: ['study-items', selectedChunkId] })
    },
  })

  const recordStudyAttemptMutation = useMutation({
    mutationFn: ({ answer, rating, studyItemId }: { answer: string; rating: StudyAttemptRating; studyItemId: number }) =>
      api<StudyAttemptResponse>('/api/study-session/attempts', {
        method: 'POST',
        body: JSON.stringify({ answer, rating, studyItemId }),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['study-session-next'] })
      await queryClient.invalidateQueries({ queryKey: ['study-session-summary'] })
      await queryClient.invalidateQueries({ queryKey: ['study-items'] })
    },
  })

  const updateVideoWatchStateMutation = useMutation({
    mutationFn: ({
      isWatched,
      notes,
      resourceId,
      watchProgressSeconds,
    }: {
      isWatched: boolean
      notes: string
      resourceId: number
      watchProgressSeconds: number
    }) =>
      api<LearningResourceResponse>(`/api/learning-resources/${resourceId}/watch-state`, {
        method: 'PUT',
        body: JSON.stringify({ isWatched, notes, watchProgressSeconds }),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['learning-resources'] })
    },
  })

  const acceptCandidateMutation = useMutation({
    mutationFn: (candidateId: number) =>
      api<AcceptVideoCandidateResponse>(`/api/learning-resources/candidates/${candidateId}/accept`, { method: 'POST' }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['video-candidates'] })
      await queryClient.invalidateQueries({ queryKey: ['learning-resources'] })
    },
  })

  const rejectCandidateMutation = useMutation({
    mutationFn: (candidateId: number) =>
      api<VideoCandidateResponse>(`/api/learning-resources/candidates/${candidateId}/reject`, {
        method: 'POST',
        body: JSON.stringify({ reason: 'Not a fit for the current study catalog.' }),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['video-candidates'] })
    },
  })

  const topics = topicsQuery.data ?? emptyTopics
  const modules = modulesQuery.data ?? emptyModules
  const sources = sourcesQuery.data ?? emptySources
  const completedSources = sources.filter((source) => source.extractionStatus === 'completed')
  const sourceChunks = chunksQuery.data ?? emptySourceChunks
  const selectedSource = sources.find((source) => source.id === selectedSourceId) ?? null

  const filteredSources = useMemo(() => {
    const value = sourceSearch.trim().toLowerCase()
    if (!value) return sources

    return sources.filter((source) =>
      [source.title, source.author, source.fileName]
        .filter(Boolean)
        .some((field) => field!.toLowerCase().includes(value)),
    )
  }, [sourceSearch, sources])

  useEffect(() => {
    if (selectedSourceId !== null || completedSources.length === 0) return
    setSelectedSourceId(completedSources[0].id)
  }, [completedSources, selectedSourceId])

  useEffect(() => {
    setSelectedChunkId(null)
  }, [selectedSourceId])

  useEffect(() => {
    setGeneratedStudyItems([])
  }, [selectedChunkId])

  useEffect(() => {
    if (selectedChunkId !== null || sourceChunks.length === 0) return
    setSelectedChunkId(sourceChunks[0].id)
  }, [selectedChunkId, sourceChunks])

  const selectedModuleTitle = useMemo(() => {
    if (selectedModule === 'all') return 'All modules'
    return modules.find((module) => module.slug === selectedModule)?.title ?? 'Module'
  }, [modules, selectedModule])

  return (
    <main className="min-h-screen bg-[#0b0d10] text-slate-200">
      <div className="grid min-h-screen grid-cols-1 lg:grid-cols-[280px_minmax(0,1fr)]">
        <AppSidebar
          activeView={activeView}
          completedSourceCount={completedSources.length}
          modules={modules}
          onSelectModule={setSelectedModule}
          onSelectView={setActiveView}
          selectedModule={selectedModule}
          sources={sources}
        />

        {activeView === 'sources' ? (
          <SourceLibraryView
            chunk={chunkQuery.data ?? null}
            chunks={sourceChunks}
            chunksLoading={chunksQuery.isLoading}
            filteredSources={filteredSources}
            ingestionError={sourceIngestionMutation.error}
            ingestionPending={sourceIngestionMutation.isPending}
            lastIngestion={lastIngestion}
            onIngest={() => sourceIngestionMutation.mutate()}
            onSearch={setSourceSearch}
            onSelectChunk={setSelectedChunkId}
            onSelectSource={setSelectedSourceId}
            search={sourceSearch}
            selectedChunkId={selectedChunkId}
            selectedSource={selectedSource}
            selectedSourceId={selectedSourceId}
            sourcesError={sourcesQuery.isError}
            sourcesLoading={sourcesQuery.isLoading}
            studyError={generateStudyItemsMutation.error ?? createStudyItemsMutation.error}
            studyGeneratePending={generateStudyItemsMutation.isPending}
            studyItems={studyItemsQuery.data ?? emptyStudyItems}
            studyItemsLoading={studyItemsQuery.isLoading}
            studySavePending={createStudyItemsMutation.isPending}
            generatedStudyItems={generatedStudyItems}
            onGenerateStudyItems={() => {
              if (selectedChunkId !== null) {
                generateStudyItemsMutation.mutate(selectedChunkId)
              }
            }}
            onSaveStudyItems={(items) => {
              if (selectedChunkId !== null) {
                createStudyItemsMutation.mutate({ sourceDocumentChunkId: selectedChunkId, items })
              }
            }}
          />
        ) : activeView === 'study' ? (
          <StudySessionView
            attemptPending={recordStudyAttemptMutation.isPending}
            error={studySessionQuery.error ?? recordStudyAttemptMutation.error}
            item={studySessionQuery.data?.item ?? null}
            loading={studySessionQuery.isLoading}
            onSkip={() => queryClient.invalidateQueries({ queryKey: ['study-session-next'] })}
            onSubmitAttempt={(answer, rating) => {
              const studyItemId = studySessionQuery.data?.item?.id
              if (studyItemId) {
                recordStudyAttemptMutation.mutate({ answer, rating, studyItemId })
              }
            }}
            summary={studySessionSummaryQuery.data}
          />
        ) : activeView === 'videos' ? (
          <VideoLibraryView
            candidateImportPending={candidateImportMutation.isPending}
            candidateLoading={videoCandidatesQuery.isLoading}
            candidateSearch={candidateSearch}
            candidateSourceFile={videoCandidateSourceFileQuery.data?.sourceFile ?? null}
            candidateStatus={candidateStatus}
            candidateUpdating={acceptCandidateMutation.isPending || rejectCandidateMutation.isPending}
            candidates={videoCandidatesQuery.data ?? emptyCandidates}
            channelImportPending={channelImportMutation.isPending}
            channelLoading={channelsQuery.isLoading}
            channelSearch={channelSearch}
            channelSourceFile={channelSourceFileQuery.data?.sourceFile ?? null}
            channels={channelsQuery.data ?? emptyChannels}
            error={
              videosQuery.error ??
              channelsQuery.error ??
              channelSourceFileQuery.error ??
              videoCandidatesQuery.error ??
              videoCandidateSourceFileQuery.error ??
              videoImportMutation.error ??
              channelImportMutation.error ??
              candidateImportMutation.error ??
              acceptCandidateMutation.error ??
              rejectCandidateMutation.error ??
              updateVideoWatchStateMutation.error
            }
            importPending={videoImportMutation.isPending}
            lastCandidateImport={lastCandidateImport}
            lastChannelImport={lastChannelImport}
            lastImport={lastVideoImport}
            loading={videosQuery.isLoading}
            onCandidateAccept={(candidateId) => acceptCandidateMutation.mutate(candidateId)}
            onCandidateImport={() => candidateImportMutation.mutate()}
            onCandidateReject={(candidateId) => rejectCandidateMutation.mutate(candidateId)}
            onCandidateSearch={setCandidateSearch}
            onCandidateStatusChange={setCandidateStatus}
            onChannelImport={() => channelImportMutation.mutate()}
            onChannelSearch={setChannelSearch}
            onImport={() => videoImportMutation.mutate()}
            onSaveWatchState={(resourceId, isWatched, watchProgressSeconds, notes) =>
              updateVideoWatchStateMutation.mutate({ resourceId, isWatched, watchProgressSeconds, notes })
            }
            saving={updateVideoWatchStateMutation.isPending}
            videos={videosQuery.data ?? emptyVideos}
          />
        ) : (
          <CatalogView
            activeTopics={topics.filter((topic) => topic.status === 'inProgress')}
            doneTopics={topics.filter((topic) => topic.status === 'done')}
            hasData={modules.length > 0 || topics.length > 0}
            importPending={importMutation.isPending}
            modules={modules}
            onImport={() => importMutation.mutate()}
            onProgressChange={(slug, status) => progressMutation.mutate({ slug, status })}
            onSearch={setCatalogSearch}
            progressPending={progressMutation.isPending}
            rollups={rollupsQuery.data}
            search={catalogSearch}
            selectedModuleTitle={selectedModuleTitle}
            topics={topics}
            topicsError={topicsQuery.isError}
            topicsLoading={topicsQuery.isLoading}
          />
        )}
      </div>
    </main>
  )
}

export default App
