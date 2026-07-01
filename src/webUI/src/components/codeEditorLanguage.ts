export function normalizeCodeLanguage(language: string) {
  const normalized = language.trim().toLowerCase()
  if (normalized === 'csharp' || normalized === 'c#') return 'csharp'
  if (normalized === 'typescript' || normalized === 'ts' || normalized === 'tsx') return 'typescript'
  if (normalized === 'javascript' || normalized === 'js' || normalized === 'jsx') return 'javascript'
  if (normalized === 'html') return 'html'
  if (normalized === 'css') return 'css'
  return 'plaintext'
}
