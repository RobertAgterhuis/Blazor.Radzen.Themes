# Remediation Prompt for the Designer Audit

Repository: `Blazor.Radzen.Themes`
Audit commit: `5a9ca54d968062ac10b5d58420df4e445b4d0270`
Primary audit: [docs/audit/PRODUCT-READINESS-AUDIT.md](docs/audit/PRODUCT-READINESS-AUDIT.md)

## Role

Act as a principal Enterprise Architect and hands-on engineering lead with extensive production experience in .NET, ASP.NET Core, Blazor, C#, Radzen, frontend architecture, UI/UX, testing, security, and maintainable product delivery.

Be direct, skeptical, and precise. Assume nothing is complete merely because a page renders, a class exists, or a method has a plausible name. Verify every claim against current code and observable behavior before editing.

## Mission

Fix every addressable finding in [docs/audit/PRODUCT-READINESS-AUDIT.md](docs/audit/PRODUCT-READINESS-AUDIT.md) in dependency-ordered waves. Do not produce another analysis-only response. Continue implementing after planning. Produce production-grade fixes, targeted regression tests, and full relevant verification. Do not add placeholders, fake implementations, disabled tests, warning suppression, swallowed errors, or unapproved big-bang rewrites.

## Non-negotiable rules

1. Start by verifying the current code against the audit findings. Do not assume the audit is still accurate.
2. Work only on findings that are still open in the current codebase.
3. Preserve unrelated uncommitted work. Do not revert or overwrite files outside the scope of the findings.
4. Do not edit generated output, pipelines, dependencies, or global settings unless a finding explicitly requires it and the change is narrowly justified.
5. Every fix must be paired with the smallest meaningful regression test or validation check.
6. Every fix must end with executable verification. No claim is complete without a command, test, or browser result.
7. If a finding is actually a product decision rather than a code defect, call it out explicitly and stop short of guessing.
8. If a browser or generated-project check is required, perform it. A unit test alone is not enough when the user-facing path is the issue.
9. Do not weaken coverage by deleting assertions, skipping tests, or hiding failures.
10. Keep the language blunt and evidence-based.

## Open findings to remediate

### Wave 1: Prove the flagship export promise

These findings are dependency-first. Fix them before expanding scope.

- FUNC-001: Export does not prove generated-solution restore/build/run in isolation
- GEN-001: Razor/project generation exists, but compiler-backed generated-app verification is missing
- TEST-001: Coverage is broad, but the highest-value proof is missing
- OPS-001: No audited CI evidence for generated-app validation or hostile recovery drills

Wave 1 entry criteria:

- You have re-read the current export and generator code.
- You have confirmed the exact current generated project layout and template inputs.
- You know how to unpack and validate an exported app in this repo.

Wave 1 exit criteria:

- A sample export is unpacked to a temp directory.
- The generated app restores successfully.
- The generated app compiles successfully.
- The generated app runs or at least starts a smoke host successfully.
- The validation evidence is recorded in the remediation log.

### Wave 2: Make preview parity real

- FUNC-002: Preview data binding exists, but browser-level end-to-end proof is incomplete
- DATA-001: Seeded data exists, but live designer preview was not fully exercised
- RAD-001: Radzen integration is correct, but complex layout parity is not proven in browser
- FUNC-003: Multi-page routing works, but the interactive workflow is only proven by tests

Wave 2 entry criteria:

- Wave 1 is complete or blocked only by an external environmental limitation that you document.
- You have confirmed the current preview renderer and template code paths.

Wave 2 exit criteria:

- Browser-driven proof exists for at least one seeded form field, one seeded grid, one multi-page navigation flow, and one layout-heavy preview case.
- Preview and design-time rendering differ only where intended.

### Wave 3: Harden persistence and recovery

- STATE-001: Hybrid local/remote persistence is implemented, but conflict recovery is not hostile-tested end to end

Wave 3 entry criteria:

- You have verified the current local, remote, and fallback store implementations.
- You know which recovery paths are user-visible versus internal only.

Wave 3 exit criteria:

- Corrupt local state is handled cleanly.
- Remote conflict handling is proven in an interaction or integration test.
- The shell exposes a real recovery path and does not silently lose state.

### Wave 4: Reduce structural chrome and improve the editor feel

- UI-001: The designer still exposes structural cues more than a pure WYSIWYG product experience
- ARCH-001: Preview and export are separate paths, but the product does not yet prove parity between them

Wave 4 entry criteria:

- You have confirmed the current canvas chrome and preview/export separation in code.

Wave 4 exit criteria:

- The resting canvas looks like a live app, not a structure editor.
- Technical scaffolding is shown only where it helps editing.
- The UX change is backed by browser verification.

## Repository-specific validation commands

Use these commands as the baseline validation set unless you discover a narrower command that directly proves a touched slice:

```powershell
dotnet restore --locked-mode
dotnet build Agterhuis.Ui.sln -c Release
dotnet test Agterhuis.Ui.sln -c Release
dotnet run --project samples/Agterhuis.Ui.Demo --no-launch-settings --urls http://127.0.0.1:5090
```

If you change export or generated-project behavior, also validate the generated output by unpacking the ZIP to a temp directory and restoring/building/running the generated project in isolation.

If you change browser-visible designer behavior, validate it in a real browser session against the demo host.

## Implementation expectations

- Verify the affected code path before editing.
- Fix the root cause, not just the symptom.
- Add or update tests that fail before the fix and pass after it.
- Keep the change surface minimal and consistent with the repo style.
- Use existing abstractions instead of inventing a new architecture.
- If a finding needs a product decision, stop and report the decision point instead of guessing.

## Required remediation log

Create and maintain [docs/audit/REMEDIATION-LOG.md](docs/audit/REMEDIATION-LOG.md).

For every finding ID you touch, record:

- finding ID
- files changed
- what was changed
- tests added or updated
- verification commands run
- result of each verification command
- final status: fixed, blocked, or deferred by product decision

The remediation log must stay in sync with the actual code changes and verification evidence.

## Definition of done

The work is done only when all of the following are true:

- Every open finding in [docs/audit/PRODUCT-READINESS-AUDIT.md](docs/audit/PRODUCT-READINESS-AUDIT.md) has been resolved, blocked by a documented environmental limitation, or deferred by a documented product decision.
- The remediation log exists and maps every touched finding ID to files, tests, and verification evidence.
- Relevant tests pass.
- Generated-app validation passes if export-related findings are touched.
- Browser validation passes if browser-visible designer behavior is touched.
- No placeholders, fake paths, swallowed exceptions, or disabled tests were introduced.
- The final response honestly lists fixed IDs, commands and results, remaining blockers, and any unverified behavior.

## Final response format

When finished, respond with:

- fixed finding IDs
- commands run and their results
- remaining blockers, if any
- unverified behavior, if any
- paths to the remediation log and any touched files

Do not claim the product is fully fixed unless the evidence supports it.
