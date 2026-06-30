import type { ReactNode } from 'react'
import { BookOpen, Code2, Library, Repeat2 } from 'lucide-react'
import type { ModuleResponse } from '../features/catalog/types'
import type { SourceMaterialResponse } from '../features/sources/types'

export type WorkspaceView = 'sources' | 'study' | 'catalog'

type AppSidebarProps = {
  activeView: WorkspaceView
  completedSourceCount: number
  modules: ModuleResponse[]
  onSelectModule: (slug: string) => void
  onSelectView: (view: WorkspaceView) => void
  selectedModule: string
  sources: SourceMaterialResponse[]
}

export function AppSidebar({
  activeView,
  completedSourceCount,
  modules,
  onSelectModule,
  onSelectView,
  selectedModule,
  sources,
}: AppSidebarProps) {
  return (
    <aside className="border-b border-zinc-800 bg-[#0f1216] lg:border-b-0 lg:border-r">
      <div className="border-b border-zinc-800 px-5 py-5">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-md border border-cyan-800/70 bg-cyan-950/40">
            <Code2 className="h-5 w-5 text-cyan-300" />
          </div>
          <div>
            <div className="text-sm font-semibold text-slate-100">SFSS Platform</div>
            <div className="text-xs text-slate-500">Senior full-stack study</div>
          </div>
        </div>
      </div>

      <nav className="px-3 py-4">
        <SidebarButton
          active={activeView === 'sources'}
          icon={<Library className="h-4 w-4" />}
          label="Source library"
          onClick={() => onSelectView('sources')}
        />
        <SidebarButton
          active={activeView === 'study'}
          icon={<Repeat2 className="h-4 w-4" />}
          label="Study session"
          onClick={() => onSelectView('study')}
        />
        <SidebarButton
          active={activeView === 'catalog'}
          icon={<BookOpen className="h-4 w-4" />}
          label="Curriculum"
          onClick={() => onSelectView('catalog')}
        />

        {activeView === 'catalog' && (
          <>
            <div className="mt-5 px-3 text-xs font-medium uppercase tracking-wide text-slate-500">Modules</div>
            <div className="mt-2 space-y-1">
              <ModuleButton active={selectedModule === 'all'} label="All modules" onClick={() => onSelectModule('all')} />
              {modules.map((module) => (
                <ModuleButton
                  key={module.slug}
                  active={selectedModule === module.slug}
                  label={module.title}
                  meta={`${module.doneTopics}/${module.totalTopics}`}
                  onClick={() => onSelectModule(module.slug)}
                />
              ))}
            </div>
          </>
        )}

        {activeView === 'sources' && (
          <div className="mt-6 space-y-3 px-3 text-sm">
            <SidebarStat label="Sources" value={String(sources.length)} />
            <SidebarStat label="Readable" value={String(completedSourceCount)} />
            <SidebarStat label="Chunks" value={String(sources.reduce((total, source) => total + source.chunkCount, 0))} />
          </div>
        )}
      </nav>
    </aside>
  )
}

function SidebarButton({
  active,
  icon,
  label,
  onClick,
}: {
  active: boolean
  icon: ReactNode
  label: string
  onClick: () => void
}) {
  return (
    <button
      className={`focus-ring mb-1 flex w-full items-center gap-3 rounded-md px-3 py-2 text-left text-sm ${
        active ? 'bg-zinc-800 text-slate-50' : 'text-slate-400 hover:bg-zinc-900 hover:text-slate-100'
      }`}
      type="button"
      onClick={onClick}
    >
      {icon}
      {label}
    </button>
  )
}

function ModuleButton({
  active,
  label,
  meta,
  onClick,
}: {
  active: boolean
  label: string
  meta?: string
  onClick: () => void
}) {
  return (
    <button
      className={`focus-ring flex w-full items-center justify-between rounded-md px-3 py-2 text-left text-sm ${
        active ? 'bg-zinc-800 text-slate-50' : 'text-slate-400 hover:bg-zinc-900 hover:text-slate-100'
      }`}
      type="button"
      onClick={onClick}
    >
      <span className="truncate">{label}</span>
      {meta && <span className="ml-2 text-xs text-slate-500">{meta}</span>}
    </button>
  )
}

function SidebarStat({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between border-b border-zinc-800 pb-2">
      <span className="text-slate-500">{label}</span>
      <span className="font-medium text-slate-200">{value}</span>
    </div>
  )
}
