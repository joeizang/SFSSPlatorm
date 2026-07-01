import Editor, { type OnMount } from '@monaco-editor/react'
import { useMemo } from 'react'
import { normalizeCodeLanguage } from './codeEditorLanguage'
import { configureMonacoLanguage, reactModelPath } from './monacoReactSupport'

type CodeEditorSurfaceProps = {
  editorHeight?: string
  language: string
  loadingLabel?: string
  onChange: (value: string) => void
  onMount?: OnMount
  tabSize?: number
  value: string
}

export function CodeEditorSurface({
  editorHeight = '460px',
  language,
  loadingLabel = 'Loading editor',
  onChange,
  onMount,
  tabSize = 4,
  value,
}: CodeEditorSurfaceProps) {
  const normalizedLanguage = normalizeCodeLanguage(language)
  const options = useMemo(
    () => ({
      automaticLayout: true,
      fontFamily: '"SFMono-Regular", "SF Mono", Consolas, "Liberation Mono", Menlo, monospace',
      fontSize: 13,
      lineHeight: 21,
      minimap: { enabled: false },
      padding: { top: 12, bottom: 12 },
      renderLineHighlight: 'line' as const,
      scrollBeyondLastLine: false,
      smoothScrolling: true,
      tabSize,
      wordWrap: 'on' as const,
    }),
    [tabSize],
  )

  const handleMount: OnMount = (editor, monaco) => {
    configureMonacoLanguage(monaco, normalizedLanguage)
    monaco.editor.defineTheme('sfss-dark', {
      base: 'vs-dark',
      inherit: true,
      rules: [
        { token: 'comment', foreground: '7f8c98', fontStyle: 'italic' },
        { token: 'keyword', foreground: 'c792ea' },
        { token: 'number', foreground: 'f78c6c' },
        { token: 'string', foreground: 'ecc48d' },
        { token: 'type', foreground: 'addb67' },
      ],
      colors: {
        'editor.background': '#080a0d',
        'editor.foreground': '#d6deeb',
        'editor.lineHighlightBackground': '#11182766',
        'editorLineNumber.foreground': '#697586',
        'editorLineNumber.activeForeground': '#b8c2d2',
        'editorCursor.foreground': '#22d3ee',
        'editor.selectionBackground': '#164e63',
        'editor.inactiveSelectionBackground': '#1f2937',
      },
    })
    monaco.editor.setTheme('sfss-dark')
    onMount?.(editor, monaco)
  }

  return (
    <div style={{ height: editorHeight }}>
      <Editor
        defaultLanguage={normalizedLanguage}
        language={normalizedLanguage}
        loading={<div className="p-3 text-sm text-slate-500">{loadingLabel}</div>}
        options={options}
        path={reactModelPath(normalizedLanguage)}
        theme="sfss-dark"
        value={value}
        onChange={(nextValue) => onChange(nextValue ?? '')}
        onMount={handleMount}
      />
    </div>
  )
}
