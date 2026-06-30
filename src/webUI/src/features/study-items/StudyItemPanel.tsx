import { useEffect, useState } from 'react'
import { Check, PencilLine, Sparkles } from 'lucide-react'
import { EmptyState } from '../../components/shared'
import { studyItemKindLabel } from './studyItemLabels'
import type { StudyItemDraftResponse, StudyItemKind, StudyItemResponse } from './types'

type StudyItemPanelProps = {
  chunkId: number | null
  generatedItems: StudyItemDraftResponse[]
  generating: boolean
  onGenerate: () => void
  onSave: (items: StudyItemDraftResponse[]) => void
  savedItems: StudyItemResponse[]
  savedItemsLoading: boolean
  saving: boolean
  studyError: Error | null
}

export function StudyItemPanel({
  chunkId,
  generatedItems,
  generating,
  onGenerate,
  onSave,
  savedItems,
  savedItemsLoading,
  saving,
  studyError,
}: StudyItemPanelProps) {
  const [drafts, setDrafts] = useState<StudyItemDraftResponse[]>([])

  useEffect(() => {
    setDrafts(generatedItems)
  }, [generatedItems])

  const hasDrafts = drafts.length > 0

  return (
    <aside className="border-t border-zinc-800 bg-[#0f1216] px-5 py-5 xl:border-l xl:border-t-0">
      <div className="flex items-center justify-between gap-3">
        <div>
          <h2 className="text-sm font-semibold text-slate-100">Study builder</h2>
          <p className="mt-1 text-xs leading-5 text-slate-500">Generate, edit, and save active-recall items from this chunk.</p>
        </div>
        <button
          className="focus-ring inline-flex h-8 shrink-0 items-center rounded-md border border-cyan-700 bg-cyan-950/30 px-2.5 text-xs font-medium text-cyan-100 hover:bg-cyan-900/40 disabled:cursor-not-allowed disabled:opacity-60"
          type="button"
          disabled={!chunkId || generating}
          onClick={onGenerate}
        >
          <Sparkles className={`mr-1.5 h-3.5 w-3.5 ${generating ? 'animate-pulse' : ''}`} />
          {generating ? 'Generating' : 'Generate'}
        </button>
      </div>

      {studyError && <p className="mt-3 text-xs leading-5 text-red-300">{studyError.message}</p>}

      <section className="mt-5">
        <div className="mb-2 flex items-center justify-between">
          <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Drafts</h3>
          {hasDrafts && (
            <button
              className="focus-ring inline-flex h-8 items-center rounded-md bg-cyan-500 px-2.5 text-xs font-medium text-slate-950 hover:bg-cyan-400 disabled:cursor-not-allowed disabled:opacity-60"
              type="button"
              disabled={saving}
              onClick={() => onSave(drafts)}
            >
              <Check className="mr-1.5 h-3.5 w-3.5" />
              {saving ? 'Saving' : `Save ${drafts.length}`}
            </button>
          )}
        </div>

        {!chunkId ? (
          <EmptyState title="No chunk selected" detail="Select a page chunk before generating study items." />
        ) : !hasDrafts ? (
          <EmptyState title="No drafts yet" detail="Generate study items, review the wording, then save the useful ones." />
        ) : (
          <div className="space-y-3">
            {drafts.map((draft, index) => (
              <DraftEditor
                key={`${draft.kind}-${index}`}
                draft={draft}
                onChange={(next) => {
                  setDrafts((current) => current.map((item, itemIndex) => (itemIndex === index ? next : item)))
                }}
                onRemove={() => setDrafts((current) => current.filter((_, itemIndex) => itemIndex !== index))}
              />
            ))}
          </div>
        )}
      </section>

      <section className="mt-6">
        <div className="mb-2 flex items-center justify-between">
          <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Saved for this chunk</h3>
          <span className="text-xs text-slate-500">{savedItems.length}</span>
        </div>

        {savedItemsLoading ? (
          <EmptyState title="Loading saved items" detail="Reading saved study items for this chunk." />
        ) : savedItems.length === 0 ? (
          <EmptyState title="Nothing saved" detail="Saved questions and exercises will appear here." />
        ) : (
          <div className="space-y-2">
            {savedItems.map((item) => (
              <article key={item.id} className="rounded-md border border-zinc-800 bg-[#11151a] p-3">
                <div className="mb-2 flex items-center justify-between gap-3">
                  <span className="rounded border border-zinc-700 px-1.5 py-0.5 text-xs text-slate-300">
                    {studyItemKindLabel(item.kind)}
                  </span>
                  <span className="text-xs text-slate-500">{item.status}</span>
                </div>
                <p className="text-sm font-medium leading-5 text-slate-100">{item.prompt}</p>
                <p className="mt-2 line-clamp-3 text-xs leading-5 text-slate-500">{item.expectedAnswer}</p>
              </article>
            ))}
          </div>
        )}
      </section>
    </aside>
  )
}

function DraftEditor({
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
        <label className="sr-only">Study item type</label>
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

      <Field label="Prompt" value={draft.prompt} onChange={(prompt) => onChange({ ...draft, prompt })} />
      <Field
        label="Expected answer"
        value={draft.expectedAnswer}
        rows={4}
        onChange={(expectedAnswer) => onChange({ ...draft, expectedAnswer })}
      />
      <Field label="Explanation" value={draft.explanation} rows={3} onChange={(explanation) => onChange({ ...draft, explanation })} />

      <div className="mt-3 border-t border-zinc-800 pt-3">
        <div className="mb-1 flex items-center gap-1.5 text-xs font-medium text-slate-500">
          <PencilLine className="h-3.5 w-3.5" />
          Source excerpt
        </div>
        <p className="line-clamp-4 text-xs leading-5 text-slate-400">{draft.sourceExcerpt}</p>
      </div>
    </article>
  )
}

function Field({
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
