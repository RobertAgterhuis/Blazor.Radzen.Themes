# Prompt 27 — Showcase-shell reparatie (/app layout kapot)

Observed on /app (plum-dark): the sidebar OVERLAYS the content (a giant clipped "92" metric hides behind it), the dashboard renders as unstyled fragments scattered down an empty black page, notifications appear INLINE in the page flow as bare labels ("NOTIFICATIONSINFO/SUCCESS/WARNING") instead of positioned toasts, the "Thema" label in the topbar is danger-red, and nav items are filled gray blocks again. This is a structural layout + CSS-delivery failure, not styling polish.

---

Copy below into Claude Code in the repo root:

---

Fix the Werkorders showcase shell (`/app/*`) in `samples/Agterhuis.Ui.Demo`. Diagnose in this order; fix root causes.

## 1. Layout structure (sidebar overlays content)

Inspect the showcase layout component. It must use a real grid: `RadzenLayout` + `RadzenHeader` + `RadzenSidebar` + `RadzenBody` (like the working catalog MainLayout — compare side by side), so the body is OFFSET next to the sidebar, responsive collapse included. If a custom flex/absolute construction was hand-rolled, replace it with the RadzenLayout structure. Content must never render under the sidebar; verify at 1280px and 1920px and with the sidebar collapsed.

## 2. Scoped CSS not applied (unstyled fragments everywhere)

The metric cards, hero, lists and typography render as bare HTML. Check: (a) do the new showcase pages/layout have their `.razor.css` files and do the class names match the markup; (b) is the demo's scoped bundle (`Agterhuis.Ui.Demo.styles.css`) actually referenced in the App.razor used by the /app layout — if the showcase uses its own root layout it may bypass the head that loads the bundle and theme CSS entirely (that would also explain the broken look); the showcase layout must reuse the SAME App.razor head (theme css, fonts, styles bundle, theme-interop, anti-FOUC). (c) stale build artifacts: clean + rebuild so the scoped bundle regenerates. Fix whichever applies and state which one it was.

## 3. Notifications render inline instead of as toasts

The RadzenNotification host must be mounted ONCE at the layout level with the standard Radzen positioning CSS active — currently notifications flow inline where triggered. Verify RadzenComponents hosts (Notification, Dialog, Tooltip, ContextMenu) are present exactly once in the showcase layout (not per page, not missing), and that toasts appear top-right, themed (glass surface, semantic edge, accent progress line), auto-dismissing.

## 4. Token misuse in the showcase chrome

- "Thema" label uses the danger red token — must be text-secondary like the catalog header.
- Nav items: idle = transparent (the same rule as the main sidebar — reuse the RCL `_navigation.css` classes instead of custom showcase nav styling), hover = alpha tint, active = accent left edge + surface fill. The uppercase group headers keep muted styling.
- Sweep the showcase pages for any other literals/misused tokens (bleed audit must stay clean — run it).

## 5. Dashboard content integrity

After the layout/CSS fixes, verify the dashboard actually composes: 4 metric cards in a responsive grid (count-up works, numbers not clipped), charts sized inside cards, "vandaag" list and notifications panel styled, no orphan text fragments. Fix remaining page-level issues found.

## Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (showcase smoke tests + bleed audit). Walk ALL showcase pages (dashboard, werkorders incl. dialog CRUD + toast, planning, klanten, rapportage, instellingen) in plum-dark, hoth-light and imperial-dark at two viewport widths; sidebar collapse works; toasts positioned; nothing under the sidebar. Report per section (1–5) the root cause found and the fix.
