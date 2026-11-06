# Voice Agent Gemini - Calendario-Estudiantil

Objective: integrate a speech-to-text + NLU voice agent (Google Gemini free tier, function calling) into the MAUI app so the user can manage tasks, subjects and schedules by voice.
Branch: `feature/voice-assistant-gemini` (from `master`)
Delivery strategy: `ask-on-risk`
Test runner: `dotnet test` (no test project exists yet - see S8)

## Specs

S1. Authorized goal (user's original request, verbatim): "podemos ver cuales son las posibilidades de agregar un agente de iA con interpretacion de voz a esta aplicacion? la idea es que se pueda aregar tareas materias o modificar horarios ect con la voz"

S2. V1 scope (user decision, verbatim intent): "Núcleo + mutaciones principales" - add/modify/delete tasks, subjects and schedules. Conversational Q&A and proactive suggestions are OUT of V1.

S3. Microphone placement (user decision, verbatim intent): "habria que hacer una parte mas general" / "el microfono este en una parte mas general" -> a global floating mic entry point reachable from all main pages (not tied to one page).

S4. STT+NLU provider decision (user decision): "vamos directo con gemini" - Google Gemini (free tier) with audio inline + JSON/function-calling output. Whisper.net is explicitly deferred ("dejamos para despues lo de whisper").

S5. NLU output contract: field-explicit JSON (option 1), `additionalProperties: false`. Intents resolved by Gemini function calling/tool dispatch, not by a fixed JSON triple hardcoded per operation.

S6. Provider preferences and rejections (user decision context): OpenRouter `:free` kept as possible textual fallback. apinex rejected by default (third-party reseller proxy, unstable free pools, no STT). OpenAI/Azure STT declined (cost).

S7. Strict validation on every mutation tool; destructive operations (deletes, subject/schedule edits) require an explicit user confirmation in the UI before executing. Internal lookup helpers (e.g. FindSubjectByName) are in scope for V1; user-facing conversational queries are out of scope.

S8. No test project exists in the repo today. V1 includes unit tests for the parser/dispatcher; a full test suite is not retrofitted.

S9. API keys MUST NOT be hardcoded or committed. `GEMINI_API_KEY` comes from environment/user secrets in development.

## Tasks

- [x] T1 - Microphone permissions + `Plugin.Maui.Audio` package (Android/iOS/MacCatalyst/Windows) | S4, S9 | inline | Evidence: `dotnet build -f net8.0-windows10.0.19041.0` and `-f net8.0-android` both OK (0 errors/warnings). Plugin.Maui.Audio pinned to **3.1.0** (4.0.0 requires net9.0; project targets net8.0). iOS/MacCatalyst build not run (Windows host). | commit: see branch
- [ ] T2 - `IGeminiClient` (HttpClient, base64 audio, timeout + backoff, 429 handling) | S4, S5, S9 | pending | WAITING: user authorized work "hasta T1" only (L7)
- [ ] T3 - Function-calling tool schema (mirror of TaskService/SubjectService operations) | S2, S5, S7 | route: pending
- [ ] T4 - Tool dispatcher + tools + internal lookups | S2, S7 | route: pending
- [ ] T5 - `IVoiceAgent` orchestrator (listen -> LLM -> confirm -> dispatch) | S1, S2 | route: pending
- [ ] T6 - Global FAB mic + listening UI, confirmation dialog with mutation summary | S3, S7 | route: pending
- [ ] T7 - Error handling UX (429, timeout, malformed JSON, subject not found) | S4, S7 | route: pending
- [ ] T8 - Unit tests for parser/dispatcher + manual end-to-end verification | S8 | route: pending

Forecast: ~400 authored lines per work unit slice; full feature estimated 1500-2500 authored changed lines (delivery review before each commit slice).

## Log

L1 (verbatim, 2026-11-06): "C:\Users\Lucas\source\repos\Calendario-Estudiantil podemos ver cuales son las posibilidades de agregar un agente de iA con interpretacion de voz a esta aplicacion? la idea es que se pueda aregar tareas materias o modificar horarios ect con la voz"

L2 (2026-11-06): Investigation - app is .NET 8 MAUI, SQLite local, no backend, no existing AI. Researched STT options (Azure Speech, Whisper.net, Gemini audio inline) and NLU options (GPT/Azure/Gemini JSON mode, OpenRouter free tier, apinex). Rates verified: OpenRouter free = 20 RPM/50 RPD (<$10 credits), Gemini free = ~15 RPM/1500 RPD, apinex = third-party proxy with 10-20M daily tokens but poor stability/privacy culture.

L3 (2026-11-06): Decision - go direct with Google Gemini free tier per user "vamos directo con gemini y dejamos para despues lo de whisper".

L4 (2026-11-06): Decision - V1 scope "Nucleo + mutaciones principales" (add/update/delete tasks, subjects, schedules). User questions/suggestions explicitly out of scope. Global mic entry point (FAB) per user "habria que hacer una parte mas general".

L5 (2026-11-06): Decision - field-explicit JSON (option 1) over generic entities; function calling with tools for extensibility ("tareas materias u horarios ... modificarlos"). Internal lookups (FindSubjectByName) in scope; user-facing conversational queries out.

L6 (2026-11-06): User authorized implementation through T1 only. Branch created from master: `feature/voice-assistant-gemini`.

L7 (2026-11-06): Implementation decision - Plugin.Maui.Audio 4.0.0 does not support net8.0 (net9-only); pinned 3.1.0. RECORD_AUDIO permission added to Android manifest; NSMicrophoneUsageDescription added to iOS and MacCatalyst Info.plist; microphone DeviceCapability added to Windows Package.appxmanifest. Build verified for net8.0-windows10.0.19041.0 and net8.0-android (0 errors); iOS/MacCatalyst not built on Windows host.

L8 (2026-11-06): Engram mirror for topic `odd/voice-agent-gemini/tasks` FAILED twice with `ambiguous_project` - the Engram server runs with cwd=C:\Users\Lucas (multiple git repos); available_projects listed only `%userprofile%.pyenv` and `engram`. Mirror is PENDING; the on-disk document is the source of truth until the engram server runs from a project context or the repo gains `.engram/config.json`. NOT marked as persisted.
