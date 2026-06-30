import type { CurriculumSeed } from './types'

export const previewSeed: CurriculumSeed = {
  modules: [
    {
      id: 'module-aspnet-core',
      slug: 'aspnet-core',
      title: 'ASP.NET Core API Engineering',
      description: 'HTTP APIs, contracts, diagnostics, validation, and production behavior.',
      order: 1,
      topics: [
        {
          id: 'topic-minimal-api-routing',
          slug: 'minimal-api-routing',
          title: 'Minimal API route groups',
          summary: 'Design stable route groups with typed results and cancellation-aware handlers.',
          order: 1,
          taskTypes: ['read', 'build', 'writeTests'],
        },
        {
          id: 'topic-problem-details',
          slug: 'problem-details',
          title: 'ProblemDetails error contracts',
          summary: 'Return consistent validation and failure responses from API boundaries.',
          order: 2,
          taskTypes: ['read', 'review', 'writeTests'],
        },
        {
          id: 'topic-openapi-contracts',
          slug: 'openapi-contracts',
          title: 'OpenAPI as frontend contract',
          summary: 'Treat the generated contract as the agreement between API and TypeScript client.',
          order: 3,
          taskTypes: ['read', 'build', 'document'],
        },
      ],
    },
    {
      id: 'module-ef-core',
      slug: 'ef-core',
      title: 'EF Core Persistence',
      description: 'SQLite-backed persistence, projections, migrations, and re-import safety.',
      order: 2,
      topics: [
        {
          id: 'topic-sqlite-modeling',
          slug: 'sqlite-modeling',
          title: 'SQLite model configuration',
          summary: 'Map aggregate data explicitly with keys, indexes, owned state, and conversions.',
          order: 1,
          taskTypes: ['read', 'build', 'review'],
        },
        {
          id: 'topic-idempotent-importer',
          slug: 'idempotent-importer',
          title: 'Idempotent curriculum import',
          summary: 'Upsert reference data while preserving progress and linked study records.',
          order: 2,
          taskTypes: ['build', 'debug', 'writeTests'],
        },
      ],
    },
    {
      id: 'module-react-typescript',
      slug: 'react-typescript',
      title: 'React + TypeScript Learning UI',
      description: 'Dense, code-adjacent learning workflows with server state kept explicit.',
      order: 3,
      topics: [
        {
          id: 'topic-tanstack-query',
          slug: 'tanstack-query',
          title: 'TanStack Query catalog state',
          summary: 'Cache server-owned catalog/progress data without hiding API behavior.',
          order: 1,
          taskTypes: ['read', 'build'],
        },
        {
          id: 'topic-tailwind-product-ui',
          slug: 'tailwind-product-ui',
          title: 'Tailwind product interface',
          summary: 'Build restrained dark product screens without generic dashboard ornament.',
          order: 2,
          taskTypes: ['build', 'review', 'document'],
        },
      ],
    },
  ],
  phases: [
    {
      id: 'phase-foundation',
      slug: 'foundation-path',
      title: 'Foundation Path',
      description: 'API, persistence, and frontend contract fundamentals.',
      order: 1,
      topics: [
        { topicId: 'topic-minimal-api-routing', order: 1 },
        { topicId: 'topic-sqlite-modeling', order: 2 },
        { topicId: 'topic-tanstack-query', order: 3 },
      ],
    },
  ],
}
