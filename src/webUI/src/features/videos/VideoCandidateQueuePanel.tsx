import { Check, ExternalLink, RefreshCw, Search, X } from 'lucide-react'
import { Badge, EmptyState } from '../../components/shared'
import type { VideoCandidateImportResult, VideoCandidateResponse, VideoCandidateStatus } from './types'

type VideoCandidateQueuePanelProps = {
  candidates: VideoCandidateResponse[]
  importPending: boolean
  lastImport: VideoCandidateImportResult | null
  loading: boolean
  onAccept: (candidateId: number) => void
  onImport: () => void
  onReject: (candidateId: number) => void
  onSearch: (value: string) => void
  onStatusChange: (value: VideoCandidateStatus | 'all') => void
  search: string
  sourceFile: string | null
  status: VideoCandidateStatus | 'all'
  updating: boolean
}

export function VideoCandidateQueuePanel({
  candidates,
  importPending,
  lastImport,
  loading,
  onAccept,
  onImport,
  onReject,
  onSearch,
  onStatusChange,
  search,
  sourceFile,
  status,
  updating,
}: VideoCandidateQueuePanelProps) {
  return (
    <section className="border-b border-zinc-800 bg-[#0b0d10] px-5 py-4 md:px-8">
      <div className="flex flex-col gap-4 2xl:flex-row 2xl:items-center 2xl:justify-between">
        <div>
          <h2 className="text-sm font-semibold text-slate-100">Candidate queue</h2>
          <p className="mt-1 text-sm text-slate-500">
            Review videos from trusted channels before they become embedded study resources.
          </p>
          {sourceFile && <p className="mt-1 text-xs text-slate-500">{sourceFile}</p>}
        </div>
        <div className="flex flex-col gap-2 xl:flex-row xl:items-center">
          <label className="relative block xl:w-72">
            <Search className="pointer-events-none absolute left-3 top-2.5 h-4 w-4 text-slate-500" />
            <input
              className="focus-ring h-9 w-full rounded-md border border-zinc-800 bg-[#11151a] pl-9 pr-3 text-sm text-slate-100 placeholder:text-slate-600"
              placeholder="Search candidates"
              value={search}
              onChange={(event) => onSearch(event.target.value)}
            />
          </label>
          <div className="flex gap-1">
            {(['all', 'candidate', 'accepted', 'rejected'] as const).map((value) => (
              <button
                key={value}
                className={`focus-ring h-9 rounded-md border px-2.5 text-xs ${
                  status === value ? 'border-cyan-700 bg-cyan-950/30 text-cyan-100' : 'border-zinc-800 text-slate-400 hover:bg-zinc-900'
                }`}
                type="button"
                onClick={() => onStatusChange(value)}
              >
                {value}
              </button>
            ))}
          </div>
          <button
            className="focus-ring inline-flex h-9 items-center justify-center rounded-md border border-cyan-800 bg-cyan-950/40 px-3 text-sm font-medium text-cyan-100 hover:bg-cyan-900/50 disabled:cursor-not-allowed disabled:opacity-60"
            type="button"
            disabled={importPending}
            onClick={onImport}
          >
            <RefreshCw className={`mr-2 h-4 w-4 ${importPending ? 'animate-spin' : ''}`} />
            {importPending ? 'Importing candidates' : 'Import candidates'}
          </button>
        </div>
      </div>

      {lastImport && (
        <p className="mt-3 text-xs text-slate-400">
          Candidates: {lastImport.candidatesDiscovered} found, {lastImport.candidatesCreated} added,{' '}
          {lastImport.candidatesUpdated} refreshed, {lastImport.candidatesSkipped} skipped.
        </p>
      )}

      <div className="mt-4">
        {loading ? (
          <EmptyState title="Loading candidates" detail="Reading the curation queue from SQLite." />
        ) : candidates.length === 0 ? (
          <EmptyState title="No candidates in this view" detail="Import local candidates or adjust the current filters." />
        ) : (
          <div className="grid gap-2 xl:grid-cols-2 2xl:grid-cols-3">
            {candidates.map((candidate) => (
              <article key={candidate.id} className="rounded-md border border-zinc-800 bg-[#11151a] px-3 py-3">
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0">
                    <h3 className="line-clamp-2 text-sm font-medium leading-5 text-slate-100">{candidate.title}</h3>
                    <p className="mt-1 text-xs text-slate-500">{candidate.channelName}</p>
                  </div>
                  <a className="focus-ring shrink-0 rounded p-1 text-slate-500 hover:text-slate-200" href={candidate.url} rel="noreferrer" target="_blank">
                    <ExternalLink className="h-4 w-4" />
                  </a>
                </div>
                <p className="mt-2 line-clamp-2 text-xs leading-5 text-slate-400">{candidate.summary}</p>
                <div className="mt-2 flex flex-wrap gap-1.5">
                  <Badge>{candidate.difficulty}</Badge>
                  <Badge>{candidate.status}</Badge>
                  {candidate.tags.slice(0, 3).map((tag) => (
                    <Badge key={tag}>{tag}</Badge>
                  ))}
                </div>
                {candidate.rejectionReason && <p className="mt-2 line-clamp-2 text-xs text-red-300">{candidate.rejectionReason}</p>}
                <div className="mt-3 flex gap-2">
                  <button
                    className="focus-ring inline-flex h-8 flex-1 items-center justify-center rounded-md bg-emerald-500 px-2 text-xs font-medium text-emerald-950 hover:bg-emerald-400 disabled:cursor-not-allowed disabled:opacity-60"
                    type="button"
                    disabled={updating || candidate.status === 'accepted'}
                    onClick={() => onAccept(candidate.id)}
                  >
                    <Check className="mr-1.5 h-3.5 w-3.5" />
                    Accept
                  </button>
                  <button
                    className="focus-ring inline-flex h-8 flex-1 items-center justify-center rounded-md border border-zinc-700 px-2 text-xs font-medium text-slate-300 hover:bg-zinc-900 disabled:cursor-not-allowed disabled:opacity-60"
                    type="button"
                    disabled={updating || candidate.status === 'rejected'}
                    onClick={() => onReject(candidate.id)}
                  >
                    <X className="mr-1.5 h-3.5 w-3.5" />
                    Reject
                  </button>
                </div>
              </article>
            ))}
          </div>
        )}
      </div>
    </section>
  )
}
