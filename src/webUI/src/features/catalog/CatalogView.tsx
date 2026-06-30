import { CheckCircle2, Clock3, Database, Search, SplitSquareVertical } from 'lucide-react'
import { EmptyState, Metric } from '../../components/shared'
import type { CatalogRollupsResponse, ModuleResponse, TaskType, TopicProgressStatus, TopicSearchResponse } from './types'

type CatalogViewProps = {
  activeTopics: TopicSearchResponse[]
  doneTopics: TopicSearchResponse[]
  hasData: boolean
  importPending: boolean
  modules: ModuleResponse[]
  onImport: () => void
  onProgressChange: (slug: string, status: TopicProgressStatus) => void
  onSearch: (value: string) => void
  progressPending: boolean
  rollups: CatalogRollupsResponse | undefined
  search: string
  selectedModuleTitle: string
  topics: TopicSearchResponse[]
  topicsError: boolean
  topicsLoading: boolean
}

export function CatalogView({
  activeTopics,
  doneTopics,
  hasData,
  importPending,
  modules,
  onImport,
  onProgressChange,
  onSearch,
  progressPending,
  rollups,
  search,
  selectedModuleTitle,
  topics,
  topicsError,
  topicsLoading,
}: CatalogViewProps) {
  return (
    <section className="min-w-0">
      <header className="border-b border-zinc-800 bg-[#0f1216] px-5 py-4 md:px-8">
        <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <h1 className="text-xl font-semibold tracking-tight text-slate-50">Catalog and progress</h1>
            <p className="mt-1 text-sm text-slate-500">
              {selectedModuleTitle}. Import a local seed, browse topics, and update study state.
            </p>
          </div>
          <button
            className="focus-ring inline-flex h-9 items-center justify-center rounded-md bg-cyan-500 px-3 text-sm font-medium text-slate-950 hover:bg-cyan-400 disabled:cursor-not-allowed disabled:opacity-60"
            type="button"
            disabled={importPending}
            onClick={onImport}
          >
            <Database className="mr-2 h-4 w-4" />
            {importPending ? 'Importing' : hasData ? 'Re-import seed' : 'Import local seed'}
          </button>
        </div>
      </header>

      <div className="grid gap-0 xl:grid-cols-[minmax(0,1fr)_340px]">
        <div className="min-w-0 px-5 py-5 md:px-8">
          <div className="mb-4 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
            <label className="relative block md:w-96">
              <Search className="pointer-events-none absolute left-3 top-2.5 h-4 w-4 text-slate-500" />
              <input
                className="focus-ring h-9 w-full rounded-md border border-zinc-800 bg-[#11151a] pl-9 pr-3 text-sm text-slate-100 placeholder:text-slate-600"
                placeholder="Search topics"
                value={search}
                onChange={(event) => onSearch(event.target.value)}
              />
            </label>
            <div className="text-sm text-slate-500">
              {topics.length} topic{topics.length === 1 ? '' : 's'}
            </div>
          </div>

          {topicsLoading ? (
            <EmptyState title="Loading catalog" detail="Reading modules and topics from the API." />
          ) : topicsError ? (
            <EmptyState title="API unavailable" detail="Start the backend on http://localhost:5105 and refresh." />
          ) : topics.length === 0 ? (
            <EmptyState
              title="No curriculum imported"
              detail="Use Import local seed to populate SQLite and exercise the catalog API."
            />
          ) : (
            <TopicTable
              onProgressChange={onProgressChange}
              progressPending={progressPending}
              topics={topics}
            />
          )}
        </div>

        <CatalogRollups
          activeCount={activeTopics.length}
          doneCount={doneTopics.length}
          moduleCount={modules.length}
          rollups={rollups}
        />
      </div>
    </section>
  )
}

function TopicTable({
  onProgressChange,
  progressPending,
  topics,
}: {
  onProgressChange: (slug: string, status: TopicProgressStatus) => void
  progressPending: boolean
  topics: TopicSearchResponse[]
}) {
  return (
    <div className="overflow-hidden rounded-md border border-zinc-800">
      <table className="w-full border-collapse text-left text-sm">
        <thead className="bg-[#11151a] text-xs uppercase tracking-wide text-slate-500">
          <tr>
            <th className="px-4 py-3 font-medium">Topic</th>
            <th className="hidden px-4 py-3 font-medium md:table-cell">Module</th>
            <th className="hidden px-4 py-3 font-medium lg:table-cell">Task types</th>
            <th className="px-4 py-3 font-medium">Progress</th>
          </tr>
        </thead>
        <tbody>
          {topics.map((topic) => (
            <tr key={topic.slug} className="border-t border-zinc-800 align-top">
              <td className="px-4 py-3">
                <div className="font-medium text-slate-100">{topic.title}</div>
                <div className="mt-1 max-w-2xl text-xs leading-5 text-slate-500">{topic.summary}</div>
              </td>
              <td className="hidden px-4 py-3 text-slate-400 md:table-cell">{topic.moduleTitle}</td>
              <td className="hidden px-4 py-3 lg:table-cell">
                <div className="flex flex-wrap gap-1.5">
                  {topic.taskTypes.map((taskType) => (
                    <span key={taskType} className="rounded border border-zinc-800 px-1.5 py-0.5 text-xs text-slate-400">
                      {taskTypeLabel(taskType)}
                    </span>
                  ))}
                </div>
              </td>
              <td className="px-4 py-3">
                <ProgressSelect
                  disabled={progressPending}
                  value={topic.status}
                  onChange={(status) => onProgressChange(topic.slug, status)}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function CatalogRollups({
  activeCount,
  doneCount,
  moduleCount,
  rollups,
}: {
  activeCount: number
  doneCount: number
  moduleCount: number
  rollups: CatalogRollupsResponse | undefined
}) {
  return (
    <aside className="border-t border-zinc-800 bg-[#0f1216] px-5 py-5 xl:border-l xl:border-t-0">
      <div className="grid grid-cols-3 gap-3 xl:grid-cols-1">
        <Metric icon={<SplitSquareVertical className="h-4 w-4" />} label="Modules" value={String(moduleCount)} />
        <Metric icon={<Clock3 className="h-4 w-4" />} label="Active" value={String(activeCount)} />
        <Metric icon={<CheckCircle2 className="h-4 w-4" />} label="Done" value={String(doneCount)} />
      </div>

      <div className="mt-6">
        <h2 className="text-sm font-semibold text-slate-100">Module rollups</h2>
        <div className="mt-3 space-y-3">
          {(rollups?.modules ?? []).map((module) => (
            <div key={module.slug}>
              <div className="flex items-center justify-between text-sm">
                <span className="truncate text-slate-300">{module.title}</span>
                <span className="text-xs text-slate-500">{module.doneTopics}/{module.totalTopics}</span>
              </div>
              <div className="mt-2 h-1.5 rounded bg-zinc-800">
                <div className="h-1.5 rounded bg-cyan-400" style={{ width: `${module.completionPercentage}%` }} />
              </div>
            </div>
          ))}
          {!rollups?.modules.length && (
            <p className="text-sm leading-6 text-slate-500">Import the local seed to see rollups.</p>
          )}
        </div>
      </div>
    </aside>
  )
}

function ProgressSelect({
  disabled,
  value,
  onChange,
}: {
  disabled: boolean
  value: TopicProgressStatus
  onChange: (status: TopicProgressStatus) => void
}) {
  return (
    <select
      className="focus-ring h-8 rounded-md border border-zinc-800 bg-[#11151a] px-2 text-xs text-slate-200"
      disabled={disabled}
      value={value}
      onChange={(event) => onChange(event.target.value as TopicProgressStatus)}
    >
      <option value="notStarted">Not started</option>
      <option value="inProgress">In progress</option>
      <option value="done">Done</option>
    </select>
  )
}

function taskTypeLabel(taskType: TaskType) {
  return taskType
    .replace('writeTests', 'write tests')
    .replace(/[A-Z]/g, (match) => ` ${match.toLowerCase()}`)
}
