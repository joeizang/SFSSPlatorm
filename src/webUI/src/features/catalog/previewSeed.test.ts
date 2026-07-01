import { describe, expect, it } from 'vitest'
import { previewSeed } from './previewSeed'

describe('previewSeed', () => {
  it('ships thick runtime and standard-library coverage', () => {
    const modules = new Map(previewSeed.modules.map((module) => [module.slug, module]))

    expect(modules.get('dotnet-bcl')?.topics.map((topic) => topic.slug)).toEqual(
      expect.arrayContaining([
        'bcl-collections',
        'bcl-linq',
        'bcl-async-tasks',
        'bcl-streams-files',
        'bcl-memory-span',
        'bcl-system-text-json',
      ]),
    )

    expect(modules.get('js-node-standard-library')?.topics.map((topic) => topic.slug)).toEqual(
      expect.arrayContaining([
        'js-values-equality',
        'js-promises-event-loop',
        'node-fs-path',
        'node-streams-buffers',
        'node-http-fetch',
        'node-crypto',
      ]),
    )

    expect(modules.get('react-typescript')?.topics.map((topic) => topic.slug)).toEqual(
      expect.arrayContaining(['typescript-type-modeling', 'typescript-runtime-validation', 'react-state-composition']),
    )

    expect(modules.get('aspnet-core')?.topics.map((topic) => topic.slug)).toContain('aspnet-middleware-pipeline')
    expect(previewSeed.phases.map((phase) => phase.slug)).toContain('runtime-foundations')
  })
})
