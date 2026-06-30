import type { ExtractionStatus, SourceAccess } from './types'

export function accessLabel(access: SourceAccess) {
  if (access === 'open') return 'Open source'
  if (access === 'purchasedLocal') return 'Purchased local'
  return 'Local source'
}

export function statusLabel(status: ExtractionStatus) {
  if (status === 'completed') return 'readable'
  if (status === 'failed') return 'failed'
  return 'not extracted'
}
