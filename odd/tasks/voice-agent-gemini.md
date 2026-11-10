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

- [x] T1 - Microphone permissions + `Plugin.Maui.Audio` package (Android/iOS/MacCatalyst/Windows) | S4, S9 | inline | Evidence: `dotnet build -f net8.0-windows10.0.19041.0` and `-f net8.0-android` both OK (0 errors/warnings). Plugin.Maui.Audio pinned to **3.1.0** (4.0.0 requires net9.0; project targets net8.0). iOS/MacCatalyst build not run (Windows host). | commit: 2eacaf3
- [x] T2 - `IGeminiClient` (HttpClient, base64 audio + function calling, timeout + backoff, 429 handling, GEMINI_API_KEY via env) | S4, S5, S9 | inline | Evidence: `dotnet build -f net8.0-windows10.0.19041.0` OK (0 errors). Files: Models/VoiceCommand.cs, Models/VoiceCommandResult.cs, Services/Voice/IGeminiClient.cs, Services/Voice/GeminiVoiceClient.cs, Services/Voice/FunctionDeclaration.cs (schema helpers, needed by T2 signature). Registered `HttpClient` + `IGeminiClient` in DI. | commit: (this commit)
- [x] T3 - Tool schema + dispatch registry + 10 tools + confirmation gating + orchestrator glue | S2, S5, S7 | inline | Evidence: `dotnet build -f net8.0-windows10.0.19041.0` and `-f net8.0-android` OK (0 errors). Files: Models/ToolCallResult.cs (ToolCallResult, PendingConfirmation, VoiceAssistantResult), Services/Voice/FunctionDeclaration.cs (simplified builder: Create/Prop/EnumProp with object-level `required`), Services/Voice/IToolRegistry.cs, Services/Voice/ToolRegistry.cs (find_subject, add/update/delete task, add/update/delete subject, add/update/delete schedule), Services/Voice/IVoiceAssistant.cs, Services/Voice/VoiceAssistant.cs, DI registration in MauiProgram.cs. Absorbed old T4 (dispatcher+tools) and old T5 (orchestrator) because they are the same unit of behavior. | commit: (this commit)
- [x] T6 - Global FAB mic + listening UI, confirmation dialog with mutation summary | S3, S7 | inline | Evidence: `dotnet build` OK en `net8.0-windows10.0.19041.0` y `net8.0-android` (0 errors). Files: Views/MainPage.xaml (FAB `Border` con glifo FluentIcons `E720` + overlay de escucha/procesamiento), Views/MainPage.xaml.cs (toggle grabar/procesar, auto-stop 60s, confirmación por `DisplayAlert` por cada `PendingConfirmation`, refresh del tab activo), Services/Voice/IAudioRecorderService.cs (grabación WAV PCM mono 16 kHz con `ChannelType.Mono`/`BitDepth.Pcm16bit`), DI (`IAudioManager` + `IAudioRecorderService`). | commit: (this commit)
- [x] T7 - Error handling UX (429, timeout, malformed JSON, subject not found) | S4, S7 | inline | Evidence: build OK en ambos TFMs. `MainPage.ShowErrorAsync` mapea `GeminiRateLimitException` → límite gratuito, `TimeoutException` → respuesta lenta, `GeminiRequestException` → error del servicio, `HttpRequestException` → sin conexión, `InvalidOperationException` con `GEMINI_API_KEY` → falta configuración, permiso de micrófono denegado → ajustes del sistema. Los fallos de tool (materia no encontrada, JSON inválido, estado inválido) se muestran tal cual los devuelve `ToolCallResult.Failure`. | commit: (this commit)
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

L9 (2026-11-10): Implementation decisions for T2/T3. (a) T2 build fixes: registered a plain `HttpClient` singleton instead of `AddHttpClient()` (avoids adding Microsoft.Extensions.Http); fixed `choices[0]` indexing with pattern matching to `Dictionary<string, object>`; replaced `Array.Empty<ToolCall>()` with `new List<ToolCall>()`. (b) `FunctionDeclarationBuilder` rewritten: the earlier per-type helpers put `required` inside each property schema (invalid); now `Create(name, description, properties, params required)` emits object-level `required`. (c) Confirmation policy: only update/delete require explicit confirmation (S7 names deletes and subject/schedule edits); add_* tools execute immediately because they are additive. (d) `delete_subject` cascades schedules AND tasks via `IDatabaseService.DeleteSchedulesBySubjectAsync`/`DeleteTasksBySubjectAsync`, with both counts shown in the confirmation prompt (the manual UI path leaves orphans; the voice path is explicit and transparent). (e) No `GetScheduleById` exists, so update/delete schedule resolve through `IDatabaseService.GetAllSchedulesAsync()` (small personal dataset). (f) `update_task`/`update_schedule` recompute the change list and refuse with a clear message when no change was specified. (g) `TryParseDate` treats a date-only ISO value as end-of-day (23:59) so a spoken date without time does not immediately mark the task overdue. (h) Schedule time parsing accepts `H:mm`, `HH:mm` and seconds variants; day accepts 1..7 or Spanish names accent-insensitively.

L10 (2026-11-10): T6/T7 implementation. (a) BUG FIXED in `GeminiVoiceClient.ParseResponse`: the previous version deserialized into `Dictionary<string, object>` and then used `as List<object>` / `(string?)dict["args"]`; System.Text.Json materializes nested values as `JsonElement`, so `parts` was always null and every response fell through to "no contiene partes", and `args` would have thrown on cast. Rewritten with `JsonDocument`/`JsonElement`, `args` taken via `GetRawText()`. (b) Request shape corrected to documented Gemini fields: `systemInstruction.parts[].text` + `contents[].role=user` (previously the system text was mixed into user parts). (c) Retry loop rewritten: fresh `StringContent` per attempt, single increment per failure, `TaskCanceledException` without user cancellation mapped to `TimeoutException`, 429 mapped to `GeminiRateLimitException` without retry. (d) FAB lives in `MainPage` (the only container shared by all tabs) at bottom-right above the bottom nav; icon is FluentIcons `E720` (font verified to contain MDL2 codepoints). (e) Recording: 16 kHz mono PCM16 WAV so Gemini accepts it inline; auto-stop after 60 s to bound payload and runaway recordings; `PermissionStatus.Denied` blocks, `Unknown`/`Granted` attempt anyway (Windows permission reporting is unreliable). (f) Confirmation UX: one `DisplayAlert` per pending mutation with its prompt (which already includes the change summary/counts), then `ConfirmAsync` executes only the accepted ones. (g) After a successful mutation the active tab reloads; services already invalidate their caches. (h) Error mapping keeps provider detail out of the UI and gives actionable Spanish messages.
