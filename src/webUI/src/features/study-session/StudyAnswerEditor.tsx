import { Copy, RotateCcw } from 'lucide-react'
import { useState } from 'react'
import { CodeEditorSurface } from '../../components/CodeEditorSurface'
import { normalizeCodeLanguage } from '../../components/codeEditorLanguage'
import { SandboxedDesignPreview } from '../../components/SandboxedDesignPreview'
import type { StudySessionItemResponse } from './types'

type StudyAnswerEditorProps = {
  answer: string
  item: StudySessionItemResponse
  onAnswerChange: (value: string) => void
}

export function StudyAnswerEditor({ answer, item, onAnswerChange }: StudyAnswerEditorProps) {
  const [copied, setCopied] = useState(false)
  const codeMode = isCodeStudyItem(item)
  const previewMode = isFrontendStudyItem(item)
  const language = inferLanguage(item)

  if (!codeMode) {
    return (
      <label className="mt-5 block">
        <span className="text-xs font-medium uppercase tracking-wide text-slate-500">Your answer</span>
        <textarea
          className="focus-ring mt-2 min-h-36 w-full resize-y rounded-md border border-zinc-700 bg-[#0b0d10] px-3 py-3 text-sm leading-6 text-slate-100 placeholder:text-slate-600"
          placeholder="Answer from memory before revealing the expected answer."
          value={answer}
          onChange={(event) => onAnswerChange(event.target.value)}
        />
      </label>
    )
  }

  const copyAnswer = async () => {
    await navigator.clipboard?.writeText(answer)
    setCopied(true)
    window.setTimeout(() => setCopied(false), 1600)
  }

  return (
    <div className="mt-5">
      <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="text-xs font-medium uppercase tracking-wide text-slate-500">Your answer</span>
          <span className="rounded border border-zinc-800 px-1.5 py-0.5 text-xs text-slate-400">{language}</span>
        </div>
        <div className="flex items-center gap-1.5">
          <button
            className="focus-ring inline-flex h-7 items-center rounded-md px-2 text-xs text-slate-400 hover:bg-zinc-900 hover:text-slate-100"
            type="button"
            onClick={() => onAnswerChange('')}
          >
            <RotateCcw className="mr-1.5 h-3.5 w-3.5" />
            Reset
          </button>
          <button
            className="focus-ring inline-flex h-7 items-center rounded-md px-2 text-xs text-slate-400 hover:bg-zinc-900 hover:text-slate-100"
            type="button"
            onClick={copyAnswer}
          >
            <Copy className="mr-1.5 h-3.5 w-3.5" />
            {copied ? 'Copied' : 'Copy'}
          </button>
        </div>
      </div>
      <div className={previewMode ? 'grid gap-4 2xl:grid-cols-[minmax(560px,1fr)_minmax(680px,1fr)]' : ''}>
        <div className="overflow-hidden rounded-md border border-zinc-800 bg-[#080a0d]">
          <textarea
            className="sr-only"
            aria-label="Code answer text"
            value={answer}
            onChange={(event) => onAnswerChange(event.target.value)}
          />
          <CodeEditorSurface editorHeight="min(68vh, 720px)" language={language} tabSize={2} value={answer} onChange={onAnswerChange} />
        </div>
        {previewMode && <SandboxedDesignPreview detail={item.explanation || item.expectedAnswer} height="min(68vh, 720px)" prompt={item.prompt} />}
      </div>
    </div>
  )
}

function isCodeStudyItem(item: StudySessionItemResponse) {
  return item.kind === 'codeReading' || item.kind === 'codingExercise'
}

function isFrontendStudyItem(item: StudySessionItemResponse) {
  const text = `${item.prompt} ${item.expectedAnswer} ${item.explanation} ${item.sourceTitle} ${item.sourceChunkHeading}`.toLowerCase()
  return text.includes('react') || text.includes('typescript') || text.includes('tailwind') || text.includes('product interface')
}

function inferLanguage(item: StudySessionItemResponse) {
  const text = `${item.prompt} ${item.expectedAnswer} ${item.explanation} ${item.sourceTitle} ${item.sourceChunkHeading}`.toLowerCase()
  if (text.includes('c#') || text.includes('csharp') || text.includes('asp.net') || text.includes('.net')) return normalizeCodeLanguage('csharp')
  if (text.includes('typescript') || text.includes('tsx') || text.includes('react') || text.includes('tailwind')) return normalizeCodeLanguage('typescript')
  if (text.includes('javascript') || text.includes('node')) return normalizeCodeLanguage('javascript')
  return normalizeCodeLanguage('typescript')
}
