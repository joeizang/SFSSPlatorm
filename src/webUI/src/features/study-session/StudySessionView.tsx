import { useEffect, useState } from 'react'
import { CheckCircle2, Eye, RotateCcw, Send, Zap } from 'lucide-react'
import { EmptyState, Metric } from '../../components/shared'
import { studyItemKindLabel } from '../study-items/studyItemLabels'
import type { StudyAttemptRating, StudySessionItemResponse, StudySessionSummaryResponse } from './types'

type StudySessionViewProps = {
  attemptPending: boolean
  error: Error | null
  item: StudySessionItemResponse | null
  loading: boolean
  onSubmitAttempt: (answer: string, rating: StudyAttemptRating) => void
  onSkip: () => void
  summary: StudySessionSummaryResponse | undefined
}

const ratings: Array<{ value: StudyAttemptRating; label: string; description: string }> = [
  { value: 'again', label: 'Again', description: 'Could not recall it.' },
  { value: 'hard', label: 'Hard', description: 'Got it with effort.' },
  { value: 'good', label: 'Good', description: 'Mostly correct.' },
  { value: 'easy', label: 'Easy', description: 'Immediate recall.' },
]

export function StudySessionView({
  attemptPending,
  error,
  item,
  loading,
  onSubmitAttempt,
  onSkip,
  summary,
}: StudySessionViewProps) {
  const [answer, setAnswer] = useState('')
  const [revealed, setRevealed] = useState(false)
  const [rating, setRating] = useState<StudyAttemptRating>('good')

  useEffect(() => {
    setAnswer('')
    setRevealed(false)
    setRating('good')
  }, [item?.id])

  return (
    <section className="min-w-0">
      <header className="border-b border-zinc-800 bg-[#0f1216] px-5 py-4 md:px-8">
        <div>
          <h1 className="text-xl font-semibold tracking-tight text-slate-50">Study session</h1>
          <p className="mt-1 text-sm text-slate-500">Answer saved study items, reveal the expected answer, then rate recall quality.</p>
        </div>
      </header>

      <div className="grid gap-0 xl:grid-cols-[minmax(0,1fr)_320px]">
        <div className="min-w-0 px-5 py-5 md:px-8">
          {loading ? (
            <EmptyState title="Loading session" detail="Finding the next due study item." />
          ) : error ? (
            <EmptyState title="Session unavailable" detail={error.message} />
          ) : !item ? (
            <EmptyState title="No due items" detail="Generate and save study items from source chunks, or come back when reviews are due." />
          ) : (
            <article className="max-w-4xl">
              <div className="mb-4 flex flex-wrap items-center gap-2">
                <span className="rounded border border-cyan-800 bg-cyan-950/30 px-2 py-1 text-xs font-medium text-cyan-100">
                  {studyItemKindLabel(item.kind)}
                </span>
                <span className="rounded border border-zinc-800 px-2 py-1 text-xs text-slate-400">
                  Attempts: {item.attemptCount}
                </span>
                <span className="rounded border border-zinc-800 px-2 py-1 text-xs text-slate-400">
                  Confidence: {item.confidenceScore}/10
                </span>
              </div>

              <div className="rounded-md border border-zinc-800 bg-[#0f1216] p-5">
                <div className="text-xs text-slate-500">{item.sourceTitle} · pages {item.startPage}-{item.endPage}</div>
                <h2 className="mt-3 text-xl font-semibold leading-8 text-slate-50">{item.prompt}</h2>
                <label className="mt-5 block">
                  <span className="text-xs font-medium uppercase tracking-wide text-slate-500">Your answer</span>
                  <textarea
                    className="focus-ring mt-2 min-h-36 w-full resize-y rounded-md border border-zinc-700 bg-[#0b0d10] px-3 py-3 text-sm leading-6 text-slate-100 placeholder:text-slate-600"
                    placeholder="Answer from memory before revealing the expected answer."
                    value={answer}
                    onChange={(event) => setAnswer(event.target.value)}
                  />
                </label>

                {!revealed ? (
                  <button
                    className="focus-ring mt-4 inline-flex h-9 items-center rounded-md bg-cyan-500 px-3 text-sm font-medium text-slate-950 hover:bg-cyan-400"
                    type="button"
                    onClick={() => setRevealed(true)}
                  >
                    <Eye className="mr-2 h-4 w-4" />
                    Reveal answer
                  </button>
                ) : (
                  <ReviewPanel
                    answer={answer}
                    attemptPending={attemptPending}
                    item={item}
                    onSubmit={() => onSubmitAttempt(answer, rating)}
                    rating={rating}
                    setRating={setRating}
                  />
                )}
              </div>
            </article>
          )}
        </div>

        <aside className="border-t border-zinc-800 bg-[#0f1216] px-5 py-5 xl:border-l xl:border-t-0">
          <div className="grid grid-cols-2 gap-3 xl:grid-cols-1">
            <Metric icon={<Zap className="h-4 w-4" />} label="Due" value={String(summary?.dueItems ?? 0)} />
            <Metric icon={<CheckCircle2 className="h-4 w-4" />} label="Answered" value={String(summary?.answeredToday ?? 0)} />
            <Metric icon={<RotateCcw className="h-4 w-4" />} label="Weak" value={String(summary?.weakItems ?? 0)} />
            <Metric icon={<Send className="h-4 w-4" />} label="Active" value={String(summary?.activeItems ?? 0)} />
          </div>
          <button
            className="focus-ring mt-5 h-9 w-full rounded-md border border-zinc-700 text-sm text-slate-300 hover:bg-zinc-900 disabled:cursor-not-allowed disabled:opacity-60"
            type="button"
            disabled={loading}
            onClick={onSkip}
          >
            Load next due item
          </button>
        </aside>
      </div>
    </section>
  )
}

function ReviewPanel({
  answer,
  attemptPending,
  item,
  onSubmit,
  rating,
  setRating,
}: {
  answer: string
  attemptPending: boolean
  item: StudySessionItemResponse
  onSubmit: () => void
  rating: StudyAttemptRating
  setRating: (rating: StudyAttemptRating) => void
}) {
  return (
    <div className="mt-5 border-t border-zinc-800 pt-5">
      <div className="grid gap-4 lg:grid-cols-2">
        <section>
          <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Expected answer</h3>
          <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-slate-200">{item.expectedAnswer}</p>
        </section>
        <section>
          <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Explanation</h3>
          <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-slate-300">{item.explanation}</p>
        </section>
      </div>

      <div className="mt-5 rounded-md border border-zinc-800 bg-[#11151a] p-3">
        <div className="text-xs font-semibold uppercase tracking-wide text-slate-500">Source excerpt</div>
        <p className="mt-2 line-clamp-6 text-xs leading-5 text-slate-400">{item.sourceExcerpt}</p>
      </div>

      <div className="mt-5 grid gap-2 sm:grid-cols-4">
        {ratings.map((option) => (
          <button
            key={option.value}
            className={`focus-ring rounded-md border px-3 py-2 text-left ${
              rating === option.value ? 'border-cyan-700 bg-cyan-950/30' : 'border-zinc-800 bg-[#11151a] hover:border-zinc-700'
            }`}
            type="button"
            onClick={() => setRating(option.value)}
          >
            <div className="text-sm font-medium text-slate-100">{option.label}</div>
            <div className="mt-1 text-xs leading-4 text-slate-500">{option.description}</div>
          </button>
        ))}
      </div>

      <button
        className="focus-ring mt-5 inline-flex h-9 items-center rounded-md bg-cyan-500 px-3 text-sm font-medium text-slate-950 hover:bg-cyan-400 disabled:cursor-not-allowed disabled:opacity-60"
        type="button"
        disabled={attemptPending || !answer.trim()}
        onClick={onSubmit}
      >
        <Send className="mr-2 h-4 w-4" />
        {attemptPending ? 'Recording' : 'Record attempt'}
      </button>
    </div>
  )
}
