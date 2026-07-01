import { type OnMount } from '@monaco-editor/react'
import { useEffect, useRef, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { AlertTriangle, CheckCircle2, Copy, Play, RotateCcw, Save } from 'lucide-react'
import { api } from '../../api/http'
import { CodeEditorSurface } from '../../components/CodeEditorSurface'
import { normalizeCodeLanguage } from '../../components/codeEditorLanguage'
import type { CodingExerciseResponse, CodingExerciseSolutionResponse, ExerciseCheckResponse } from '../study-items/types'

type ExerciseCodeEditorProps = {
  editorHeight?: string
  exercise: CodingExerciseResponse
}

export function ExerciseCodeEditor({ editorHeight = '460px', exercise }: ExerciseCodeEditorProps) {
  const [code, setCode] = useState(exercise.starterCode)
  const [copied, setCopied] = useState(false)
  const [lastCheck, setLastCheck] = useState<ExerciseCheckResponse | null>(null)
  const editorRef = useRef<Parameters<OnMount>[0] | null>(null)
  const monacoRef = useRef<Parameters<OnMount>[1] | null>(null)
  const queryClient = useQueryClient()
  const language = normalizeCodeLanguage(exercise.language)

  const solutionQuery = useQuery({
    queryKey: ['coding-exercise-solution', exercise.topicSlug, exercise.id],
    queryFn: () =>
      api<CodingExerciseSolutionResponse>(`/api/catalog/topics/${exercise.topicSlug}/exercises/${exercise.id}/solution`),
  })

  const saveMutation = useMutation({
    mutationFn: (nextCode: string) =>
      api<CodingExerciseSolutionResponse>(`/api/catalog/topics/${exercise.topicSlug}/exercises/${exercise.id}/solution`, {
        method: 'PUT',
        body: JSON.stringify({ code: nextCode }),
      }),
    onSuccess: async (solution) => {
      setCode(solution.code)
      await queryClient.invalidateQueries({ queryKey: ['coding-exercise-solution', exercise.topicSlug, exercise.id] })
    },
  })

  const diagnosticsMutation = useMutation({
    mutationFn: (nextCode: string) =>
      api<ExerciseCheckResponse>(`/api/catalog/topics/${exercise.topicSlug}/exercises/${exercise.id}/diagnostics`, {
        method: 'POST',
        body: JSON.stringify({ code: nextCode }),
      }),
    onSuccess: (result) => {
      setLastCheck(result)
      applyMarkers(result)
    },
  })

  const checkMutation = useMutation({
    mutationFn: (nextCode: string) =>
      api<ExerciseCheckResponse>(`/api/catalog/topics/${exercise.topicSlug}/exercises/${exercise.id}/check`, {
        method: 'POST',
        body: JSON.stringify({ code: nextCode }),
      }),
    onSuccess: (result) => {
      setLastCheck(result)
      applyMarkers(result)
      void queryClient.invalidateQueries({ queryKey: ['coding-exercise-solution', exercise.topicSlug, exercise.id] })
    },
  })

  const handleMount: OnMount = (editor, monaco) => {
    editorRef.current = editor
    monacoRef.current = monaco
    editor.focus()
  }

  useEffect(() => {
    setCode(solutionQuery.data?.code ?? exercise.starterCode)
    setLastCheck(null)
    clearMarkers()
  }, [exercise.id, exercise.starterCode, solutionQuery.data?.code])

  useEffect(() => {
    if (!solutionQuery.isFetched || code.trim().length === 0) {
      clearMarkers()
      return
    }

    const handle = window.setTimeout(() => {
      diagnosticsMutation.mutate(code)
    }, 900)

    return () => window.clearTimeout(handle)
  }, [code, diagnosticsMutation, solutionQuery.isFetched])

  const copyCode = async () => {
    await navigator.clipboard?.writeText(code)
    setCopied(true)
    window.setTimeout(() => setCopied(false), 1600)
  }

  const applyMarkers = (result: ExerciseCheckResponse) => {
    const editor = editorRef.current
    const monaco = monacoRef.current
    const model = editor?.getModel()
    if (!editor || !monaco || !model) return

    monaco.editor.setModelMarkers(
      model,
      'roslyn',
      result.diagnostics.map((diagnostic) => ({
        severity: diagnostic.severity === 'error' ? monaco.MarkerSeverity.Error : monaco.MarkerSeverity.Warning,
        message: diagnostic.message,
        source: diagnostic.id,
        startLineNumber: diagnostic.startLine,
        startColumn: diagnostic.startColumn,
        endLineNumber: diagnostic.endLine,
        endColumn: Math.max(diagnostic.endColumn, diagnostic.startColumn + 1),
      })),
    )
  }

  const clearMarkers = () => {
    const editor = editorRef.current
    const monaco = monacoRef.current
    const model = editor?.getModel()
    if (!monaco || !model) return

    monaco.editor.setModelMarkers(model, 'roslyn', [])
  }

  return (
    <div className="overflow-hidden rounded-md border border-zinc-800 bg-[#080a0d]">
      <div className="flex flex-wrap items-center justify-between gap-2 border-b border-zinc-800 bg-[#0b0d10] px-3 py-2">
        <div className="flex min-w-0 flex-wrap items-center gap-2">
          <span className="text-xs font-semibold uppercase tracking-wide text-slate-500">Editor</span>
          <span className="rounded border border-zinc-800 px-1.5 py-0.5 text-xs text-slate-400">{language}</span>
          {lastCheck && (
            <span
              className={`inline-flex items-center rounded border px-1.5 py-0.5 text-xs ${
                lastCheck.succeeded ? 'border-emerald-800 text-emerald-300' : 'border-red-900 text-red-300'
              }`}
            >
              {lastCheck.succeeded ? <CheckCircle2 className="mr-1 h-3 w-3" /> : <AlertTriangle className="mr-1 h-3 w-3" />}
              {lastCheck.succeeded ? 'Passed' : `${lastCheck.grading.requiredFailed} check${lastCheck.grading.requiredFailed === 1 ? '' : 's'} failing`}
            </span>
          )}
        </div>
        <div className="flex flex-wrap items-center gap-1.5">
          <button
            className="focus-ring inline-flex h-7 items-center rounded-md px-2 text-xs text-slate-400 hover:bg-zinc-900 hover:text-slate-100 disabled:cursor-not-allowed disabled:opacity-60"
            type="button"
            disabled={saveMutation.isPending || solutionQuery.isLoading}
            onClick={() => saveMutation.mutate(code)}
          >
            <Save className="mr-1.5 h-3.5 w-3.5" />
            {saveMutation.isPending ? 'Saving' : 'Save code'}
          </button>
          <button
            className="focus-ring inline-flex h-7 items-center rounded-md bg-cyan-500 px-2 text-xs font-medium text-slate-950 hover:bg-cyan-400 disabled:cursor-not-allowed disabled:opacity-60"
            type="button"
            disabled={checkMutation.isPending || code.trim().length === 0}
            onClick={() => checkMutation.mutate(code)}
          >
            <Play className="mr-1.5 h-3.5 w-3.5" />
            {checkMutation.isPending ? 'Checking' : 'Check'}
          </button>
          <button
            className="focus-ring inline-flex h-7 items-center rounded-md px-2 text-xs text-slate-400 hover:bg-zinc-900 hover:text-slate-100"
            type="button"
            onClick={() => setCode(exercise.starterCode)}
          >
            <RotateCcw className="mr-1.5 h-3.5 w-3.5" />
            Reset
          </button>
          <button
            className="focus-ring inline-flex h-7 items-center rounded-md px-2 text-xs text-slate-400 hover:bg-zinc-900 hover:text-slate-100"
            type="button"
            onClick={copyCode}
          >
            <Copy className="mr-1.5 h-3.5 w-3.5" />
            {copied ? 'Copied' : 'Copy'}
          </button>
        </div>
      </div>
      <CodeEditorSurface editorHeight={editorHeight} language={language} value={code} onChange={setCode} onMount={handleMount} />
      <div className="grid gap-0 border-t border-zinc-800 md:grid-cols-[minmax(0,1fr)_260px]">
        <div className="min-h-20 p-3">
          <div className="mb-2 flex items-center justify-between">
            <h4 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Diagnostics</h4>
            {diagnosticsMutation.isPending && <span className="text-xs text-slate-500">Analyzing</span>}
          </div>
          {solutionQuery.isLoading ? (
            <p className="text-xs leading-5 text-slate-500">Loading saved solution.</p>
          ) : lastCheck?.diagnostics.length ? (
            <div className="space-y-2">
              {lastCheck.diagnostics.map((diagnostic, index) => (
                <div key={`${diagnostic.id}-${diagnostic.startLine}-${diagnostic.startColumn}-${index}`} className="rounded border border-zinc-800 bg-[#0b0d10] p-2">
                  <div className="flex items-center gap-2 text-xs">
                    <span className={diagnostic.severity === 'error' ? 'font-semibold text-red-300' : 'font-semibold text-amber-300'}>
                      {diagnostic.severity}
                    </span>
                    <span className="text-slate-600">{diagnostic.id}</span>
                    <span className="text-slate-500">
                      line {diagnostic.startLine}, col {diagnostic.startColumn}
                    </span>
                  </div>
                  <p className="mt-1 text-xs leading-5 text-slate-300">{diagnostic.message}</p>
                </div>
              ))}
            </div>
          ) : lastCheck ? (
            <p className="text-xs leading-5 text-emerald-300">Roslyn compile diagnostics are clean for the current editor contents.</p>
          ) : (
            <p className="text-xs leading-5 text-slate-500">Diagnostics appear here after the editor settles or when you run Check.</p>
          )}
          {lastCheck && (
            <div className="mt-3 border-t border-zinc-800 pt-3">
              <div className="mb-2 flex items-center justify-between">
                <h4 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Grading</h4>
                <span className={lastCheck.succeeded ? 'text-xs text-emerald-300' : 'text-xs text-amber-300'}>
                  {lastCheck.grading.passed}/{lastCheck.grading.checks.length} passed
                </span>
              </div>
              <div className="space-y-1.5">
                {lastCheck.grading.checks.map((check) => (
                  <div key={check.id} className="rounded border border-zinc-800 bg-[#0b0d10] p-2">
                    <div className="flex items-start gap-2">
                      {check.status === 'pass' ? (
                        <CheckCircle2 className="mt-0.5 h-3.5 w-3.5 shrink-0 text-emerald-300" />
                      ) : (
                        <AlertTriangle className="mt-0.5 h-3.5 w-3.5 shrink-0 text-amber-300" />
                      )}
                      <div className="min-w-0">
                        <div className="text-xs font-medium text-slate-200">{check.title}</div>
                        <p className="mt-0.5 text-xs leading-5 text-slate-500">{check.status === 'pass' ? check.description : check.guidance}</p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
              {lastCheck.grading.sampleCases.length > 0 && (
                <div className="mt-3">
                  <h4 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Sample cases</h4>
                  <div className="mt-2 grid gap-1.5 sm:grid-cols-2">
                    {lastCheck.grading.sampleCases.map((sample) => (
                      <div key={sample.name} className="rounded border border-zinc-800 bg-[#0b0d10] p-2 text-xs">
                        <div className="font-medium text-slate-300">{sample.name}</div>
                        <div className="mt-1 text-slate-500">Input: {sample.input}</div>
                        <div className="mt-0.5 text-slate-500">
                          Expected: {sample.expected ?? sample.expectedException ?? 'specified by grader'}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}
          {(saveMutation.error || checkMutation.error || diagnosticsMutation.error) && (
            <p className="mt-2 text-xs leading-5 text-red-300">
              {(saveMutation.error ?? checkMutation.error ?? diagnosticsMutation.error)?.message}
            </p>
          )}
        </div>
        <div className="border-t border-zinc-800 p-3 md:border-l md:border-t-0">
          <h4 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Packages</h4>
          {lastCheck?.packageRequirements.length ? (
            <div className="mt-2 space-y-1.5">
              {lastCheck.packageRequirements.map((item) => (
                <div key={`${item.manager}-${item.name}`} className="flex items-center justify-between gap-2 text-xs">
                  <span className="min-w-0 truncate text-slate-300">{item.name}</span>
                  <span className="shrink-0 rounded border border-zinc-800 px-1.5 py-0.5 text-slate-500">{item.manager}</span>
                </div>
              ))}
            </div>
          ) : (
            <p className="mt-2 text-xs leading-5 text-slate-500">NuGet and npm requirements are parsed on check.</p>
          )}
        </div>
      </div>
    </div>
  )
}
