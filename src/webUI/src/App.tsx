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
import './App.css'

const emptyModules: ModuleResponse[] = []
const emptyTopics: TopicSearchResponse[] = []
const emptySources: SourceMaterialResponse[] = []
const emptySourceChunks: SourceChunkSummaryResponse[] = []
const emptyStudyItems: StudyItemResponse[] = []

function App() {
  const queryClient = useQueryClient()
  const [activeView, setActiveView] = useState<WorkspaceView>('sources')
  const [catalogSearch, setCatalogSearch] = useState('')
  const [sourceSearch, setSourceSearch] = useState('')
  const [selectedModule, setSelectedModule] = useState<string>('all')
  const [selectedSourceId, setSelectedSourceId] = useState<number | null>(null)
  const [selectedChunkId, setSelectedChunkId] = useState<number | null>(null)
  const [lastIngestion, setLastIngestion] = useState<LocalPdfIngestionResult | null>(null)
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
