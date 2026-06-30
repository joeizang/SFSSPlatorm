import { useMemo } from 'react'
import hljs from 'highlight.js/lib/core'
import bash from 'highlight.js/lib/languages/bash'
import csharp from 'highlight.js/lib/languages/csharp'
import css from 'highlight.js/lib/languages/css'
import javascript from 'highlight.js/lib/languages/javascript'
import plaintext from 'highlight.js/lib/languages/plaintext'
import sql from 'highlight.js/lib/languages/sql'
import typescript from 'highlight.js/lib/languages/typescript'
import xml from 'highlight.js/lib/languages/xml'
import { parseReaderBlocks } from './readerBlocks'
import type { ReaderBlock, SourceChunkResponse } from './types'

hljs.registerLanguage('bash', bash)
hljs.registerLanguage('csharp', csharp)
hljs.registerLanguage('css', css)
hljs.registerLanguage('html', xml)
hljs.registerLanguage('javascript', javascript)
hljs.registerLanguage('sql', sql)
hljs.registerLanguage('text', plaintext)
hljs.registerLanguage('typescript', typescript)

export function ReaderContent({ chunk }: { chunk: SourceChunkResponse }) {
  const blocks = useMemo(() => parseReaderBlocks(chunk.text, chunk.sourceTitle), [chunk.sourceTitle, chunk.text])

  return (
    <div className="reader-text mt-5 max-w-4xl text-[15px] leading-7 text-slate-300">
      {blocks.map((block, index) =>
        block.kind === 'code' ? (
          <CodeBlock key={`${block.kind}-${index}`} block={block} />
        ) : (
          <p key={`${block.kind}-${index}`} className="mb-5 whitespace-pre-wrap">
            {block.text}
          </p>
        ),
      )}
    </div>
  )
}

function CodeBlock({ block }: { block: Extract<ReaderBlock, { kind: 'code' }> }) {
  const lineCount = block.text.split('\n').length
  const highlighted = highlightCode(block.text, block.language)

  return (
    <figure className="reader-code-block my-6 overflow-hidden rounded-md border border-zinc-700 bg-[#0a0d12] shadow-[inset_0_1px_0_rgba(255,255,255,0.04)]">
      <figcaption className="flex h-9 items-center justify-between border-b border-zinc-800 bg-[#111722] px-3">
        <span className="text-xs font-semibold uppercase tracking-wide text-cyan-200">Code snippet</span>
        <span className="text-xs text-slate-400">{block.label}</span>
      </figcaption>
      <pre className="reader-code-scroll m-0 overflow-x-auto bg-[#0a0d12] p-4 text-[13.5px] leading-[1.65]">
        <code
          className={`hljs language-${block.language} ${lineCount > 3 ? 'with-lines' : ''}`}
          dangerouslySetInnerHTML={{ __html: lineCount > 3 ? withLineNumbers(highlighted) : highlighted }}
        />
      </pre>
    </figure>
  )
}

function highlightCode(code: string, language: string) {
  if (!hljs.getLanguage(language)) {
    return escapeHtml(code)
  }

  return hljs.highlight(code, { language, ignoreIllegals: true }).value
}

function withLineNumbers(highlightedCode: string) {
  return highlightedCode
    .split('\n')
    .map((line, index) => `<span class="code-line"><span class="line-number">${index + 1}</span><span class="line-source">${line || ' '}</span></span>`)
    .join('\n')
}

function escapeHtml(value: string) {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;')
}
