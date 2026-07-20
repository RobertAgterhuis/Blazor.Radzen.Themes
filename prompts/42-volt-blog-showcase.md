# Prompt 42 — "Volt" theme + editorial blog-showcase (mobile-first, scroll-driven)

Two deliverables in one: (A) theme family `volt` — a color/personality space no existing family touches; (B) a dedicated editorial showcase at `/blog`: a magazine-style site about professional projects, custom AI agents, prompts and skills — completely different LAYOUT from the app-shell showcases. Mobile-first, custom animations, a designed user journey. All house rules hold: tokens only, parity/bleed/contrast guards, WCAG 2.2 AA, reduced-motion kills all motion.

---

Copy below into Claude Code in the repo root:

---

# PART A — theme family "volt"

`volt-dark` (hero) + `volt-light` ("paper mode") per docs/THEMING.md, full parity.

- **volt-dark**: canvas warm graphite `#141412`-class (warm near-black, NOT blue-black — distinct from imperial/hoth), surfaces `#1a1a17` → `#212120` → `#2c2c28`; text warm off-white `#f4f3ee`, secondary `#c9c7bd`, muted `#8d8b80`; hairline `#2e2e29`, strong `#4a4a42`.
- **Accent (the identity): electric lime** `#c8f542`-class (tune vividness on the graphite), muted `#a3cc2e`; `--agt-on-accent: #1a2005` near-black — NEVER white on lime. Budget ≤5%: kinetic hero words, links-hover underline sweep, active states, focus, selection, progress bar.
- **Primary (interactive)**: desaturated warm bone/cream `#e8e5d8`-class fills with dark text (buttons read as paper chips on graphite) — an inversion no other family uses; interactive TEXT: lime-tinted `#d6e89a`-class, measured.
- **volt-light ("paper")**: cream canvas `#faf8f2`, ink text `#1c1c18`, lime works as fills/edges only (lime text on cream fails — text-use `#5a7016`+, measure), soft warm borders; the READING variant — article pages default to it via a per-page preference toggle ("Leesmodus").
- Semantics measured on both: success distinct from lime (use a cool `#3fa66a` — verify side-by-side!), warning amber, danger `#e05252`-class, info steel.
- **Personality**: display font = the boldest of the bundled set (Sora heavy) with tight leading and clamp()-based fluid scale up to 4.5rem; radius 0 on nav (rule) and near-0 elsewhere (editorial sharp) EXCEPT pill-shaped tags/chips (999px) — the contrast is the style; shadows nearly none (flat editorial); atmosphere: none — whitespace IS the atmosphere; charts lime+bone+steel.

# PART B — `/blog` editorial showcase ("ICT365 Journal"-achtig, eigen naam vrij, geen merkassets van derden)

## B1. Eigen root-layout, mobile-first

- Route prefix `/blog`, own layout (reuses App.razor head — theme css/fonts/interop; lesson learned): NO sidebar, NO topbar-app-chrome. Desktop: slim sticky header (wordmark, 4 links, theme/leesmodus-toggle). Mobile (<768px): header shrinks to wordmark + menu; primary nav is a BOTTOM TAB BAR in the thumb zone (Home, Projecten, Agents, Prompts, Skills) with safe-area-inset padding, 44px targets.
- Mobile-first CSS: single fluid column, `clamp()` type scale, container queries for the bento cards; desktop enhances to an asymmetric 12-col bento grid. Test at 360px, 768px, 1440px.
- Entry: "Blog-showcase" item in the demo sidebar (Getting started); the /blog pages force the volt family by default but respect the switcher (document the choice made).

## B2. De journey (elke stap ontworpen)

1. **Landing**: kinetic typography hero — headline woorden animeren woord-voor-woord in (translate+fade, 40ms stagger, once), één woord in lime met een sweep-underline; scroll-cue. Daaronder de featured-project bento (grote kaart + 4 kleine, hover lift).
2. **Projecten** (`/blog/projecten`): horizontaal scrollende project-reel op desktop (scroll-snap, edge-fade, drag), verticale stack op mobiel; projectkaart → detailpagina met View Transition (shared-element feel op de kaart-titel/afbeelding).
3. **AI Agents** (`/blog/agents`): agent-kaarten met een TERMINAL-animatie — een prompt/response typt zich uit (IntersectionObserver-getriggerd, eenmalig, ≤3 regels, cursor-blink); seeded met fictieve maar realistische agents (bijv. "GraphDroid — M365 rapportage-agent"). Geen echte API-calls.
4. **Prompts** (`/blog/prompts`): DE dogfood-pagina — laad de repo's eigen `prompts/*.md` (build-time embed of statisch gekopieerd) als doorzoekbare bibliotheek: filterbare kaarten (titel, nummer, onderwerp-tags), detailweergave met syntax-net opgemaakte markdown en een "Kopieer prompt"-knop (toast). Zoekveld met debounce.
5. **Skills** (`/blog/skills`): skills-matrix die on-scroll invult (bars/dots animeren once), gegroepeerd (Azure, M365, AI/Agents, DevOps, Blazor).
6. **Artikel-leeservaring** (`/blog/artikel/{slug}`, 2–3 seeded artikelen): leesvoortgang-hairline bovenin (lime), geschatte leestijd, zwevende mini-TOC (desktop) die met scroll-spy meeloopt, brede maat (65ch), pull-quotes in display-font, code-blokken met kopieerknop, "volgend artikel"-kaart onderaan; leesmodus-toggle (volt-light) prominent.
7. **Over/contact**: compacte pagina met social-links (generieke iconen).

## B3. Animatie-regels

Scroll-driven via IntersectionObserver (of de Scroll-Driven Animations API met fallback — feature-detect), transform/opacity only, entrance-animaties éénmalig per sessie, 60fps (trace bij verificatie), ALLES uit onder `prefers-reduced-motion` (content direct zichtbaar — nooit content verstopt achter een animatie die niet afspeelt). Geen parallax op mobiel. Terminal-animatie pauzeert buiten viewport.

## B4. Kwaliteit

Tokens only (bleed audit clean — volt-tokens incl. nieuwe editorial-tokens zoals leesmaat/type-scale in pariteit); contrast-sweep over /blog in beide varianten; a11y: landmark-structuur, tab bar met aria-current, skip-link, artikel-heading-hiërarchie, terminal-animatie aria-hidden met statisch tekstalternatief; bUnit smoke per blogpagina + gedragstest (prompt kopiëren toont toast; filter filtert).

## Verificatie

`dotnet build -c Release` zero warnings; `dotnet test` groen. Walk de volledige journey op 360px én 1440px in volt-dark en leesmodus; check View Transitions, terminal-animatie, prompts-bibliotheek gevuld vanuit de echte prompts-map, bottom tab bar met safe-area. Draai contrast-sweep + bleed audit. Rapporteer: de journey-stappen met wat er animeert, de mobile-first beslissingen, en hoe de prompts-map is ingeladen.
