# Skill Registry

**Delegator use only.** Any agent that launches sub-agents reads this registry to resolve compact rules, then injects them directly into sub-agent prompts. Sub-agents do NOT read this registry or individual SKILL.md files.

## User Skills

| Trigger | Skill | Path |
|---------|-------|------|
| Creating new AI skills | skill-creator | C:\Users\Lucas\.config\opencode\skills\skill-creator\SKILL.md |
| Go tests, Bubbletea TUI testing | go-testing | C:\Users\Lucas\.config\opencode\skills\go-testing\SKILL.md |

## Compact Rules

Pre-digested rules per skill. Delegators copy matching blocks into sub-agent prompts as `## Project Standards (auto-resolved)`.

### go-testing
- Use `teatest` package for Bubbletea TUI testing
- `app.Run()` for programmatic invocation, `app.Start()` for async
- Capture output with `teatest.NewApp(*testingM)` pattern
- Use `teatest.WithTimeout()` for async test scenarios
- Mock stdin/stdout/stderr via `ui.StdinPipe()` and capture via `ui.ReadStdout()`
- `require.Equal()` assertions from standard testing

### skill-creator
- Follow Agent Skills spec format
- YAML frontmatter: name, description, metadata
- Include: Purpose, When to Run, What to Do, Rules
- Glob skills from known directories
- Generate Compact Rules (5-15 lines, actionable, no fluff)
- Write `.atl/skill-registry.md` + save to engram

## Project Conventions

No project-level conventions found (new project).

## Notes

- No .NET MAUI-specific skills detected in the global skills directory
- Project is new — conventions will be established during SDD workflow
- All SDD skills (sdd-*) are excluded from this registry (workflow skills)