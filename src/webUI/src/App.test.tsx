import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'

const modules = [
  {
    slug: 'aspnet-core',
    title: 'ASP.NET Core API Engineering',
    description: 'HTTP APIs.',
    totalTopics: 1,
    doneTopics: 0,
    completionPercentage: 0,
    topics: [{ slug: 'minimal-api-routing', title: 'Minimal API route groups', status: 'notStarted' }],
  },
]

const topics = [
  {
    slug: 'minimal-api-routing',
    title: 'Minimal API route groups',
    summary: 'Design stable route groups.',
    moduleSlug: 'aspnet-core',
    moduleTitle: 'ASP.NET Core API Engineering',
    status: 'notStarted',
    taskTypes: ['read', 'build', 'writeTests'],
  },
]

const rollups = {
  modules: [
    {
      slug: 'aspnet-core',
      title: 'ASP.NET Core API Engineering',
      totalTopics: 1,
      doneTopics: 0,
      inProgressTopics: 0,
      completionPercentage: 0,
    },
  ],
  phases: [],
}

const sources = [
  {
    id: 7,
    title: 'ASP.NET Core in Action',
    author: 'Andrew Lock',
    fileName: 'aspnet-core-in-action.pdf',
    access: 'purchasedLocal',
    fileSizeBytes: 10_000,
    pageCount: 800,
    extractionStatus: 'completed',
    extractionError: null,
    extractedAt: '2026-06-30T12:00:00Z',
    chunkCount: 2,
    characterCount: 12_000,
  },
]

const chunks = [
  {
    id: 11,
    order: 1,
    startPage: 1,
    endPage: 4,
    heading: 'Middleware and request pipeline',
    characterCount: 6_000,
  },
]

const chunk = {
  ...chunks[0],
  sourceMaterialId: 7,
  sourceTitle: 'ASP.NET Core in Action',
  text: `ASP.NET Core middleware forms a request pipeline for handling HTTP requests.

public sealed class MiddlewareExample
{
    public Task InvokeAsync(HttpContext context)
    {
        return Task.CompletedTask;
    }
}`,
}

const generatedStudyItems = {
  sourceDocumentChunkId: 11,
  sourceMaterialId: 7,
  sourceTitle: 'ASP.NET Core in Action',
  startPage: 1,
  endPage: 4,
  items: [
    {
      kind: 'codeReading',
      prompt: 'Read the code snippet. What will it do, return, or print, and why?',
      expectedAnswer: 'It returns a completed task after the middleware receives the current HTTP context.',
      explanation: 'The answer should trace the InvokeAsync method and the returned Task.',
      sourceExcerpt: 'public sealed class MiddlewareExample { public Task InvokeAsync(HttpContext context) { return Task.CompletedTask; } }',
    },
  ],
}

const studySessionNext = {
  dueCount: 1,
  item: {
    id: 99,
    kind: 'shortAnswer',
    prompt: 'What does ASP.NET Core middleware do?',
    expectedAnswer: 'It composes a request pipeline that handles HTTP requests and responses.',
    explanation: 'Middleware is ordered, and each component can inspect or modify the request and response.',
    sourceExcerpt: 'ASP.NET Core middleware forms a request pipeline for handling HTTP requests.',
    startPage: 1,
    endPage: 4,
    sourceTitle: 'ASP.NET Core in Action',
    sourceChunkHeading: 'Middleware and request pipeline',
    attemptCount: 0,
    confidenceScore: 0,
    lastAttemptAt: null,
    nextReviewAt: null,
  },
}

const videos = [
  {
    id: 201,
    externalId: 'YbRe4iIVYJk',
    provider: 'youTube',
    title: '.NET 10 + C# 14 - Full Course 2026 [E-commerce MVC]',
    creator: '.NET',
    url: 'https://www.youtube.com/watch?v=YbRe4iIVYJk',
    embedUrl: 'https://www.youtube-nocookie.com/embed/YbRe4iIVYJk',
    summary: 'Official .NET channel course material for modern C# and ASP.NET Core application development.',
    tags: ['aspnet-core', 'backend', 'csharp', 'dotnet', 'official'],
    durationSeconds: null,
    topicId: null,
    sourceMaterialId: null,
    sourceDocumentChunkId: null,
    isWatched: false,
    watchedAt: null,
    watchProgressSeconds: 0,
    notes: '',
    createdAt: '2026-06-30T12:00:00Z',
    updatedAt: '2026-06-30T12:00:00Z',
  },
]

const trustedChannels = [
  {
    id: 301,
    name: 'Nick Chapsas',
    url: 'https://www.youtube.com/@nickchapsas',
    tags: ['architecture', 'csharp', 'dotnet', 'performance'],
    priority: 'high',
    notes: 'High-signal modern .NET and C# engineering.',
    createdAt: '2026-06-30T12:00:00Z',
    updatedAt: '2026-06-30T12:00:00Z',
  },
  {
    id: 302,
    name: 'Raw Coding',
    url: 'https://www.youtube.com/@RawCoding',
    tags: ['aspnet-core', 'csharp', 'dotnet'],
    priority: 'high',
    notes: 'ASP.NET Core and C# implementation depth.',
    createdAt: '2026-06-30T12:00:00Z',
    updatedAt: '2026-06-30T12:00:00Z',
  },
]

const videoCandidates = [
  {
    id: 401,
    externalId: 'R-z2Hv-7nxk',
    title: 'Deep .NET: Writing async/await from scratch',
    channelName: 'dotnet',
    channelUrl: 'https://www.youtube.com/@dotnet',
    url: 'https://www.youtube.com/watch?v=R-z2Hv-7nxk',
    embedUrl: 'https://www.youtube-nocookie.com/embed/R-z2Hv-7nxk',
    summary: 'Stephen Toub and Scott Hanselman break down async and await by building the mechanism from scratch.',
    tags: ['async', 'csharp', 'dotnet', 'runtime'],
    difficulty: 'expert',
    status: 'candidate',
    notes: 'High-value runtime internals candidate.',
    rejectionReason: '',
    learningResourceId: null,
    acceptedAt: null,
    rejectedAt: null,
    createdAt: '2026-06-30T12:00:00Z',
    updatedAt: '2026-06-30T12:00:00Z',
  },
  {
    id: 402,
    externalId: 'Qwb-Za6cBws',
    title: 'How To Maximize Performance In Your React Apps',
    channelName: 'Web Dev Simplified',
    channelUrl: 'https://www.youtube.com/@WebDevSimplified',
    url: 'https://www.youtube.com/watch?v=Qwb-Za6cBws',
    embedUrl: 'https://www.youtube-nocookie.com/embed/Qwb-Za6cBws',
    summary: 'Practical React performance techniques and profiling-oriented thinking.',
    tags: ['frontend', 'performance', 'react'],
    difficulty: 'intermediate',
    status: 'candidate',
    notes: 'Good applied frontend performance candidate.',
    rejectionReason: '',
    learningResourceId: null,
    acceptedAt: null,
    rejectedAt: null,
    createdAt: '2026-06-30T12:00:00Z',
    updatedAt: '2026-06-30T12:00:00Z',
  },
]

describe('App', () => {
  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      vi.fn((input: RequestInfo | URL, init?: RequestInit) => {
        const url = String(input)

        if (url.endsWith('/api/catalog/modules')) {
          return jsonResponse(modules)
        }

        if (url.includes('/api/catalog/topics') && init?.method !== 'PUT') {
          return jsonResponse(topics)
        }

        if (url.endsWith('/api/catalog/rollups')) {
          return jsonResponse(rollups)
        }

        if (url.endsWith('/api/curriculum/import')) {
          return jsonResponse({ modulesProcessed: 3, topicsProcessed: 7 })
        }

        if (url.includes('/progress')) {
          return jsonResponse({ ...topics[0], status: 'done' })
        }

        if (url.endsWith('/api/sources/ingest-local')) {
          return jsonResponse({
            sourcesDiscovered: 28,
            sourcesIngested: 1,
            sourcesSkipped: 27,
            sourcesFailed: 0,
            chunksCreated: 2,
          })
        }

        if (url.endsWith('/api/sources/')) {
          return jsonResponse(sources)
        }

        if (url.endsWith('/api/sources/7/chunks')) {
          return jsonResponse(chunks)
        }

        if (url.endsWith('/api/sources/chunks/11')) {
          return jsonResponse(chunk)
        }

        if (url.endsWith('/api/study-items/?sourceDocumentChunkId=11')) {
          return jsonResponse([])
        }

        if (url.endsWith('/api/study-items/generate')) {
          return jsonResponse(generatedStudyItems)
        }

        if (url.endsWith('/api/study-items/')) {
          return jsonResponse([
            {
              id: 99,
              sourceMaterialId: 7,
              sourceDocumentChunkId: 11,
              startPage: 1,
              endPage: 4,
              status: 'active',
              createdAt: '2026-06-30T12:00:00Z',
              updatedAt: '2026-06-30T12:00:00Z',
              attemptCount: 0,
              confidenceScore: 0,
              lastAttemptAt: null,
              nextReviewAt: null,
              ...generatedStudyItems.items[0],
            },
          ])
        }

        if (url.endsWith('/api/study-session/next')) {
          return jsonResponse(studySessionNext)
        }

        if (url.endsWith('/api/study-session/summary')) {
          return jsonResponse({ activeItems: 1, dueItems: 1, answeredToday: 0, weakItems: 0 })
        }

        if (url.endsWith('/api/study-session/attempts')) {
          return jsonResponse({
            id: 123,
            studyItemId: 99,
            answer: 'It builds the HTTP request pipeline.',
            rating: 'good',
            attemptedAt: '2026-06-30T12:00:00Z',
            attemptCount: 1,
            confidenceScore: 1,
            nextReviewAt: '2026-07-01T12:00:00Z',
          })
        }

        if (url.endsWith('/api/learning-resources/')) {
          return jsonResponse(videos)
        }

        if (url.endsWith('/api/learning-resources/import-seed')) {
          return jsonResponse({ resourcesDiscovered: 6, resourcesCreated: 6, resourcesUpdated: 0 })
        }

        if (url.endsWith('/api/learning-resources/201/watch-state')) {
          return jsonResponse({
            ...videos[0],
            isWatched: true,
            notes: 'Strong overview of modern .NET structure.',
            watchProgressSeconds: 900,
            watchedAt: '2026-06-30T13:00:00Z',
          })
        }

        if (url.endsWith('/api/learning-resources/channels/source-file')) {
          return jsonResponse({ sourceFile: '/workspace/local-source/youtube/channels.csv' })
        }

        if (url.endsWith('/api/learning-resources/channels/import-local')) {
          return jsonResponse({
            sourceFile: '/workspace/local-source/youtube/channels.csv',
            channelsDiscovered: 29,
            channelsCreated: 29,
            channelsUpdated: 0,
            channelsSkipped: 0,
          })
        }

        if (url.includes('/api/learning-resources/channels/') && init?.method !== 'POST') {
          return jsonResponse(trustedChannels)
        }

        if (url.endsWith('/api/learning-resources/candidates/source-file')) {
          return jsonResponse({ sourceFile: '/workspace/local-source/youtube/video-candidates.csv' })
        }

        if (url.endsWith('/api/learning-resources/candidates/import-local')) {
          return jsonResponse({
            sourceFile: '/workspace/local-source/youtube/video-candidates.csv',
            candidatesDiscovered: 18,
            candidatesCreated: 18,
            candidatesUpdated: 0,
            candidatesSkipped: 0,
          })
        }

        if (url.endsWith('/api/learning-resources/candidates/401/accept')) {
          return jsonResponse({
            candidate: { ...videoCandidates[0], status: 'accepted', learningResourceId: 501, acceptedAt: '2026-06-30T13:00:00Z' },
            resource: {
              ...videos[0],
              id: 501,
              externalId: 'R-z2Hv-7nxk',
              title: 'Deep .NET: Writing async/await from scratch',
              creator: 'dotnet',
              url: 'https://www.youtube.com/watch?v=R-z2Hv-7nxk',
              embedUrl: 'https://www.youtube-nocookie.com/embed/R-z2Hv-7nxk',
              tags: ['async', 'csharp', 'dotnet', 'runtime'],
            },
          })
        }

        if (url.endsWith('/api/learning-resources/candidates/402/reject')) {
          return jsonResponse({
            ...videoCandidates[1],
            status: 'rejected',
            rejectionReason: 'Not a fit for the current study catalog.',
            rejectedAt: '2026-06-30T13:00:00Z',
          })
        }

        if (url.includes('/api/learning-resources/candidates/') && init?.method !== 'POST') {
          return jsonResponse(videoCandidates)
        }

        return Promise.resolve(new Response(null, { status: 404 }))
      }),
    )
  })

  afterEach(() => {
    cleanup()
    vi.unstubAllGlobals()
  })

  it('renders the local source reader from the API', async () => {
    const { container } = renderApp()

    expect(await screen.findByText('ASP.NET Core in Action')).toBeInTheDocument()
    expect(await screen.findByText('Middleware and request pipeline')).toBeInTheDocument()
    expect(await screen.findByText(/middleware forms a request pipeline/i)).toBeInTheDocument()
    expect(await screen.findByText('Code snippet')).toBeInTheDocument()
    expect(await screen.findByText('C#')).toBeInTheDocument()
    await waitFor(() => {
      expect(container.querySelector('.reader-code-block .hljs-keyword')).not.toBeNull()
    })
  })

  it('can trigger focused PDF ingestion', async () => {
    const user = userEvent.setup()
    renderApp()

    await user.click(await screen.findByRole('button', { name: /ingest focused pdfs/i }))

    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        'http://localhost:5105/api/sources/ingest-local',
        expect.objectContaining({ method: 'POST' }),
      )
    })
  })

  it('can generate and save study items for the selected chunk', async () => {
    const user = userEvent.setup()
    renderApp()

    expect(await screen.findByText('Middleware and request pipeline')).toBeInTheDocument()
    await user.click(await screen.findByRole('button', { name: /generate/i }))
    expect(await screen.findByText(/read the code snippet/i)).toBeInTheDocument()

    await user.click(await screen.findByRole('button', { name: /save 1/i }))

    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        'http://localhost:5105/api/study-items/',
        expect.objectContaining({ method: 'POST' }),
      )
    })
  })

  it('still exposes the curriculum catalog workflow', async () => {
    const user = userEvent.setup()
    renderApp()

    await user.click(await screen.findByRole('button', { name: /curriculum/i }))

    expect(await screen.findByText('Minimal API route groups')).toBeInTheDocument()
    expect(screen.getAllByText('ASP.NET Core API Engineering').length).toBeGreaterThan(0)
  })

  it('can run a study session attempt', async () => {
    const user = userEvent.setup()
    renderApp()

    await user.click(await screen.findByRole('button', { name: /study session/i }))
    expect(await screen.findByText('What does ASP.NET Core middleware do?')).toBeInTheDocument()

    await user.type(screen.getByPlaceholderText(/answer from memory/i), 'It builds the HTTP request pipeline.')
    await user.click(screen.getByRole('button', { name: /reveal answer/i }))
    expect(await screen.findByText(/it composes a request pipeline/i)).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /record attempt/i }))

    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        'http://localhost:5105/api/study-session/attempts',
        expect.objectContaining({ method: 'POST' }),
      )
    })
  })

  it('embeds curated videos and saves local watch notes', async () => {
    const user = userEvent.setup()
    const { container } = renderApp()

    await user.click(await screen.findByRole('button', { name: /^videos$/i }))
    expect(await screen.findAllByText('.NET 10 + C# 14 - Full Course 2026 [E-commerce MVC]')).toHaveLength(2)
    expect(await screen.findByText('Nick Chapsas')).toBeInTheDocument()
    expect(container.querySelector('iframe')?.getAttribute('src')).toBe('https://www.youtube-nocookie.com/embed/YbRe4iIVYJk')

    await user.click(screen.getByRole('button', { name: /import local channels/i }))
    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        'http://localhost:5105/api/learning-resources/channels/import-local',
        expect.objectContaining({ method: 'POST' }),
      )
    })

    expect(await screen.findByText('Deep .NET: Writing async/await from scratch')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /import candidates/i }))
    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        'http://localhost:5105/api/learning-resources/candidates/import-local',
        expect.objectContaining({ method: 'POST' }),
      )
    })

    await user.click(screen.getAllByRole('button', { name: /^accept$/i })[0])
    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        'http://localhost:5105/api/learning-resources/candidates/401/accept',
        expect.objectContaining({ method: 'POST' }),
      )
    })

    await user.click(screen.getAllByRole('button', { name: /^reject$/i })[1])
    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        'http://localhost:5105/api/learning-resources/candidates/402/reject',
        expect.objectContaining({ method: 'POST' }),
      )
    })

    await user.click(screen.getByRole('button', { name: /import curated videos/i }))
    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        'http://localhost:5105/api/learning-resources/import-seed',
        expect.objectContaining({ method: 'POST' }),
      )
    })

    await user.click(screen.getByRole('checkbox', { name: /watched/i }))
    await user.clear(screen.getByRole('spinbutton', { name: /progress minutes/i }))
    await user.type(screen.getByRole('spinbutton', { name: /progress minutes/i }), '15')
    await user.type(screen.getByPlaceholderText(/capture useful explanations/i), 'Strong overview of modern .NET structure.')
    await user.click(screen.getByRole('button', { name: /save video notes/i }))

    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        'http://localhost:5105/api/learning-resources/201/watch-state',
        expect.objectContaining({ method: 'PUT' }),
      )
    })
  })
})

function renderApp() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>,
  )
}

function jsonResponse(body: unknown) {
  return Promise.resolve(
    new Response(JSON.stringify(body), {
      status: 200,
      headers: { 'content-type': 'application/json' },
    }),
  )
}
