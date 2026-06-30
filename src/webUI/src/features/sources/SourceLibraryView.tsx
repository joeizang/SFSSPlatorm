import { BookMarked, FileText, RefreshCw, Search } from 'lucide-react'
import { Badge, EmptyState, StatusPill } from '../../components/shared'
import { StudyItemPanel } from '../study-items/StudyItemPanel'
import type { StudyItemDraftResponse, StudyItemResponse } from '../study-items/types'
import { ReaderContent } from './ReaderContent'
import { accessLabel, statusLabel } from './sourceLabels'
import type {
  LocalPdfIngestionResult,
  SourceChunkResponse,
  SourceChunkSummaryResponse,
  SourceMaterialResponse,
} from './types'

type SourceLibraryViewProps = {
  chunk: SourceChunkResponse | null
  chunks: SourceChunkSummaryResponse[]
  chunksLoading: boolean
  filteredSources: SourceMaterialResponse[]
  ingestionError: Error | null
  ingestionPending: boolean
  lastIngestion: LocalPdfIngestionResult | null
  onIngest: () => void
  onGenerateStudyItems: () => void
  onSearch: (value: string) => void
  onSelectChunk: (id: number) => void
  onSelectSource: (id: number) => void
  onSaveStudyItems: (items: StudyItemDraftResponse[]) => void
  search: string
  selectedChunkId: number | null
  selectedSource: SourceMaterialResponse | null
  selectedSourceId: number | null
  sourcesError: boolean
  sourcesLoading: boolean
  generatedStudyItems: StudyItemDraftResponse[]
  studyError: Error | null
  studyGeneratePending: boolean
  studyItems: StudyItemResponse[]
  studyItemsLoading: boolean
  studySavePending: boolean
}

export function SourceLibraryView({
  chunk,
  chunks,
  chunksLoading,
  filteredSources,
  ingestionError,
  ingestionPending,
  lastIngestion,
  onIngest,
  onGenerateStudyItems,
  onSearch,
  onSelectChunk,
  onSelectSource,
  onSaveStudyItems,
  search,
  selectedChunkId,
  selectedSource,
  selectedSourceId,
  sourcesError,
  sourcesLoading,
  generatedStudyItems,
  studyError,
  studyGeneratePending,
  studyItems,
  studyItemsLoading,
  studySavePending,
}: SourceLibraryViewProps) {
  return (
    <section className="min-w-0">
      <header className="border-b border-zinc-800 bg-[#0f1216] px-5 py-4 md:px-8">
        <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
          <div>
            <h1 className="text-xl font-semibold tracking-tight text-slate-50">Source library</h1>
            <p className="mt-1 text-sm text-slate-500">
              Local PDFs stay on this machine. Extracted text is stored in the ignored SQLite database for in-app study.
            </p>
          </div>
          <button
            className="focus-ring inline-flex h-9 items-center justify-center rounded-md bg-cyan-500 px-3 text-sm font-medium text-slate-950 hover:bg-cyan-400 disabled:cursor-not-allowed disabled:opacity-60"
            type="button"
            disabled={ingestionPending}
            onClick={onIngest}
          >
            <RefreshCw className={`mr-2 h-4 w-4 ${ingestionPending ? 'animate-spin' : ''}`} />
            {ingestionPending ? 'Extracting PDFs' : 'Ingest focused PDFs'}
          </button>
        </div>
        {lastIngestion && (
          <div className="mt-4 grid gap-2 text-xs text-slate-400 sm:grid-cols-5">
            <StatusPill label="Found" value={lastIngestion.sourcesDiscovered} />
            <StatusPill label="Ingested" value={lastIngestion.sourcesIngested} />
            <StatusPill label="Skipped" value={lastIngestion.sourcesSkipped} />
            <StatusPill label="Failed" value={lastIngestion.sourcesFailed} />
            <StatusPill label="Chunks" value={lastIngestion.chunksCreated} />
          </div>
        )}
        {ingestionError && <p className="mt-3 text-sm text-red-300">Ingestion failed: {ingestionError.message}</p>}
      </header>

      <div className="grid min-h-[calc(100vh-121px)] xl:grid-cols-[360px_300px_minmax(0,1fr)_420px]">
        <SourceList
          filteredSources={filteredSources}
          onSearch={onSearch}
          onSelectSource={onSelectSource}
          search={search}
          selectedSourceId={selectedSourceId}
          sourcesError={sourcesError}
          sourcesLoading={sourcesLoading}
        />
        <ChunkList
          chunks={chunks}
          chunksLoading={chunksLoading}
          onSelectChunk={onSelectChunk}
          selectedChunkId={selectedChunkId}
          selectedSource={selectedSource}
        />
        <ReaderPane chunk={chunk} />
        <StudyItemPanel
          chunkId={selectedChunkId}
          generatedItems={generatedStudyItems}
          generating={studyGeneratePending}
          onGenerate={onGenerateStudyItems}
          onSave={onSaveStudyItems}
          savedItems={studyItems}
          savedItemsLoading={studyItemsLoading}
          saving={studySavePending}
          studyError={studyError}
        />
      </div>
    </section>
  )
}

function SourceList({
  filteredSources,
  onSearch,
  onSelectSource,
  search,
  selectedSourceId,
  sourcesError,
  sourcesLoading,
}: Pick<
  SourceLibraryViewProps,
  'filteredSources' | 'onSearch' | 'onSelectSource' | 'search' | 'selectedSourceId' | 'sourcesError' | 'sourcesLoading'
>) {
  return (
    <section className="border-b border-zinc-800 px-5 py-5 xl:border-b-0 xl:border-r">
      <label className="relative block">
        <Search className="pointer-events-none absolute left-3 top-2.5 h-4 w-4 text-slate-500" />
        <input
          className="focus-ring h-9 w-full rounded-md border border-zinc-800 bg-[#11151a] pl-9 pr-3 text-sm text-slate-100 placeholder:text-slate-600"
          placeholder="Search books"
          value={search}
          onChange={(event) => onSearch(event.target.value)}
        />
      </label>

      <div className="mt-4 space-y-2">
        {sourcesLoading ? (
          <EmptyState title="Loading sources" detail="Reading local source metadata from SQLite." />
        ) : sourcesError ? (
          <EmptyState title="API unavailable" detail="Start the backend on http://localhost:5105 and refresh." />
        ) : filteredSources.length === 0 ? (
          <EmptyState title="No sources ingested" detail="Use Ingest focused PDFs to extract readable local content." />
        ) : (
          filteredSources.map((source) => (
            <SourceCard
              key={source.id}
              onSelectSource={onSelectSource}
              selected={selectedSourceId === source.id}
              source={source}
            />
          ))
        )}
      </div>
    </section>
  )
}

function SourceCard({
  onSelectSource,
  selected,
  source,
}: {
  onSelectSource: (id: number) => void
  selected: boolean
  source: SourceMaterialResponse
}) {
  return (
    <button
      className={`focus-ring w-full rounded-md border px-3 py-3 text-left ${
        selected ? 'border-cyan-700 bg-cyan-950/20' : 'border-zinc-800 bg-[#0f1216] hover:border-zinc-700 hover:bg-[#11151a]'
      }`}
      type="button"
      onClick={() => onSelectSource(source.id)}
    >
      <div className="flex items-start gap-3">
        <FileText className="mt-0.5 h-4 w-4 shrink-0 text-slate-500" />
        <div className="min-w-0">
          <div className="line-clamp-2 text-sm font-medium leading-5 text-slate-100">{source.title}</div>
          <div className="mt-1 truncate text-xs text-slate-500">{source.author ?? accessLabel(source.access)}</div>
          <div className="mt-2 flex flex-wrap gap-1.5">
            <Badge>{source.chunkCount} chunks</Badge>
            {source.pageCount && <Badge>{source.pageCount} pages</Badge>}
            <Badge>{statusLabel(source.extractionStatus)}</Badge>
          </div>
          {source.extractionError && <p className="mt-2 line-clamp-2 text-xs text-red-300">{source.extractionError}</p>}
        </div>
      </div>
    </button>
  )
}

function ChunkList({
  chunks,
  chunksLoading,
  onSelectChunk,
  selectedChunkId,
  selectedSource,
}: Pick<SourceLibraryViewProps, 'chunks' | 'chunksLoading' | 'onSelectChunk' | 'selectedChunkId' | 'selectedSource'>) {
  return (
    <section className="border-b border-zinc-800 bg-[#0f1216] px-4 py-5 xl:border-b-0 xl:border-r">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-semibold text-slate-100">Page chunks</h2>
        {selectedSource && <span className="text-xs text-slate-500">{selectedSource.chunkCount}</span>}
      </div>
      <div className="mt-4 space-y-2">
        {!selectedSource ? (
          <EmptyState title="Select a source" detail="Choose an ingested book to read extracted sections." />
        ) : chunksLoading ? (
          <EmptyState title="Loading chunks" detail="Reading extracted page ranges." />
        ) : chunks.length === 0 ? (
          <EmptyState title="No readable chunks" detail="This source has no extracted text chunks yet." />
        ) : (
          chunks.map((item) => (
            <button
              key={item.id}
              className={`focus-ring w-full rounded-md border px-3 py-2 text-left ${
                selectedChunkId === item.id ? 'border-cyan-700 bg-cyan-950/20' : 'border-zinc-800 bg-[#11151a] hover:border-zinc-700'
              }`}
              type="button"
              onClick={() => onSelectChunk(item.id)}
            >
              <div className="text-xs text-slate-500">Pages {item.startPage}-{item.endPage}</div>
              <div className="mt-1 line-clamp-2 text-sm leading-5 text-slate-200">{item.heading}</div>
            </button>
          ))
        )}
      </div>
    </section>
  )
}

function ReaderPane({ chunk }: { chunk: SourceChunkResponse | null }) {
  return (
    <article className="min-w-0 px-5 py-5 md:px-8">
      {chunk ? (
        <>
          <div className="border-b border-zinc-800 pb-4">
            <div className="flex items-center gap-2 text-xs text-slate-500">
              <BookMarked className="h-4 w-4" />
              <span>{chunk.sourceTitle}</span>
            </div>
            <h2 className="mt-2 text-lg font-semibold text-slate-50">{chunk.heading}</h2>
            <p className="mt-1 text-sm text-slate-500">
              Pages {chunk.startPage}-{chunk.endPage} · {chunk.characterCount.toLocaleString()} characters
            </p>
          </div>
          <ReaderContent chunk={chunk} />
        </>
      ) : (
        <EmptyState
          title="Reader empty"
          detail="Ingest local PDFs, select a source, then choose a page chunk to read inside the app."
        />
      )}
    </article>
  )
}
