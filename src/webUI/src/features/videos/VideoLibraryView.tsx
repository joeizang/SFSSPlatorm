import { useEffect, useMemo, useState } from 'react'
import { Check, ExternalLink, Film, RefreshCw, Save, Search } from 'lucide-react'
import { Badge, EmptyState } from '../../components/shared'
import type { TopicSearchResponse } from '../catalog/types'
import { TrustedChannelsPanel } from './TrustedChannelsPanel'
import { VideoCandidateQueuePanel } from './VideoCandidateQueuePanel'
import type {
  VideoCandidateImportResult,
  VideoCandidateResponse,
  VideoCandidateStatus,
  LearningResourceImportResult,
  LearningResourceResponse,
  TrustedYouTubeChannelImportResult,
  TrustedYouTubeChannelResponse,
} from './types'

type VideoLibraryViewProps = {
  channelImportPending: boolean
  channelLoading: boolean
  channelSearch: string
  channelSourceFile: string | null
  channels: TrustedYouTubeChannelResponse[]
  candidateImportPending: boolean
  candidateLoading: boolean
  candidateSearch: string
  candidateSourceFile: string | null
  candidateStatus: VideoCandidateStatus | 'all'
  candidateUpdating: boolean
  candidates: VideoCandidateResponse[]
  error: Error | null
  importPending: boolean
  lastCandidateImport: VideoCandidateImportResult | null
  lastChannelImport: TrustedYouTubeChannelImportResult | null
  lastImport: LearningResourceImportResult | null
  loading: boolean
  onCandidateAccept: (candidateId: number) => void
  onCandidateImport: () => void
  onCandidateLinkToTopic: (candidateId: number) => void
  onCandidateReject: (candidateId: number) => void
  onCandidateSearch: (value: string) => void
  onCandidateStatusChange: (value: VideoCandidateStatus | 'all') => void
  onChannelImport: () => void
  onChannelSearch: (value: string) => void
  onImport: () => void
  onSaveWatchState: (resourceId: number, isWatched: boolean, watchProgressSeconds: number, notes: string) => void
  onSelectedTopicChange: (topicSlug: string) => void
  onSelectedVideoLinkToTopic: (resourceId: number) => void
  selectedTopicSlug: string
  saving: boolean
  topicLinking: boolean
  topics: TopicSearchResponse[]
  videos: LearningResourceResponse[]
}

export function VideoLibraryView({
  channelImportPending,
  channelLoading,
  channelSearch,
  channelSourceFile,
  channels,
  candidateImportPending,
  candidateLoading,
  candidateSearch,
  candidateSourceFile,
  candidateStatus,
  candidateUpdating,
  candidates,
  error,
  importPending,
  lastCandidateImport,
  lastChannelImport,
  lastImport,
  loading,
  onCandidateAccept,
  onCandidateImport,
  onCandidateLinkToTopic,
  onCandidateReject,
  onCandidateSearch,
  onCandidateStatusChange,
  onChannelImport,
  onChannelSearch,
  onImport,
  onSaveWatchState,
  onSelectedTopicChange,
  onSelectedVideoLinkToTopic,
  selectedTopicSlug,
  saving,
  topicLinking,
  topics,
  videos,
}: VideoLibraryViewProps) {
  const [search, setSearch] = useState('')
  const [selectedTag, setSelectedTag] = useState('all')
  const [selectedVideoId, setSelectedVideoId] = useState<number | null>(null)
  const [notes, setNotes] = useState('')
  const [progressMinutes, setProgressMinutes] = useState('')
  const [isWatched, setIsWatched] = useState(false)

  const tags = useMemo(() => {
    return Array.from(new Set(videos.flatMap((video) => video.tags))).sort((a, b) => a.localeCompare(b))
  }, [videos])

  const filteredVideos = useMemo(() => {
    const value = search.trim().toLowerCase()

    return videos.filter((video) => {
      const matchesSearch =
        !value ||
        [video.title, video.creator, video.summary, ...video.tags].some((field) => field.toLowerCase().includes(value))
      const matchesTag = selectedTag === 'all' || video.tags.includes(selectedTag)
      return matchesSearch && matchesTag
    })
  }, [search, selectedTag, videos])

  useEffect(() => {
    if (selectedVideoId !== null && videos.some((video) => video.id === selectedVideoId)) return
    setSelectedVideoId(videos[0]?.id ?? null)
  }, [selectedVideoId, videos])

  const selectedVideo = videos.find((video) => video.id === selectedVideoId) ?? null

  useEffect(() => {
    setNotes(selectedVideo?.notes ?? '')
    setProgressMinutes(selectedVideo ? String(Math.floor(selectedVideo.watchProgressSeconds / 60)) : '')
    setIsWatched(selectedVideo?.isWatched ?? false)
  }, [selectedVideo])

  return (
    <section className="min-w-0">
      <header className="border-b border-zinc-800 bg-[#0f1216] px-5 py-4 md:px-8">
        <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
          <div>
            <h1 className="text-xl font-semibold tracking-tight text-slate-50">Video library</h1>
            <p className="mt-1 text-sm text-slate-500">
              Curated lessons embedded in the workspace, with watch state and notes stored locally.
            </p>
          </div>
          <button
            className="focus-ring inline-flex h-9 items-center justify-center rounded-md bg-cyan-500 px-3 text-sm font-medium text-slate-950 hover:bg-cyan-400 disabled:cursor-not-allowed disabled:opacity-60"
            type="button"
            disabled={importPending}
            onClick={onImport}
          >
            <RefreshCw className={`mr-2 h-4 w-4 ${importPending ? 'animate-spin' : ''}`} />
            {importPending ? 'Importing videos' : 'Import curated videos'}
          </button>
        </div>
        {lastImport && (
          <p className="mt-3 text-xs text-slate-400">
            Curated set: {lastImport.resourcesDiscovered} found, {lastImport.resourcesCreated} added,{' '}
            {lastImport.resourcesUpdated} refreshed.
          </p>
        )}
        {error && <p className="mt-3 text-sm text-red-300">Video feature failed: {error.message}</p>}
      </header>
      <TrustedChannelsPanel
        channels={channels}
        importPending={channelImportPending}
        lastImport={lastChannelImport}
        loading={channelLoading}
        onImport={onChannelImport}
        onSearch={onChannelSearch}
        search={channelSearch}
        sourceFile={channelSourceFile}
      />
      <VideoCandidateQueuePanel
        candidates={candidates}
        importPending={candidateImportPending}
        lastImport={lastCandidateImport}
        loading={candidateLoading}
        onAccept={onCandidateAccept}
        onImport={onCandidateImport}
        onLinkToTopic={onCandidateLinkToTopic}
        onReject={onCandidateReject}
        onSearch={onCandidateSearch}
        onSelectedTopicChange={onSelectedTopicChange}
        onStatusChange={onCandidateStatusChange}
        search={candidateSearch}
        selectedTopicSlug={selectedTopicSlug}
        sourceFile={candidateSourceFile}
        status={candidateStatus}
        topicLinking={topicLinking}
        topics={topics}
        updating={candidateUpdating}
      />

      <div className="grid min-h-[calc(100vh-553px)] xl:grid-cols-[360px_minmax(0,1fr)_360px]">
        <VideoList
          filteredVideos={filteredVideos}
          loading={loading}
          onSearch={setSearch}
          onSelectTag={setSelectedTag}
          onSelectVideo={setSelectedVideoId}
          search={search}
          selectedTag={selectedTag}
          selectedVideoId={selectedVideoId}
          tags={tags}
        />
        <VideoPlayerPane selectedVideo={selectedVideo} />
        <VideoNotesPane
          isWatched={isWatched}
          notes={notes}
          onChangeNotes={setNotes}
          onChangeProgressMinutes={setProgressMinutes}
          onChangeWatched={setIsWatched}
          onLinkToTopic={() => {
            if (!selectedVideo) return
            onSelectedVideoLinkToTopic(selectedVideo.id)
          }}
          onSave={() => {
            if (!selectedVideo) return
            onSaveWatchState(selectedVideo.id, isWatched, Math.max(0, Number(progressMinutes || 0) * 60), notes)
          }}
          progressMinutes={progressMinutes}
          selectedTopicSlug={selectedTopicSlug}
          saving={saving}
          selectedVideo={selectedVideo}
          topicLinking={topicLinking}
        />
      </div>
    </section>
  )
}

function VideoList({
  filteredVideos,
  loading,
  onSearch,
  onSelectTag,
  onSelectVideo,
  search,
  selectedTag,
  selectedVideoId,
  tags,
}: {
  filteredVideos: LearningResourceResponse[]
  loading: boolean
  onSearch: (value: string) => void
  onSelectTag: (value: string) => void
  onSelectVideo: (id: number) => void
  search: string
  selectedTag: string
  selectedVideoId: number | null
  tags: string[]
}) {
  return (
    <section className="border-b border-zinc-800 px-5 py-5 xl:border-b-0 xl:border-r">
      <label className="relative block">
        <Search className="pointer-events-none absolute left-3 top-2.5 h-4 w-4 text-slate-500" />
        <input
          className="focus-ring h-9 w-full rounded-md border border-zinc-800 bg-[#11151a] pl-9 pr-3 text-sm text-slate-100 placeholder:text-slate-600"
          placeholder="Search videos"
          value={search}
          onChange={(event) => onSearch(event.target.value)}
        />
      </label>

      <div className="mt-4 flex gap-2 overflow-x-auto pb-1">
        <TagButton active={selectedTag === 'all'} label="All" onClick={() => onSelectTag('all')} />
        {tags.map((tag) => (
          <TagButton key={tag} active={selectedTag === tag} label={tag} onClick={() => onSelectTag(tag)} />
        ))}
      </div>

      <div className="mt-4 space-y-2">
        {loading ? (
          <EmptyState title="Loading videos" detail="Reading embedded lessons from SQLite." />
        ) : filteredVideos.length === 0 ? (
          <EmptyState title="No videos available" detail="Import the curated set or adjust the current filter." />
        ) : (
          filteredVideos.map((video) => (
            <button
              key={video.id}
              className={`focus-ring w-full rounded-md border px-3 py-3 text-left ${
                selectedVideoId === video.id
                  ? 'border-cyan-700 bg-cyan-950/20'
                  : 'border-zinc-800 bg-[#0f1216] hover:border-zinc-700 hover:bg-[#11151a]'
              }`}
              type="button"
              onClick={() => onSelectVideo(video.id)}
            >
              <div className="flex items-start gap-3">
                <Film className="mt-0.5 h-4 w-4 shrink-0 text-slate-500" />
                <div className="min-w-0">
                  <div className="line-clamp-2 text-sm font-medium leading-5 text-slate-100">{video.title}</div>
                  <div className="mt-1 text-xs text-slate-500">{video.creator}</div>
                  <div className="mt-2 flex flex-wrap gap-1.5">
                    {video.isWatched && <Badge>watched</Badge>}
                    {video.tags.slice(0, 3).map((tag) => (
                      <Badge key={tag}>{tag}</Badge>
                    ))}
                  </div>
                </div>
              </div>
            </button>
          ))
        )}
      </div>
    </section>
  )
}

function VideoPlayerPane({ selectedVideo }: { selectedVideo: LearningResourceResponse | null }) {
  return (
    <section className="min-w-0 border-b border-zinc-800 bg-[#0b0d10] xl:border-b-0 xl:border-r">
      {!selectedVideo ? (
        <div className="p-5">
          <EmptyState title="Select a video" detail="Choose a lesson to watch without leaving the workspace." />
        </div>
      ) : (
        <>
          <div className="aspect-video border-b border-zinc-800 bg-black">
            <iframe
              allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
              allowFullScreen
              className="h-full w-full"
              referrerPolicy="strict-origin-when-cross-origin"
              src={selectedVideo.embedUrl}
              title={selectedVideo.title}
            />
          </div>
          <div className="px-5 py-5 md:px-8">
            <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
              <div>
                <h2 className="text-lg font-semibold leading-7 text-slate-50">{selectedVideo.title}</h2>
                <p className="mt-1 text-sm text-slate-400">{selectedVideo.creator}</p>
              </div>
              <a
                className="focus-ring inline-flex h-9 shrink-0 items-center justify-center rounded-md border border-zinc-700 px-3 text-sm text-slate-200 hover:bg-zinc-900"
                href={selectedVideo.url}
                rel="noreferrer"
                target="_blank"
              >
                <ExternalLink className="mr-2 h-4 w-4" />
                Open on YouTube
              </a>
            </div>
            <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-300">{selectedVideo.summary}</p>
            <div className="mt-4 flex flex-wrap gap-1.5">
              {selectedVideo.tags.map((tag) => (
                <Badge key={tag}>{tag}</Badge>
              ))}
            </div>
          </div>
        </>
      )}
    </section>
  )
}

function VideoNotesPane({
  isWatched,
  notes,
  onChangeNotes,
  onChangeProgressMinutes,
  onChangeWatched,
  onLinkToTopic,
  onSave,
  progressMinutes,
  selectedTopicSlug,
  saving,
  selectedVideo,
  topicLinking,
}: {
  isWatched: boolean
  notes: string
  onChangeNotes: (value: string) => void
  onChangeProgressMinutes: (value: string) => void
  onChangeWatched: (value: boolean) => void
  onLinkToTopic: () => void
  onSave: () => void
  progressMinutes: string
  selectedTopicSlug: string
  saving: boolean
  selectedVideo: LearningResourceResponse | null
  topicLinking: boolean
}) {
  return (
    <aside className="bg-[#0f1216] px-5 py-5">
      <h2 className="text-sm font-semibold text-slate-100">Study notes</h2>
      {!selectedVideo ? (
        <p className="mt-2 text-sm text-slate-500">Select a video to capture notes and progress.</p>
      ) : (
        <div className="mt-4 space-y-4">
          <label className="flex items-center justify-between gap-3 rounded-md border border-zinc-800 bg-[#11151a] px-3 py-2">
            <span className="text-sm text-slate-300">Watched</span>
            <input
              checked={isWatched}
              className="h-4 w-4 accent-cyan-400"
              type="checkbox"
              onChange={(event) => onChangeWatched(event.target.checked)}
            />
          </label>

          <label className="block">
            <span className="text-xs font-medium uppercase tracking-wide text-slate-500">Progress minutes</span>
            <input
              className="focus-ring mt-2 h-9 w-full rounded-md border border-zinc-800 bg-[#11151a] px-3 text-sm text-slate-100"
              min="0"
              type="number"
              value={progressMinutes}
              onChange={(event) => onChangeProgressMinutes(event.target.value)}
            />
          </label>

          <label className="block">
            <span className="text-xs font-medium uppercase tracking-wide text-slate-500">Notes</span>
            <textarea
              className="focus-ring mt-2 min-h-64 w-full resize-y rounded-md border border-zinc-800 bg-[#11151a] px-3 py-3 text-sm leading-6 text-slate-100 placeholder:text-slate-600"
              placeholder="Capture useful explanations, timestamps, and follow-up questions."
              value={notes}
              onChange={(event) => onChangeNotes(event.target.value)}
            />
          </label>

          <button
            className="focus-ring inline-flex h-9 w-full items-center justify-center rounded-md bg-cyan-500 px-3 text-sm font-medium text-slate-950 hover:bg-cyan-400 disabled:cursor-not-allowed disabled:opacity-60"
            type="button"
            disabled={saving}
            onClick={onSave}
          >
            {saving ? <RefreshCw className="mr-2 h-4 w-4 animate-spin" /> : <Save className="mr-2 h-4 w-4" />}
            Save video notes
          </button>

          <button
            className="focus-ring inline-flex h-9 w-full items-center justify-center rounded-md border border-cyan-800 px-3 text-sm font-medium text-cyan-100 hover:bg-cyan-950/40 disabled:cursor-not-allowed disabled:opacity-60"
            type="button"
            disabled={topicLinking || !selectedTopicSlug}
            onClick={onLinkToTopic}
          >
            Link selected topic
          </button>

          {selectedVideo.isWatched && (
            <div className="flex items-center gap-2 text-sm text-emerald-300">
              <Check className="h-4 w-4" />
              Saved as watched
            </div>
          )}
        </div>
      )}
    </aside>
  )
}

function TagButton({ active, label, onClick }: { active: boolean; label: string; onClick: () => void }) {
  return (
    <button
      className={`focus-ring shrink-0 rounded-md border px-2.5 py-1.5 text-xs ${
        active ? 'border-cyan-700 bg-cyan-950/30 text-cyan-100' : 'border-zinc-800 text-slate-400 hover:bg-zinc-900'
      }`}
      type="button"
      onClick={onClick}
    >
      {label}
    </button>
  )
}
