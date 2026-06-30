import { ExternalLink, RefreshCw, Search } from 'lucide-react'
import { Badge, EmptyState } from '../../components/shared'
import type { TrustedYouTubeChannelImportResult, TrustedYouTubeChannelResponse } from './types'

type TrustedChannelsPanelProps = {
  channels: TrustedYouTubeChannelResponse[]
  importPending: boolean
  lastImport: TrustedYouTubeChannelImportResult | null
  loading: boolean
  onImport: () => void
  search: string
  sourceFile: string | null
  onSearch: (value: string) => void
}

export function TrustedChannelsPanel({
  channels,
  importPending,
  lastImport,
  loading,
  onImport,
  onSearch,
  search,
  sourceFile,
}: TrustedChannelsPanelProps) {
  return (
    <section className="border-b border-zinc-800 bg-[#0f1216] px-5 py-4 md:px-8">
      <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
        <div>
          <h2 className="text-sm font-semibold text-slate-100">Trusted channels</h2>
          <p className="mt-1 text-sm text-slate-500">
            Local whitelist for high-quality video discovery. No YouTube auth or Google account access is used.
          </p>
          {sourceFile && <p className="mt-1 text-xs text-slate-500">{sourceFile}</p>}
        </div>
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
          <label className="relative block sm:w-72">
            <Search className="pointer-events-none absolute left-3 top-2.5 h-4 w-4 text-slate-500" />
            <input
              className="focus-ring h-9 w-full rounded-md border border-zinc-800 bg-[#11151a] pl-9 pr-3 text-sm text-slate-100 placeholder:text-slate-600"
              placeholder="Search trusted channels"
              value={search}
              onChange={(event) => onSearch(event.target.value)}
            />
          </label>
          <button
            className="focus-ring inline-flex h-9 items-center justify-center rounded-md border border-cyan-800 bg-cyan-950/40 px-3 text-sm font-medium text-cyan-100 hover:bg-cyan-900/50 disabled:cursor-not-allowed disabled:opacity-60"
            type="button"
            disabled={importPending}
            onClick={onImport}
          >
            <RefreshCw className={`mr-2 h-4 w-4 ${importPending ? 'animate-spin' : ''}`} />
            {importPending ? 'Importing channels' : 'Import local channels'}
          </button>
        </div>
      </div>

      {lastImport && (
        <p className="mt-3 text-xs text-slate-400">
          Channels: {lastImport.channelsDiscovered} found, {lastImport.channelsCreated} added,{' '}
          {lastImport.channelsUpdated} refreshed, {lastImport.channelsSkipped} skipped.
        </p>
      )}

      <div className="mt-4 overflow-x-auto">
        {loading ? (
          <EmptyState title="Loading trusted channels" detail="Reading channel whitelist from SQLite." />
        ) : channels.length === 0 ? (
          <EmptyState title="No trusted channels imported" detail="Import the local channel CSV to seed your trusted source pool." />
        ) : (
          <div className="flex gap-2 pb-1">
            {channels.slice(0, 18).map((channel) => (
              <a
                key={channel.id}
                className="focus-ring min-w-64 rounded-md border border-zinc-800 bg-[#11151a] px-3 py-3 hover:border-zinc-700"
                href={channel.url}
                rel="noreferrer"
                target="_blank"
              >
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <div className="line-clamp-1 text-sm font-medium text-slate-100">{channel.name}</div>
                    <div className="mt-1 text-xs text-slate-500">{channel.priority} priority</div>
                  </div>
                  <ExternalLink className="mt-0.5 h-4 w-4 shrink-0 text-slate-500" />
                </div>
                <div className="mt-2 flex flex-wrap gap-1.5">
                  {channel.tags.slice(0, 3).map((tag) => (
                    <Badge key={tag}>{tag}</Badge>
                  ))}
                </div>
              </a>
            ))}
          </div>
        )}
      </div>
    </section>
  )
}
