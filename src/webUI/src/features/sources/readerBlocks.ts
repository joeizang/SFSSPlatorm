import type { ReaderBlock } from './types'

export function parseReaderBlocks(text: string, sourceTitle: string): ReaderBlock[] {
  const lines = text.split('\u0008').join('').split('\n')
  const blocks: ReaderBlock[] = []
  const proseBuffer: string[] = []
  let index = 0

  while (index < lines.length) {
    if (startsCodeBlock(lines, index)) {
      flushProse(blocks, proseBuffer)

      const codeLines: string[] = []
      let blankRun = 0

      while (index < lines.length) {
        const candidate = lines[index]
        const trimmed = candidate.trim()

        if (!trimmed) {
          blankRun++
          if (blankRun > 1) break
          codeLines.push(candidate)
          index++
          continue
        }

        if (isCodeLine(candidate) || keepsCodeBlockGoing(candidate, codeLines)) {
          blankRun = 0
          codeLines.push(candidate)
          index++
          continue
        }

        break
      }

      trimTrailingBlankLines(codeLines)
      const codeText = codeLines.join('\n').trim()
      if (codeText && isRealCodeBlock(codeLines)) {
        const language = inferCodeLanguage(codeText, sourceTitle)
        blocks.push({ kind: 'code', text: codeText, language, label: languageLabel(language) })
      } else {
        proseBuffer.push(...codeLines)
      }
      continue
    }

    proseBuffer.push(lines[index])
    index++
  }

  flushProse(blocks, proseBuffer)
  return blocks
}

function isRealCodeBlock(lines: string[]) {
  const nonBlankLines = lines.map((line) => line.trim()).filter(Boolean)
  const structuralLines = nonBlankLines.filter((line) =>
    /[{};]|=>|^<\/?[a-z][\w:-]*(\s|>|\/>)|^(function\s+[\w$]+\s*\(|console\.|using\s+|namespace\s+|public\s+|private\s+|protected\s+|const\s+|let\s+|var\s+|if\s*\(|for\s*\(|while\s*\(|switch\s*\()/i.test(line),
  )

  if (nonBlankLines.length === 1) {
    return structuralLines.length === 1 && /[{};]|=>|^<\/?[a-z][\w:-]*(\s|>|\/>)/i.test(nonBlankLines[0])
  }

  return structuralLines.length >= 2
}

function startsCodeBlock(lines: string[], index: number) {
  const current = lines[index]
  if (!isCodeLine(current)) return false

  const nextLines = lines.slice(index, index + 5).filter((line) => line.trim())
  const codeScore = nextLines.filter(isCodeLine).length
  return codeScore >= 2 || isStrongCodeLine(current)
}

function isCodeLine(line: string) {
  const value = line.trim()
  if (!value) return false
  if (/^\[\s*\d+\s*\]$/.test(value)) return false
  if (/^Chapter\s+\d+/i.test(value)) return false
  if (value.length > 160 && !/[{};]/.test(value)) return false

  return (
    isStrongCodeLine(value) ||
    /^(using|namespace|public|private|protected|internal|sealed|static|class|record|interface|enum)\b/.test(value) ||
    /^(const|let|var)\s+[\w$]+\s*(=|:|;)/.test(value) ||
    /^function\s+[\w$]+\s*\(/.test(value) ||
    /^(if|for|while|switch|catch)\s*\(/.test(value) ||
    /^(else|try)\b/.test(value) ||
    /^(import|export|return|await|async)\b.*[;({]/.test(value) ||
    /^(npm|pnpm|yarn|dotnet|git|curl|npx|node)\b/.test(value) ||
    /^(SELECT|INSERT|UPDATE|DELETE|CREATE|ALTER|FROM|WHERE|JOIN)\b/i.test(value) ||
    /^<\/?[a-z][\w:-]*(\s|>|\/>)/i.test(value) ||
    /^[.#]?[a-z][\w-]*\s*\{/.test(value) ||
    /=>|;\s*$|{\s*$|}\s*$/.test(value)
  )
}

function isStrongCodeLine(line: string) {
  const value = line.trim()
  return (
    /^console\./.test(value) ||
    /^\/\/|^\/\*|^\*/.test(value) ||
    /^#(include|define|!)/i.test(value) ||
    /^(public|private|protected|class|const|let|var|await|async|return)\b.*[;{(]/.test(value) ||
    /^function\s+[\w$]+\s*\(/.test(value) ||
    /<\/?[A-Z][\w.]*/.test(value)
  )
}

function keepsCodeBlockGoing(line: string, codeLines: string[]) {
  if (codeLines.length === 0) return false
  const trimmed = line.trim()
  const previous = codeLines[codeLines.length - 1]?.trim() ?? ''
  return (
    /^[})\]]/.test(trimmed) ||
    /[,.:]\s*$/.test(previous) ||
    previous.endsWith('{') ||
    previous.endsWith('(') ||
    previous.endsWith('[')
  )
}

function inferCodeLanguage(code: string, sourceTitle: string) {
  const combined = `${sourceTitle}\n${code}`.toLowerCase()
  if (/<\/?[a-z][\w:-]*(\s|>|\/>)/i.test(code)) return 'html'
  if (/^\s*(select|insert|update|delete|create|alter)\b/im.test(code)) return 'sql'
  if (/^\s*(npm|pnpm|yarn|dotnet|git|curl|npx|node)\b/im.test(code)) return 'bash'
  if (/^\s*[.#]?[a-z][\w-]*\s*\{/im.test(code) && /;\s*$/m.test(code)) return 'css'
  if (/\busing\s+[\w.]+;|\bnamespace\s+[\w.]+|\bpublic\s+(sealed\s+)?(class|record|interface)\b/i.test(code)) {
    return 'csharp'
  }
  if (/\binterface\s+\w+\s*\{|\btype\s+\w+\s*=|:\s*(string|number|boolean)\b/.test(code)) return 'typescript'
  if (combined.includes('typescript')) return 'typescript'
  if (combined.includes('c#') || combined.includes('.net') || combined.includes('asp.net')) return 'csharp'
  if (combined.includes('javascript') || /\b(console\.log|function|const|let|var)\b/.test(code)) return 'javascript'
  return 'text'
}

function languageLabel(language: string) {
  const labels: Record<string, string> = {
    bash: 'Shell',
    csharp: 'C#',
    css: 'CSS',
    html: 'HTML',
    javascript: 'JavaScript',
    sql: 'SQL',
    text: 'Text',
    typescript: 'TypeScript',
  }

  return labels[language] ?? language
}

function flushProse(blocks: ReaderBlock[], proseBuffer: string[]) {
  trimTrailingBlankLines(proseBuffer)
  const text = proseBuffer.join('\n').trim()
  if (text) {
    blocks.push({ kind: 'prose', text })
  }
  proseBuffer.length = 0
}

function trimTrailingBlankLines(lines: string[]) {
  while (lines.length > 0 && !lines[lines.length - 1].trim()) {
    lines.pop()
  }
}
