# Prompt 30 — Meldingen-flyout + showcase-restpunten (planning-pagina audit)

Observed on /app/planning (Plum Ink, donkere variant actief): the notification bell's "Recente meldingen" renders as BARE TEXT floating over the page without any panel — no surface, no border, overlapping the "Tot" date input. Additionally: the whole showcase renders LIGHT styling while the dark variant is active, the page kicker still says "COMPONENT CATALOG", the scheduler is in English (MON/TUE, WEEK/MONTH/DAY) duplicating the custom Dutch WEEK/MAAND/DAG toggle, and the topbar shows a broken purple square (avatar?). Fix root causes.

---

Copy below into Claude Code in the repo root:

---

Fix the following in the Werkorders showcase (`/app/*`).

## 1. Notification bell flyout (the mess)

Rebuild "Recente meldingen" as a proper themed flyout panel:
- Anchored dropdown under the bell (Radzen Popup or the overlay pattern used by the theme switcher): surface-2/glass panel, hairline border, shadow-md, width ~22rem, max-height with internal scroll after ~6 items, z-index ABOVE all page content (it currently sits under/over inputs incorrectly).
- Item anatomy: intent icon (nieuw/onderweg/afgerond/klantvraag → semantic icon + color, never color alone), title (weight 500), one-line detail (muted, ellipsis), relative tijd ("2 u geleden"); unread dot; "Alles gelezen" action in the panel header; empty state ("Geen nieuwe meldingen") via AgtEmptyState-style.
- Behavior: opens/closes on bell click, closes on outside click and Escape, focus management (focus into panel, restore to bell), aria-label + `aria-live="polite"` count on the bell badge.
- The bell badge shows the unread count; opening marks visible items read (in-memory).

## 2. Showcase ignores the dark variant (bleed — again)

The entire /app UI renders light (white header, sidebar, scheduler) while plum-dark is active. The showcase pages/layout are using hard-coded light colors or missing token references. Run the token-bleed audit scoped to the showcase files, fix every violation (tokens only), and verify /app follows ALL families in both variants. The runtime probe element list gets three showcase elements added (app header, scheduler surface, notification panel) so this cannot regress.

## 3. Wrong kicker + page header

Showcase pages must NOT say "COMPONENT CATALOG" — that's the catalog's kicker. App pages use the app context: kicker "WERKORDERS" (or a breadcrumb Werkorders → Planning), consistent on every showcase page.

## 4. Scheduler localization + duplicate controls

- Set the scheduler (and all showcase dates) to Dutch: day/month names (MA/DI/WO... of maandag/dinsdag), date range header in nl-NL format. Use the Radzen culture/localization support (`DefaultCulture` option already exists — wire it through) rather than string-replacing.
- Remove the duplication: ONE view switcher. Either use the custom Dutch WEEK/MAAND/DAG SelectBar and hide the scheduler's built-in EN buttons, or localize the built-in ones and drop the custom bar — pick one, apply consistently.
- The date-range filters (Van/Tot) must actually filter the scheduler range; verify.

## 5. Topbar details

- The purple square is a broken avatar/logo image — replace with initials-avatar (Gravatar fallback) or a proper asset; no broken images.
- "Thema" label: text-secondary token (it renders red/danger again in this shell — same fix as prompt 27 §4, verify it landed for the showcase header).
- Sidebar: remove the large empty gap above the nav items (the nav list should start right under the "Navigatie" header block).

## Verification

`dotnet build -c Release` zero warnings; `dotnet test` green (bleed audit incl. new probe elements, showcase smoke tests). Walk /app in plum-dark, autotaalglas-light (if present) and hoth-light: notification flyout opens as a themed panel above content and closes correctly, dark variant actually renders dark, Dutch scheduler with a single view switcher, no broken images, kicker correct on every page. Report root cause per section.
