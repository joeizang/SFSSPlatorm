import { Check, CheckCircle2, Code2, ExternalLink, Maximize2, Minimize2, Move, NotebookTabs, Play, Save, Sparkles, X } from 'lucide-react'
import { useEffect, useMemo, useRef, useState, type CSSProperties, type PointerEvent as ReactPointerEvent, type ReactNode } from 'react'
import { Badge, EmptyState } from '../../components/shared'
import { SandboxedDesignPreview } from '../../components/SandboxedDesignPreview'
import type { TaskType, TopicDetailResponse, TopicNoteResponse, TopicProgressStatus, TopicSearchResponse } from '../catalog/types'
import { studyItemKindLabel } from '../study-items/studyItemLabels'
import type { CodingExerciseResponse, StudyItemDraftResponse, StudyItemKind, StudyItemResponse } from '../study-items/types'
import type { TopicResourceLinkResponse } from '../videos/types'
import { ExerciseCodeEditor } from './ExerciseCodeEditor'

type TopicWorkspaceViewProps = {
  error: Error | null
  exerciseGeneratePending: boolean
  exercises: CodingExerciseResponse[]
  exercisesLoading: boolean
  generatedStudyItems: StudyItemDraftResponse[]
  generatingStudyItems: boolean
  links: TopicResourceLinkResponse[]
  loading: boolean
  note: TopicNoteResponse | null
  noteSaving: boolean
  onGenerateExercise: () => void
  onGenerateStudyItems: () => void
  onProgressChange: (slug: string, status: TopicProgressStatus) => void
  onSaveNote: (content: string) => void
  onSaveStudyItems: (items: StudyItemDraftResponse[]) => void
  onSelectTopic: (slug: string) => void
  progressPending: boolean
  savingStudyItems: boolean
  selectedTopicSlug: string
  studyItems: StudyItemResponse[]
  studyItemsLoading: boolean
  topic: TopicDetailResponse | null
  topics: TopicSearchResponse[]
}

export function TopicWorkspaceView({
  error,
  exerciseGeneratePending,
  exercises,
  exercisesLoading,
  generatedStudyItems,
  generatingStudyItems,
  links,
  loading,
  note,
  noteSaving,
  onGenerateExercise,
  onGenerateStudyItems,
  onProgressChange,
  onSaveNote,
  onSaveStudyItems,
  onSelectTopic,
  progressPending,
  savingStudyItems,
  selectedTopicSlug,
  studyItems,
  studyItemsLoading,
  topic,
  topics,
}: TopicWorkspaceViewProps) {
  const [selectedLinkId, setSelectedLinkId] = useState<number | null>(null)
  const [noteDraft, setNoteDraft] = useState('')
  const [notesOpen, setNotesOpen] = useState(true)
  const [focusMode, setFocusMode] = useState(false)
  const [editorPlacement, setEditorPlacement] = useState<'above' | 'below'>('below')

  const topicLinks = useMemo(
    () =>
      links
        .filter((link) => link.topicSlug === selectedTopicSlug)
        .sort((left, right) => right.priority - left.priority || left.title.localeCompare(right.title)),
    [links, selectedTopicSlug],
  )

  const selectedLink = topicLinks.find((link) => link.id === selectedLinkId) ?? topicLinks[0] ?? null

  useEffect(() => {
    setSelectedLinkId(topicLinks[0]?.id ?? null)
  }, [selectedTopicSlug, topicLinks])

  useEffect(() => {
    setNoteDraft(note?.content ?? '')
  }, [note?.content, selectedTopicSlug])

  if (loading) {
    return (
      <section className="min-w-0 px-5 py-5 md:px-8">
        <EmptyState title="Loading topic workspace" detail="Reading the topic, linked videos, and notes from SQLite." />
      </section>
    )
  }

  if (error) {
    return (
      <section className="min-w-0 px-5 py-5 md:px-8">
        <EmptyState title="Topic workspace unavailable" detail={error.message} />
      </section>
    )
  }

  if (!topic) {
    return (
      <section className="min-w-0 px-5 py-5 md:px-8">
        <EmptyState title="No topic selected" detail="Open a topic from Curriculum or choose one from the topic selector." />
      </section>
    )
  }

  return (
    <section className="min-w-0">
      <header className="border-b border-zinc-800 bg-[#0f1216] px-5 py-4 md:px-8">
        <div className="flex flex-col gap-4 xl:flex-row xl:items-start xl:justify-between">
          <div className="min-w-0">
            <div className="text-sm text-slate-500">{topic.moduleTitle}</div>
            <h1 className="mt-1 text-xl font-semibold tracking-tight text-slate-50">{topic.title}</h1>
            <p className="mt-2 max-w-4xl text-sm leading-6 text-slate-400">{topic.summary}</p>
            <div className="mt-3 flex flex-wrap gap-1.5">
              {topic.taskTypes.map((taskType) => (
                <Badge key={taskType}>{taskTypeLabel(taskType)}</Badge>
              ))}
            </div>
          </div>
          <div className="grid gap-2 sm:grid-cols-[auto_220px_150px]">
            <button
              className="focus-ring inline-flex h-9 items-center justify-center rounded-md border border-zinc-700 px-3 text-sm font-medium text-slate-200 hover:bg-zinc-900"
              type="button"
              onClick={() => setNotesOpen(true)}
            >
              <NotebookTabs className="mr-1.5 h-4 w-4" />
              Notes
            </button>
            <select
              className="focus-ring h-9 rounded-md border border-zinc-800 bg-[#11151a] px-2 text-sm text-slate-100"
              value={selectedTopicSlug}
              onChange={(event) => onSelectTopic(event.target.value)}
            >
              {topics.map((item) => (
                <option key={item.slug} value={item.slug}>
                  {item.title}
                </option>
              ))}
            </select>
            <select
              className="focus-ring h-9 rounded-md border border-zinc-800 bg-[#11151a] px-2 text-sm text-slate-100"
              disabled={progressPending}
              value={topic.status}
              onChange={(event) => onProgressChange(topic.slug, event.target.value as TopicProgressStatus)}
            >
              <option value="notStarted">Not started</option>
              <option value="inProgress">In progress</option>
              <option value="done">Done</option>
            </select>
          </div>
        </div>
      </header>

      <div className="min-w-0 px-5 py-5 md:px-8">
        <LearningSurface
          editorPlacement={editorPlacement}
          exerciseGeneratePending={exerciseGeneratePending}
          exercises={exercises}
          exercisesLoading={exercisesLoading}
          links={topicLinks}
          onEditorPlacementChange={() => setEditorPlacement((current) => (current === 'below' ? 'above' : 'below'))}
          onFocusMode={() => {
            setNotesOpen(false)
            setFocusMode(true)
          }}
          onGenerateExercise={onGenerateExercise}
          onSelectLink={setSelectedLinkId}
          questionCount={studyItems.length}
          selectedLink={selectedLink}
        />

        <div className="mt-5">
          <TopicQuestionBank
            drafts={generatedStudyItems}
            generating={generatingStudyItems}
            onGenerate={onGenerateStudyItems}
            onSave={onSaveStudyItems}
            savedItems={studyItems}
            savedItemsLoading={studyItemsLoading}
            saving={savingStudyItems}
          />
        </div>
      </div>

      {notesOpen ? (
        <FloatingTopicNotes
          noteDraft={noteDraft}
          noteSaving={noteSaving}
          onChange={setNoteDraft}
          onClose={() => setNotesOpen(false)}
          onSave={() => onSaveNote(noteDraft)}
        />
      ) : (
        <button
          className="focus-ring fixed bottom-5 right-5 z-50 inline-flex h-10 items-center rounded-md border border-zinc-700 bg-[#10151b] px-3 text-sm font-medium text-slate-100 shadow-2xl shadow-black/40 hover:bg-zinc-900"
          type="button"
          onClick={() => setNotesOpen(true)}
        >
          <NotebookTabs className="mr-2 h-4 w-4" />
          Notes
        </button>
      )}

      {focusMode && (
        <FocusStudyOverlay
          exerciseGeneratePending={exerciseGeneratePending}
          exercises={exercises}
          exercisesLoading={exercisesLoading}
          link={selectedLink}
          onClose={() => setFocusMode(false)}
          onGenerateExercise={onGenerateExercise}
        />
      )}
    </section>
  )
}

function LearningSurface({
  editorPlacement,
  exerciseGeneratePending,
  exercises,
  exercisesLoading,
  links,
  onEditorPlacementChange,
  onFocusMode,
  onGenerateExercise,
  onSelectLink,
  questionCount,
  selectedLink,
}: {
  editorPlacement: 'above' | 'below'
  exerciseGeneratePending: boolean
  exercises: CodingExerciseResponse[]
  exercisesLoading: boolean
  links: TopicResourceLinkResponse[]
  onEditorPlacementChange: () => void
  onFocusMode: () => void
  onGenerateExercise: () => void
  onSelectLink: (id: number) => void
  questionCount: number
  selectedLink: TopicResourceLinkResponse | null
}) {
  const videoPanel = (
    <div className="min-h-[360px] resize-y overflow-auto rounded-md border border-zinc-800 bg-[#0f1216]">
      <TopicVideoPanel link={selectedLink} />
    </div>
  )
  const exercisePanel = (
    <TopicExercisePanel
      editorHeight="clamp(520px, 58vh, 760px)"
      exerciseGeneratePending={exerciseGeneratePending}
      exercises={exercises}
      exercisesLoading={exercisesLoading}
      onGenerateExercise={onGenerateExercise}
    />
  )

  return (
    <section className="rounded-md border border-zinc-800 bg-[#0f1216]">
      <div className="border-b border-zinc-800 px-4 py-3">
        <div className="flex flex-col gap-3 xl:flex-row xl:items-center xl:justify-between">
          <div className="min-w-0">
            <div className="flex flex-wrap items-center gap-2">
              <h2 className="text-sm font-semibold text-slate-100">Linked videos</h2>
              <SurfaceMetric icon={<Play className="h-3.5 w-3.5" />} label="videos" value={links.length} />
              <SurfaceMetric icon={<CheckCircle2 className="h-3.5 w-3.5" />} label="questions" value={questionCount} />
              <SurfaceMetric icon={<Code2 className="h-3.5 w-3.5" />} label="exercises" value={exercises.length} />
            </div>
            <p className="mt-1 text-xs text-slate-500">Video, exercise, packages, diagnostics, and grading live in one adjustable study surface.</p>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <button
              className="focus-ring inline-flex h-8 items-center rounded-md border border-zinc-700 px-2.5 text-xs font-medium text-slate-200 hover:bg-zinc-900"
              type="button"
              onClick={onEditorPlacementChange}
            >
              {editorPlacement === 'below' ? 'Editor above' : 'Editor below'}
            </button>
            <button
              className="focus-ring inline-flex h-8 items-center rounded-md border border-cyan-700 bg-cyan-950/30 px-2.5 text-xs font-medium text-cyan-100 hover:bg-cyan-900/40"
              type="button"
              onClick={onFocusMode}
            >
              <Maximize2 className="mr-1.5 h-3.5 w-3.5" />
              Focus mode
            </button>
          </div>
        </div>

        {links.length === 0 ? (
          <div className="mt-3 rounded-md border border-zinc-800 bg-[#0b0d10] px-3 py-3 text-sm leading-6 text-slate-500">
            No videos are linked to this topic yet. Use the Videos workspace to assign accepted resources or candidates.
          </div>
        ) : (
          <div className="mt-3 flex gap-2 overflow-x-auto pb-1">
            {links.map((link) => (
              <button
                key={link.id}
                className={`focus-ring min-w-[240px] max-w-[320px] rounded-md border px-3 py-2 text-left ${
                  selectedLink?.id === link.id
                    ? 'border-cyan-700 bg-cyan-950/20 text-slate-50'
                    : 'border-zinc-800 bg-[#0b0d10] text-slate-300 hover:bg-zinc-950'
                }`}
                type="button"
                onClick={() => onSelectLink(link.id)}
              >
                <span className="line-clamp-2 text-xs font-semibold leading-5">{link.title}</span>
                <span className="mt-1 block truncate text-xs text-slate-500">{link.creator}</span>
              </button>
            ))}
          </div>
        )}
      </div>

      <div className="grid gap-4 p-4">
        {editorPlacement === 'above' ? (
          <>
            {exercisePanel}
            {videoPanel}
          </>
        ) : (
          <>
            {videoPanel}
            {exercisePanel}
          </>
        )}
      </div>
    </section>
  )
}

function SurfaceMetric({ icon, label, value }: { icon: ReactNode; label: string; value: number }) {
  return (
    <span className="inline-flex items-center gap-1 rounded border border-zinc-800 bg-[#0b0d10] px-2 py-1 text-xs text-slate-400">
      {icon}
      <span className="font-semibold text-slate-100">{value}</span>
      {label}
    </span>
  )
}

function FloatingTopicNotes({
  noteDraft,
  noteSaving,
  onChange,
  onClose,
  onSave,
}: {
  noteDraft: string
  noteSaving: boolean
  onChange: (value: string) => void
  onClose: () => void
  onSave: () => void
}) {
  const [position, setPosition] = useState(() => initialNotesPosition())
  const dragOffset = useRef<{ x: number; y: number } | null>(null)

  const handlePointerDown = (event: ReactPointerEvent<HTMLDivElement>) => {
    dragOffset.current = {
      x: event.clientX - position.x,
      y: event.clientY - position.y,
    }
    event.currentTarget.setPointerCapture?.(event.pointerId)
  }

  const handlePointerMove = (event: ReactPointerEvent<HTMLDivElement>) => {
    if (!dragOffset.current) return
    const width = globalThis.window?.innerWidth ?? 1280
    const height = globalThis.window?.innerHeight ?? 800
    setPosition({
      x: Math.min(Math.max(12, event.clientX - dragOffset.current.x), Math.max(12, width - 160)),
      y: Math.min(Math.max(12, event.clientY - dragOffset.current.y), Math.max(12, height - 80)),
    })
  }

  const handlePointerUp = () => {
    dragOffset.current = null
  }

  return (
    <aside
      className="fixed z-50 flex min-h-[260px] min-w-[300px] resize overflow-auto rounded-md border border-zinc-700 bg-[#10151b] shadow-2xl shadow-black/50"
      style={{ height: 340, left: position.x, top: position.y, width: 390 }}
    >
      <div className="flex min-h-full w-full flex-col">
        <div
          className="flex cursor-move items-center justify-between gap-3 border-b border-zinc-800 px-3 py-2"
          onPointerDown={handlePointerDown}
          onPointerMove={handlePointerMove}
          onPointerUp={handlePointerUp}
        >
          <div className="flex min-w-0 items-center gap-2">
            <Move className="h-3.5 w-3.5 shrink-0 text-slate-500" />
            <h2 className="truncate text-sm font-semibold text-slate-100">Topic notes</h2>
          </div>
          <div className="flex items-center gap-1.5">
            <button
              className="focus-ring inline-flex h-7 items-center rounded-md border border-zinc-700 px-2 text-xs font-medium text-slate-200 hover:bg-zinc-900 disabled:cursor-not-allowed disabled:opacity-60"
              type="button"
              disabled={noteSaving}
              onClick={onSave}
            >
              <Save className="mr-1.5 h-3.5 w-3.5" />
              {noteSaving ? 'Saving' : 'Save'}
            </button>
            <button
              className="focus-ring inline-flex h-7 w-7 items-center justify-center rounded-md text-slate-400 hover:bg-zinc-900 hover:text-slate-100"
              type="button"
              aria-label="Close notes"
              onClick={onClose}
            >
              <X className="h-3.5 w-3.5" />
            </button>
          </div>
        </div>
        <textarea
          className="focus-ring min-h-0 flex-1 resize-none border-0 bg-[#0b0d10] p-3 text-sm leading-6 text-slate-100 placeholder:text-slate-600"
          placeholder="Write the explanation in your own words, capture mistakes, link ideas, and list follow-up exercises."
          value={noteDraft}
          onChange={(event) => onChange(event.target.value)}
        />
        <p className="border-t border-zinc-800 px-3 py-2 text-xs text-slate-600">Saved locally in SQLite for this topic. Drag the title bar and resize from the corner.</p>
      </div>
    </aside>
  )
}

function FocusStudyOverlay({
  exerciseGeneratePending,
  exercises,
  exercisesLoading,
  link,
  onClose,
  onGenerateExercise,
}: {
  exerciseGeneratePending: boolean
  exercises: CodingExerciseResponse[]
  exercisesLoading: boolean
  link: TopicResourceLinkResponse | null
  onClose: () => void
  onGenerateExercise: () => void
}) {
  const [videoPercent, setVideoPercent] = useState(56)
  const containerRef = useRef<HTMLDivElement | null>(null)
  const resizing = useRef(false)

  useEffect(() => {
    const handleMove = (event: PointerEvent) => {
      if (!resizing.current || !containerRef.current) return
      const bounds = containerRef.current.getBoundingClientRect()
      const next = ((event.clientX - bounds.left) / bounds.width) * 100
      setVideoPercent(Math.min(72, Math.max(38, next)))
    }
    const handleUp = () => {
      resizing.current = false
    }
    window.addEventListener('pointermove', handleMove)
    window.addEventListener('pointerup', handleUp)
    return () => {
      window.removeEventListener('pointermove', handleMove)
      window.removeEventListener('pointerup', handleUp)
    }
  }, [])

  return (
    <div className="fixed inset-0 z-40 bg-[#080a0d]">
      <div className="flex h-14 items-center justify-between border-b border-zinc-800 bg-[#0f1216] px-4">
        <div className="min-w-0">
          <div className="text-sm font-semibold text-slate-100">Focus mode</div>
          <div className="text-xs text-slate-500">Drag the divider to rebalance video and editor space.</div>
        </div>
        <button
          className="focus-ring inline-flex h-8 items-center rounded-md border border-zinc-700 px-2.5 text-xs font-medium text-slate-200 hover:bg-zinc-900"
          type="button"
          onClick={onClose}
        >
          <Minimize2 className="mr-1.5 h-3.5 w-3.5" />
          Exit
        </button>
      </div>
      <div
        ref={containerRef}
        className="grid h-[calc(100vh-3.5rem)] min-w-0 grid-cols-1 gap-0 overflow-hidden lg:grid-cols-[var(--video-width)_8px_minmax(0,1fr)]"
        style={{ '--video-width': `${videoPercent}%` } as CSSProperties}
      >
        <div className="min-h-0 overflow-auto border-b border-zinc-800 p-3 lg:border-b-0">
          <TopicVideoPanel link={link} />
        </div>
        <button
          className="hidden cursor-col-resize border-x border-zinc-800 bg-zinc-900 hover:bg-cyan-950/50 lg:block"
          type="button"
          aria-label="Resize video and editor panels"
          onPointerDown={() => {
            resizing.current = true
          }}
        />
        <div className="min-h-0 overflow-auto p-3">
          <TopicExercisePanel
            compact
            editorHeight="calc(100vh - 250px)"
            exerciseGeneratePending={exerciseGeneratePending}
            exercises={exercises}
            exercisesLoading={exercisesLoading}
            onGenerateExercise={onGenerateExercise}
          />
        </div>
      </div>
    </div>
  )
}

function TopicVideoPanel({ link }: { link: TopicResourceLinkResponse | null }) {
  if (!link) {
    return (
      <div className="rounded-md border border-zinc-800 bg-[#0f1216] p-5">
        <EmptyState title="No video selected" detail="Assign a video to this topic from the Videos workspace." />
      </div>
    )
  }

  return (
    <div className="rounded-md border border-zinc-800 bg-[#0f1216]">
      <div className="aspect-video w-full overflow-hidden border-b border-zinc-800 bg-black">
        <iframe
          className="h-full w-full"
          src={link.embedUrl}
          title={link.title}
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
          allowFullScreen
        />
      </div>
      <div className="p-4">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <h2 className="text-base font-semibold text-slate-50">{link.title}</h2>
            <p className="mt-1 text-sm text-slate-500">{link.creator}</p>
          </div>
          <a
            className="focus-ring inline-flex h-8 shrink-0 items-center rounded-md border border-zinc-700 px-2.5 text-xs font-medium text-slate-200 hover:bg-zinc-900"
            href={link.url}
            rel="noreferrer"
            target="_blank"
          >
            <ExternalLink className="mr-1.5 h-3.5 w-3.5" />
            Source
          </a>
        </div>
        {link.notes && <p className="mt-3 text-sm leading-6 text-slate-400">{link.notes}</p>}
        <div className="mt-3 flex flex-wrap gap-1.5">
          {link.tags.map((tag) => (
            <Badge key={tag}>{tag}</Badge>
          ))}
        </div>
      </div>
    </div>
  )
}

function TopicQuestionBank({
  drafts,
  generating,
  onGenerate,
  onSave,
  savedItems,
  savedItemsLoading,
  saving,
}: {
  drafts: StudyItemDraftResponse[]
  generating: boolean
  onGenerate: () => void
  onSave: (items: StudyItemDraftResponse[]) => void
  savedItems: StudyItemResponse[]
  savedItemsLoading: boolean
  saving: boolean
}) {
  const [localDrafts, setLocalDrafts] = useState<StudyItemDraftResponse[]>([])

  useEffect(() => {
    setLocalDrafts(drafts)
  }, [drafts])

  return (
    <section className="rounded-md border border-zinc-800 bg-[#0f1216]">
      <div className="flex items-center justify-between gap-3 border-b border-zinc-800 px-4 py-3">
        <div>
          <h2 className="text-sm font-semibold text-slate-100">Question bank</h2>
          <p className="mt-1 text-xs text-slate-500">Generate topic-level questions from notes, videos, and local source context.</p>
        </div>
        <button
          className="focus-ring inline-flex h-8 shrink-0 items-center rounded-md border border-cyan-700 bg-cyan-950/30 px-2.5 text-xs font-medium text-cyan-100 hover:bg-cyan-900/40 disabled:cursor-not-allowed disabled:opacity-60"
          type="button"
          disabled={generating}
          onClick={onGenerate}
        >
          <Sparkles className={`mr-1.5 h-3.5 w-3.5 ${generating ? 'animate-pulse' : ''}`} />
          {generating ? 'Generating' : 'Generate'}
        </button>
      </div>

      <div className="grid gap-0 lg:grid-cols-2">
        <div className="border-b border-zinc-800 p-4 lg:border-b-0 lg:border-r">
          <div className="mb-3 flex items-center justify-between">
            <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Drafts</h3>
            {localDrafts.length > 0 && (
              <button
                className="focus-ring inline-flex h-8 items-center rounded-md bg-cyan-500 px-2.5 text-xs font-medium text-slate-950 hover:bg-cyan-400 disabled:cursor-not-allowed disabled:opacity-60"
                type="button"
                disabled={saving}
                onClick={() => onSave(localDrafts)}
              >
                <Check className="mr-1.5 h-3.5 w-3.5" />
                {saving ? 'Saving' : `Save ${localDrafts.length}`}
              </button>
            )}
          </div>
          {localDrafts.length === 0 ? (
            <EmptyState title="No drafts" detail="Generate topic questions, edit them, then save the useful set." />
          ) : (
            <div className="space-y-3">
              {localDrafts.map((draft, index) => (
                <TopicDraftEditor
                  key={`${draft.kind}-${index}`}
                  draft={draft}
                  onChange={(next) => {
                    setLocalDrafts((current) => current.map((item, itemIndex) => (itemIndex === index ? next : item)))
                  }}
                  onRemove={() => setLocalDrafts((current) => current.filter((_, itemIndex) => itemIndex !== index))}
                />
              ))}
            </div>
          )}
        </div>

        <div className="p-4">
          <div className="mb-3 flex items-center justify-between">
            <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Saved</h3>
            <span className="text-xs text-slate-500">{savedItems.length}</span>
          </div>
          {savedItemsLoading ? (
            <EmptyState title="Loading questions" detail="Reading saved questions for this topic." />
          ) : savedItems.length === 0 ? (
            <EmptyState title="No saved questions" detail="Saved topic questions and coding prompts will appear here." />
          ) : (
            <div className="space-y-2">
              {savedItems.map((item) => (
                <article key={item.id} className="rounded-md border border-zinc-800 bg-[#11151a] p-3">
                  <div className="mb-2 flex items-center justify-between gap-3">
                    <Badge>{studyItemKindLabel(item.kind)}</Badge>
                    <span className="text-xs text-slate-500">{item.status}</span>
                  </div>
                  <p className="text-sm font-medium leading-5 text-slate-100">{item.prompt}</p>
                  <p className="mt-2 line-clamp-3 text-xs leading-5 text-slate-500">{item.expectedAnswer}</p>
                </article>
              ))}
            </div>
          )}
        </div>
      </div>
    </section>
  )
}

function TopicDraftEditor({
  draft,
  onChange,
  onRemove,
}: {
  draft: StudyItemDraftResponse
  onChange: (draft: StudyItemDraftResponse) => void
  onRemove: () => void
}) {
  return (
    <article className="rounded-md border border-zinc-800 bg-[#11151a] p-3">
      <div className="mb-3 flex items-center justify-between gap-3">
        <select
          className="focus-ring h-8 rounded-md border border-zinc-700 bg-[#0f1216] px-2 text-xs text-slate-100"
          value={draft.kind}
          onChange={(event) => onChange({ ...draft, kind: event.target.value as StudyItemKind })}
        >
          <option value="concept">Concept</option>
          <option value="flashcard">Flashcard</option>
          <option value="shortAnswer">Short answer</option>
          <option value="codeReading">Code reading</option>
          <option value="codingExercise">Coding exercise</option>
        </select>
        <button className="focus-ring rounded-md px-2 py-1 text-xs text-slate-500 hover:bg-zinc-800 hover:text-slate-200" type="button" onClick={onRemove}>
          Remove
        </button>
      </div>
      <TopicField label="Prompt" value={draft.prompt} onChange={(prompt) => onChange({ ...draft, prompt })} />
      <TopicField
        label="Expected answer"
        rows={4}
        value={draft.expectedAnswer}
        onChange={(expectedAnswer) => onChange({ ...draft, expectedAnswer })}
      />
      <TopicField label="Explanation" rows={3} value={draft.explanation} onChange={(explanation) => onChange({ ...draft, explanation })} />
    </article>
  )
}

function TopicExercisePanel({
  compact = false,
  editorHeight,
  exerciseGeneratePending,
  exercises,
  exercisesLoading,
  onGenerateExercise,
}: {
  compact?: boolean
  editorHeight?: string
  exerciseGeneratePending: boolean
  exercises: CodingExerciseResponse[]
  exercisesLoading: boolean
  onGenerateExercise: () => void
}) {
  return (
    <section className="resize-y overflow-auto rounded-md border border-zinc-800 bg-[#0f1216]">
      <div className="flex items-center justify-between gap-3 border-b border-zinc-800 px-4 py-3">
        <div>
          <h2 className="text-sm font-semibold text-slate-100">Coding exercises</h2>
          <p className="mt-1 text-xs text-slate-500">Starter code and package requirements ready for Monaco/Roslyn.</p>
        </div>
        <button
          className="focus-ring inline-flex h-8 shrink-0 items-center rounded-md border border-zinc-700 px-2.5 text-xs font-medium text-slate-200 hover:bg-zinc-900 disabled:cursor-not-allowed disabled:opacity-60"
          type="button"
          disabled={exerciseGeneratePending}
          onClick={onGenerateExercise}
        >
          <Code2 className="mr-1.5 h-3.5 w-3.5" />
          {exerciseGeneratePending ? 'Creating' : 'Create'}
        </button>
      </div>
      <div className="p-4">
        {exercisesLoading ? (
          <EmptyState title="Loading exercises" detail="Reading exercise skeletons for this topic." />
        ) : exercises.length === 0 ? (
          <EmptyState title="No exercise skeletons" detail="Create the first skeleton before adding Monaco and Roslyn execution." />
        ) : (
          <div className="space-y-3">
            {exercises.map((exercise) => (
              <article key={exercise.id} className="rounded-md border border-zinc-800 bg-[#11151a]">
                <div className="border-b border-zinc-800 p-3">
                  <div className="flex flex-wrap items-center gap-2">
                    <h3 className="mr-auto text-sm font-semibold text-slate-100">{exercise.title}</h3>
                    <Badge>{exercise.language}</Badge>
                    <Badge>{exercise.difficulty}</Badge>
                  </div>
                  <p className={`${compact ? 'line-clamp-2' : ''} mt-2 text-sm leading-6 text-slate-400`}>{exercise.prompt}</p>
                </div>
                <div className="space-y-3 p-3">
                  {!compact && (
                    <>
                      <DetailBlock title="Package requirements" value={exercise.packageRequirements} />
                      <DetailBlock title="Success criteria" value={exercise.successCriteria} />
                      <DetailBlock title="Hints" value={exercise.hints} />
                    </>
                  )}
                  <div className={isFrontendExercise(exercise) ? 'grid gap-3 xl:grid-cols-[minmax(0,1fr)_420px]' : ''}>
                    <ExerciseCodeEditor editorHeight={editorHeight} exercise={exercise} />
                    {isFrontendExercise(exercise) && <SandboxedDesignPreview detail={exercise.successCriteria} prompt={exercise.prompt} />}
                  </div>
                </div>
              </article>
            ))}
          </div>
        )}
      </div>
    </section>
  )
}

function TopicField({
  label,
  onChange,
  rows = 2,
  value,
}: {
  label: string
  onChange: (value: string) => void
  rows?: number
  value: string
}) {
  return (
    <label className="mt-3 block">
      <span className="text-xs font-medium text-slate-500">{label}</span>
      <textarea
        className="focus-ring mt-1 w-full resize-y rounded-md border border-zinc-700 bg-[#0f1216] px-2 py-2 text-sm leading-5 text-slate-100 placeholder:text-slate-600"
        rows={rows}
        value={value}
        onChange={(event) => onChange(event.target.value)}
      />
    </label>
  )
}

function DetailBlock({ title, value }: { title: string; value: string }) {
  return (
    <div>
      <div className="text-xs font-semibold uppercase tracking-wide text-slate-500">{title}</div>
      <p className="mt-1 text-xs leading-5 text-slate-400">{value}</p>
    </div>
  )
}

function taskTypeLabel(taskType: TaskType) {
  return taskType === 'writeTests' ? 'write tests' : taskType
}

function isFrontendExercise(exercise: CodingExerciseResponse) {
  const text = `${exercise.topicSlug} ${exercise.title} ${exercise.prompt} ${exercise.language} ${exercise.packageRequirements}`.toLowerCase()
  return text.includes('react') || text.includes('typescript') || text.includes('tailwind') || text.includes('frontend') || text.includes('product interface')
}

function initialNotesPosition() {
  const width = globalThis.window?.innerWidth ?? 1280
  const height = globalThis.window?.innerHeight ?? 800
  return {
    x: Math.max(24, width - 430),
    y: Math.max(220, height - 380),
  }
}
