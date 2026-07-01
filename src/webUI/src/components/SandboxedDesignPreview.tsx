type SandboxedDesignPreviewProps = {
  detail: string
  height?: string
  prompt: string
}

export function SandboxedDesignPreview({ detail, height = '420px', prompt }: SandboxedDesignPreviewProps) {
  return (
    <section className="overflow-hidden rounded-md border border-zinc-800 bg-[#0b0d10]">
      <div className="border-b border-zinc-800 px-3 py-2">
        <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Reference design</h3>
      </div>
      <iframe
        className="w-full bg-slate-950"
        referrerPolicy="no-referrer"
        sandbox=""
        srcDoc={previewDocument(prompt, detail)}
        style={{ height }}
        title="Reference design preview"
      />
    </section>
  )
}

function previewDocument(prompt: string, detail: string) {
  return `<!doctype html>
<html>
<head>
  <meta charset="utf-8" />
  <style>
    * { box-sizing: border-box; }
    body { margin: 0; background: #090c10; color: #e5eaf2; font-family: Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; }
    .shell { min-height: 100vh; padding: 24px; }
    .toolbar { display: flex; align-items: center; justify-content: space-between; gap: 12px; border-bottom: 1px solid #27272a; padding-bottom: 14px; }
    .brand { font-size: 13px; font-weight: 700; }
    .actions { display: flex; gap: 8px; }
    .button { border: 1px solid #3f3f46; border-radius: 6px; padding: 7px 10px; color: #d4d4d8; font-size: 12px; }
    .primary { border-color: #0891b2; background: #06b6d4; color: #071014; font-weight: 700; }
    .grid { display: grid; grid-template-columns: minmax(0, 1.45fr) minmax(280px, .85fr); gap: 18px; margin-top: 20px; }
    .panel { border: 1px solid #27272a; border-radius: 8px; background: #10151b; padding: 14px; }
    h1 { margin: 0; font-size: clamp(20px, 2.4vw, 32px); line-height: 1.18; }
    p { color: #aeb8c8; font-size: 14px; line-height: 1.6; }
    .list { margin-top: 18px; display: grid; gap: 10px; }
    .row { display: flex; align-items: center; justify-content: space-between; border: 1px solid #27272a; border-radius: 6px; padding: 11px; color: #d8dee9; font-size: 13px; }
    .status { color: #67e8f9; }
  </style>
</head>
<body>
  <main class="shell">
    <nav class="toolbar">
      <div class="brand">Study Product Interface</div>
      <div class="actions"><span class="button">Preview</span><span class="button primary">Ship</span></div>
    </nav>
    <section class="grid">
      <div class="panel">
        <h1>${escapeHtml(prompt)}</h1>
        <p>Use this surface to check hierarchy, density, alignment, contrast, and whether the interface feels like a real developer-learning product.</p>
        <div class="list">
          <div class="row"><span>Primary workflow</span><span class="status">clear</span></div>
          <div class="row"><span>Editor space</span><span class="status">protected</span></div>
          <div class="row"><span>Visual noise</span><span class="status">low</span></div>
        </div>
      </div>
      <aside class="panel">
        <h1>Review notes</h1>
        <p>${escapeHtml(detail)}</p>
      </aside>
    </section>
  </main>
</body>
</html>`
}

function escapeHtml(value: string) {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;')
}
