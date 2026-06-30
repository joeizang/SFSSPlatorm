import type { ReactNode } from 'react'
import { Circle } from 'lucide-react'

export function EmptyState({ title, detail }: { title: string; detail: string }) {
  return (
    <div className="rounded-md border border-zinc-800 bg-[#0f1216] px-5 py-8">
      <div className="flex items-start gap-3">
        <Circle className="mt-0.5 h-4 w-4 text-slate-500" />
        <div>
          <h2 className="text-sm font-medium text-slate-100">{title}</h2>
          <p className="mt-1 text-sm text-slate-500">{detail}</p>
        </div>
      </div>
    </div>
  )
}

export function Metric({ icon, label, value }: { icon: ReactNode; label: string; value: string }) {
  return (
    <div className="rounded-md border border-zinc-800 bg-[#11151a] p-3">
      <div className="flex items-center gap-2 text-slate-500">
        {icon}
        <span className="text-xs font-medium uppercase tracking-wide">{label}</span>
      </div>
      <div className="mt-2 text-2xl font-semibold text-slate-50">{value}</div>
    </div>
  )
}

export function Badge({ children }: { children: ReactNode }) {
  return <span className="rounded border border-zinc-800 px-1.5 py-0.5 text-xs text-slate-400">{children}</span>
}

export function StatusPill({ label, value }: { label: string; value: number }) {
  return (
    <span className="rounded border border-zinc-800 bg-[#11151a] px-2 py-1">
      {label}: <span className="text-slate-200">{value}</span>
    </span>
  )
}
