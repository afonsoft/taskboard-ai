# Tailwind Theme Refresh Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `executing-plans` or subagent-driven execution to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Redesign the Taskboard Blazor UI with a cohesive Tailwind CSS design system, persistent light/dark theme toggle, and production-quality layouts for login, board, settings, and skills pages.

**Architecture:** Add Tailwind CSS as the single source of styling, keep MudBlazor providers (theme, dialog, snackbar) while replacing component usage with Tailwind-styled markup. Define CSS custom properties for light/dark tokens in `site.css`, wire the existing settings `Theme` value to a `<html data-theme="...">` attribute, and refactor the four primary surfaces: `MainLayout`, `Login`, `BoardView`, `Settings`, and `Skills`.

**Tech Stack:** .NET 10 Blazor Server, MudBlazor (providers only), Tailwind CSS 4.x CLI, CSS custom properties.

---

## File Structure

| File | Responsibility |
|------|----------------|
| `src/Taskboard.Server/wwwroot/css/site.css` | Tailwind base, custom theme tokens, component utilities |
| `src/Taskboard.Server/wwwroot/index.html` | Tailwind CDN/script reference, `data-theme` init script |
| `src/Taskboard.Blazor/Layout/MainLayout.razor` | Shell layout, top bar, sidebar, theme toggle |
| `src/Taskboard.Blazor/Layout/NavMenu.razor` | Navigation links with Tailwind |
| `src/Taskboard.Blazor/Components/Pages/Login.razor` | Login form with Tailwind styling |
| `src/Taskboard.Blazor/Components/Pages/Settings.razor` | Settings form and agents table |
| `src/Taskboard.Blazor/Components/Pages/Skills.razor` | Skills table with search |
| `src/Taskboard.Blazor/Components/BoardView.razor` | Kanban board and columns |
| `src/Taskboard.Blazor/Components/Shared/EmptyState.razor` | Empty state component |
| `src/Taskboard.Blazor/Components/Shared/Loading.razor` | Loading state component |
| `src/Taskboard.Blazor/Components/TaskCard.razor` | Task card component |

---

### Task 1: Add Tailwind CSS to the project

**Files:**
- Modify: `src/Taskboard.Server/wwwroot/index.html`
- Modify: `src/Taskboard.Server/wwwroot/css/site.css`
- Modify: `src/Taskboard.Blazor/Layout/MainLayout.razor`
- Create: `src/Taskboard.Server/package.json` (optional local build tooling)

- [ ] **Step 1: Reference Tailwind in `index.html`**

Replace the `<style>` block with a Tailwind CDN include and a small init script. Keep the `<base href="/">` and MudBlazor JS/CSS includes.

```html
<!DOCTYPE html>
<html lang="pt-BR" data-theme="dark">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Taskboard</title>
    <base href="/">
    <script src="https://cdn.tailwindcss.com"></script>
    <script>
        tailwind.config = {
            darkMode: 'class',
            theme: {
                extend: {
                    colors: {
                        primary: { 500: '#776be7', 600: '#5a4be2' },
                        surface: { 100: '#f7f7f8', 800: '#1e1e2e', 900: '#11111b' },
                    }
                }
            }
        };
    </script>
    <link href="css/site.css" rel="stylesheet">
    <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet">
</head>
<body class="bg-surface-100 text-slate-900 dark:bg-surface-900 dark:text-slate-100 antialiased">
    <div id="app" class="h-screen">
        <!-- Blazor app mounts here; fallback to index.html not needed for server -->
    </div>
    <script src="_framework/blazor.web.js"></script>
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>
</body>
</html>
```

- [ ] **Step 2: Write `site.css` with design tokens**

Replace the entire contents of `site.css` with Tailwind directives and custom properties for both themes.

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

:root {
    --color-primary: #776be7;
    --color-primary-dark: #5a4be2;
    --color-bg: #f7f7f8;
    --color-surface: #ffffff;
    --color-surface-elevated: #ffffff;
    --color-text: #1f2937;
    --color-text-muted: #6b7280;
    --color-border: #e5e7eb;
    --color-accent: #f59e0b;
}

[data-theme="dark"] {
    --color-bg: #0b0b10;
    --color-surface: #16161e;
    --color-surface-elevated: #1e1e2a;
    --color-text: #e2e4e9;
    --color-text-muted: #9ca3af;
    --color-border: #2a2a3a;
    --color-accent: #38bdf8;
}

html, body, #app {
    height: 100%;
}

@layer components {
    .btn-primary {
        @apply inline-flex items-center justify-center rounded-lg bg-primary-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-primary-500 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2;
    }
    .btn-secondary {
        @apply inline-flex items-center justify-center rounded-lg bg-surface-elevated px-4 py-2.5 text-sm font-semibold text-text ring-1 ring-border transition hover:bg-surface focus:outline-none focus:ring-2 focus:ring-primary-500;
    }
    .card {
        @apply rounded-xl bg-surface-elevated p-5 shadow-sm ring-1 ring-border;
    }
    .input {
        @apply w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text placeholder:text-text-muted focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500;
    }
    .nav-link {
        @apply flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-text-muted transition hover:bg-surface-elevated hover:text-text;
    }
    .nav-link.active {
        @apply bg-primary-500/10 text-primary-500;
    }
    .kanban-column {
        @apply flex w-72 shrink-0 flex-col rounded-xl bg-surface-elevated p-3 ring-1 ring-border;
    }
    .task-card {
        @apply cursor-grab rounded-lg bg-surface p-3 shadow-sm ring-1 ring-border transition hover:shadow-md;
    }
}
```

- [ ] **Step 3: Verify the build still compiles**

Run `dotnet build src/Taskboard.Server/Taskboard.Server.csproj -c Release` and confirm 0 errors.

---

### Task 2: Refactor `MainLayout` and `NavMenu`

**Files:**
- Modify: `src/Taskboard.Blazor/Layout/MainLayout.razor`
- Modify: `src/Taskboard.Blazor/Layout/NavMenu.razor`

- [ ] **Step 1: Rewrite `MainLayout.razor` with Tailwind shell**

```razor
@inherits LayoutComponentBase
@inject IHttpContextAccessor HttpContextAccessor
@inject TaskboardClient Client
@inject IJSRuntime JS

<MudThemeProvider @bind-IsDarkMode="_isDarkMode" />
<MudDialogProvider />
<MudSnackbarProvider />

<div class="flex h-screen w-full bg-bg text-text">
    <aside class="flex w-60 flex-col border-r border-border bg-surface">
        <div class="flex h-16 items-center px-5 border-b border-border">
            <span class="text-xl font-bold tracking-tight text-text">Taskboard</span>
        </div>
        <NavMenu />
    </aside>
    <div class="flex min-w-0 flex-1 flex-col">
        <header class="flex h-16 items-center justify-end gap-3 border-b border-border bg-surface px-5">
            <button type="button"
                    class="rounded-lg p-2 text-text-muted hover:bg-surface-elevated hover:text-text"
                    aria-label="toggle theme"
                    @onclick="ToggleTheme">
                @if (_isDarkMode)
                {
                    <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z"/></svg>
                }
                else
                {
                    <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364-.707-.707M6.343 6.343l-.707-.707m12.728 0-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"/></svg>
                }
            </button>
            <a href="/settings" class="rounded-lg p-2 text-text-muted hover:bg-surface-elevated hover:text-text" aria-label="settings">
                <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-.036.864-.036 1.29 0l.526 1.127 1.304.455c.41.143.785.348 1.12.607l1.034.793 1.287-.454c.43-.15.884.05 1.116.44l.605 1.04c.26.448.33.96.205 1.445l-.44 1.44.44 1.44c.125.485.055.997-.205 1.445l-.605 1.04c-.232.39-.686.59-1.116.44l-1.287-.454-1.034.793c-.335.26-.71.464-1.12.607l-1.304.455-.526 1.127c-.426.036-.864.036-1.29 0l-.526-1.127-1.304-.455c-.41-.143-.785-.348-1.12-.607l-1.034-.793-1.287.454c-.43.15-.884-.05-1.116-.44l-.605-1.04c-.26-.448-.33-.96-.205-1.445l.44-1.44-.44-1.44c-.125-.485-.055-.997.205-1.445l.605-1.04c.232-.39.686-.59 1.116-.44l1.287.454 1.034-.793c.335-.26.71-.464 1.12-.607l1.304-.455.526-1.127zM15 12a3 3 0 11-6 0 3 3 0 016 0z"/></svg>
            </a>
            @if (HttpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true)
            {
                <form method="post" action="/api/logout" class="m-0" aria-label="logout">
                    <button type="submit" class="text-sm font-medium text-text-muted hover:text-text">Log out</button>
                </form>
            }
        </header>
        <main class="flex-1 overflow-auto p-6">
            <ErrorBoundary>
                <ChildContent>
                    @Body
                </ChildContent>
                <ErrorContent>
                    <div class="rounded-lg border border-red-200 bg-red-50 p-4 text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-100">
                        An unexpected error occurred. Please refresh the page.
                    </div>
                </ErrorContent>
            </ErrorBoundary>
        </main>
    </div>
</div>

@code {
    private bool _isDarkMode = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var settings = await Client.GetSettingsAsync();
            _isDarkMode = settings.Theme != "light";
            await ApplyThemeAsync();
        }
        catch (HttpRequestException)
        {
            _isDarkMode = true;
        }
    }

    private async Task ToggleTheme()
    {
        _isDarkMode = !_isDarkMode;
        await ApplyThemeAsync();
    }

    private async Task ApplyThemeAsync()
    {
        var theme = _isDarkMode ? "dark" : "light";
        await JS.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", theme);
        await JS.InvokeVoidAsync("document.documentElement.classList.toggle", "dark", _isDarkMode);
    }
}
```

- [ ] **Step 2: Rewrite `NavMenu.razor` with Tailwind links**

```razor
<nav class="flex flex-col gap-1 p-4" role="navigation" aria-label="sidebar navigation">
    <NavLink class="nav-link" href="" Match="NavLinkMatch.All" aria-label="Board">
        <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V7M3 7l9-5 9 5"/></svg>
        Board
    </NavLink>
    <NavLink class="nav-link" href="ai-chat" aria-label="AI Chat">
        <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.77 9.77 0 01-3.5-.65L3 21l1.2-4.5A9.96 9.96 0 013 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"/></svg>
        AI Chat
    </NavLink>
    <NavLink class="nav-link" href="github-board" aria-label="GitHub Board">
        <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.5 6.5l-4 4M16 10.5l4 4M5 16a5 5 0 01-1-9.9 10.002 10.002 0 0019 3 5 5 0 01-4 10.9H5z"/></svg>
        GitHub Board
    </NavLink>
    <NavLink class="nav-link" href="settings" aria-label="Settings">
        <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-.036.864-.036 1.29 0l.526 1.127 1.304.455c.41.143.785.348 1.12.607l1.034.793 1.287-.454c.43-.15.884.05 1.116.44l.605 1.04c.26.448.33.96.205 1.445l-.44 1.44.44 1.44c.125.485.055.997-.205 1.445l-.605 1.04c-.232.39-.686.59-1.116.44l-1.287-.454-1.034.793c-.335.26-.71.464-1.12.607l-1.304.455-.526 1.127c-.426.036-.864.036-1.29 0l-.526-1.127-1.304-.455c-.41-.143-.785-.348-1.12-.607l-1.034-.793-1.287.454c-.43.15-.884-.05-1.116-.44l-.605-1.04c-.26-.448-.33-.96-.205-1.445l.44-1.44-.44-1.44c-.125-.485-.055-.997.205-1.445l.605-1.04c.232-.39.686-.59-1.116-.44l-1.287.454 1.034-.793c.335-.26.71-.464 1.12-.607l1.304-.455.526-1.127zM15 12a3 3 0 11-6 0 3 3 0 016 0z"/></svg>
        Settings
    </NavLink>
    <NavLink class="nav-link" href="skills" aria-label="Skills">
        <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"/></svg>
        Skills
    </NavLink>
</nav>
```

- [ ] **Step 3: Build and smoke test**

Run `dotnet build src/Taskboard.Server/Taskboard.Server.csproj -c Release`.

---

### Task 3: Refactor Login page

**Files:**
- Modify: `src/Taskboard.Blazor/Components/Pages/Login.razor`

- [ ] **Step 1: Replace MudBlazor with Tailwind form**

```razor
@page "/login"

<PageTitle>Login - Taskboard</PageTitle>

<div class="flex min-h-[80vh] items-center justify-center">
    <div class="card w-full max-w-sm">
        <div class="mb-6 text-center">
            <h1 class="text-2xl font-bold text-text">Taskboard</h1>
            <p class="mt-1 text-sm text-text-muted">Sign in to continue</p>
        </div>
        <form method="post" action="/api/login" class="space-y-4">
            <div>
                <label for="username" class="mb-1 block text-sm font-medium text-text">Username</label>
                <input id="username" type="text" name="Username" required autofocus class="input" placeholder="user" />
            </div>
            <div>
                <label for="password" class="mb-1 block text-sm font-medium text-text">Password</label>
                <input id="password" type="password" name="Password" required class="input" placeholder="••••••••" />
            </div>
            <input type="hidden" name="ReturnUrl" value="/" />
            <button type="submit" class="btn-primary w-full">Sign in</button>
        </form>
    </div>
</div>
```

- [ ] **Step 2: Build and run local visual check**

Run `dotnet build` and open `http://127.0.0.1:47823/login` after restart.

---

### Task 4: Refactor BoardView (Kanban) page

**Files:**
- Modify: `src/Taskboard.Blazor/Components/BoardView.razor`
- Modify: `src/Taskboard.Blazor/Components/TaskCard.razor` (as needed)

- [ ] **Step 1: Rewrite `BoardView.razor` Tailwind layout**

```razor
@page "/"
@inject TaskboardClient Client

<h1 class="mb-2 text-2xl font-bold text-text">Board</h1>
<p class="mb-6 text-sm text-text-muted">Drag or browse tasks by status</p>

@if (_isLoading)
{
    <Loading Message="Loading project..." />
}
else if (_errorMessage is not null)
{
    <div class="rounded-lg border border-red-200 bg-red-50 p-4 text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-100">
        @_errorMessage
    </div>
}
else if (_project is null)
{
    <EmptyState Message="No project found." />
}
else
{
    <div class="flex gap-4 overflow-x-auto pb-4">
        @foreach (var status in _statuses)
        {
            <div class="kanban-column">
                <div class="mb-3 flex items-center justify-between">
                    <h2 class="text-sm font-semibold capitalize text-text">@status</h2>
                    <span class="rounded-full bg-surface-elevated px-2 py-0.5 text-xs font-medium text-text-muted ring-1 ring-border">@GetTasks(status).Count()</span>
                </div>
                <div class="flex flex-col gap-2 overflow-y-auto" style="max-height: 70vh;">
                    @if (GetTasks(status).Any())
                    {
                        <Virtualize Items="GetTasks(status).ToList()" ItemSize="80" OverscanCount="5">
                            <TaskCard Task="context" @key="context.Id" />
                        </Virtualize>
                    }
                    else
                    {
                        <EmptyState Message="No tasks" />
                    }
                </div>
            </div>
        }
    </div>
}

@code {
    // existing code remains unchanged
}
```

- [ ] **Step 2: Build and verify table/cards render**

Run `dotnet build` and check local board.

---

### Task 5: Refactor Settings and Skills pages

**Files:**
- Modify: `src/Taskboard.Blazor/Components/Pages/Settings.razor`
- Modify: `src/Taskboard.Blazor/Components/Pages/Skills.razor`

- [ ] **Step 1: Rewrite `Settings.razor` with Tailwind**

```razor
@page "/settings"
@attribute [Authorize]
@inject TaskboardClient Client
@inject NavigationManager Navigation
@inject ISnackbar Snackbar

<PageTitle>Settings - Taskboard</PageTitle>

<h1 class="mb-2 text-2xl font-bold text-text">Settings</h1>
<p class="mb-6 text-sm text-text-muted">Customize your workspace and agents</p>

<div class="card mb-6">
    <h2 class="mb-4 text-lg font-semibold text-text">Preferences</h2>
    <div class="space-y-4">
        <label class="flex items-center gap-3">
            <input type="checkbox" class="h-4 w-4 rounded border-border text-primary-600" @bind="_isDark" @bind:after="() => { }" />
            <span class="text-sm text-text">Dark mode</span>
        </label>
        <div>
            <label for="github-token" class="mb-1 block text-sm font-medium text-text">GitHub token</label>
            <input id="github-token" type="password" class="input" @bind="_gitHubToken" />
        </div>
    </div>
</div>

<div class="card mb-6">
    <h2 class="mb-4 text-lg font-semibold text-text">Agents</h2>
    @if (_loading)
    {
        <Loading Message="Loading agents..." />
    }
    else if (!_agents.Any())
    {
        <EmptyState Message="No agents discovered." />
    }
    else
    {
        <div class="overflow-x-auto">
            <table class="w-full text-left text-sm">
                <thead class="border-b border-border text-text-muted">
                    <tr>
                        <th class="pb-2 font-medium">Name</th>
                        <th class="pb-2 font-medium">Version</th>
                        <th class="pb-2 font-medium">Enabled</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-border">
                    @foreach (var agent in _agents)
                    {
                        <tr>
                            <td class="py-3 text-text">@agent.Name</td>
                            <td class="py-3 text-text-muted">@agent.Version</td>
                            <td class="py-3">
                                <input type="checkbox" class="h-4 w-4 rounded border-border text-primary-600" checked="@_enabled.Contains(agent.Type)" @onchange="e => ToggleAgent(agent.Type, (bool)e.Value!)" />
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
</div>

<button class="btn-primary" @onclick="SaveAsync">Save settings</button>

@code {
    // existing code remains unchanged, but SaveAsync should call ApplyTheme if needed
}
```

- [ ] **Step 2: Rewrite `Skills.razor` with Tailwind table**

```razor
@page "/skills"
@attribute [Authorize]
@inject TaskboardClient Client

<PageTitle>Skills - Taskboard</PageTitle>

<h1 class="mb-2 text-2xl font-bold text-text">Skills</h1>
<p class="mb-6 text-sm text-text-muted">Installed agent skills</p>

<div class="card">
    <div class="mb-4 flex items-center gap-3">
        <input type="search" class="input max-w-md" placeholder="Search skills..." @bind="_search" @bind:event="oninput" />
    </div>
    @if (_loading)
    {
        <Loading Message="Loading skills..." />
    }
    else if (!_filteredSkills.Any())
    {
        <EmptyState Message="No skills found." />
    }
    else
    {
        <div class="overflow-x-auto">
            <table class="w-full text-left text-sm">
                <thead class="border-b border-border text-text-muted">
                    <tr>
                        <th class="pb-2 font-medium">Name</th>
                        <th class="pb-2 font-medium">Source</th>
                        <th class="pb-2 font-medium">Description</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-border">
                    @foreach (var skill in _filteredSkills)
                    {
                        <tr>
                            <td class="py-3 font-medium text-text">@skill.Name</td>
                            <td class="py-3 text-text-muted">@skill.Source</td>
                            <td class="py-3 text-text-muted">@skill.Description</td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
</div>

@code {
    // existing code, plus a _filteredSkills computed property using Filter
}
```

- [ ] **Step 3: Build and run full UI smoke test**

Run `dotnet build` and navigate through `login`, `board`, `settings`, `skills`.

---

### Task 6: Add runtime theme persistence to settings save

**Files:**
- Modify: `src/Taskboard.Blazor/Components/Pages/Settings.razor`
- Modify: `src/Taskboard.Blazor/Layout/MainLayout.razor`

- [ ] **Step 1: Ensure theme is saved through the settings API**

The `SaveAsync` method already calls `Client.SaveSettingsAsync` with `_isDark ? "dark" : "light"`. The `MainLayout` loads this value on init. No backend change is required.

- [ ] **Step 2: After saving, apply theme immediately to document**

Add `IJSRuntime` injection to `Settings.razor` and call the same JS helpers as `MainLayout` after a successful save, so the user sees the new theme before the redirect.

---

### Task 7: Final verification and commit

- [ ] **Step 1: Run `dotnet build` in Release**

```bash
dotnet build src/Taskboard.Server/Taskboard.Server.csproj -c Release
```

Expected: 0 warnings, 0 errors.

- [ ] **Step 2: Restart `taskboard-server` and verify public URL**

```bash
systemctl --user restart taskboard-server
sleep 5
curl -sL --max-time 15 https://task.afonsoft.dev/login | grep -o '<title>[^<]*</title>'
```

Expected: `<title>Login - Taskboard</title>`

- [ ] **Step 3: Commit the changes**

```bash
git add -A
git commit -m "feat: redesign UI with Tailwind CSS and persistent light/dark theme"
git push
```

---

## Verification Summary

| Check | Command / URL | Expected |
|-------|---------------|----------|
| Build | `dotnet build src/Taskboard.Server/Taskboard.Server.csproj -c Release` | 0 errors |
| Login | `https://task.afonsoft.dev/login` | Tailwind-styled login form |
| Board | `https://task.afonsoft.dev/` | Tailwind kanban columns |
| Settings | `https://task.afonsoft.dev/settings` | Tailwind form and agents table |
| Skills | `https://task.afonsoft.dev/skills` | Tailwind table with search |
| Theme | Click sun/moon icon | Toggle between light and dark instantly, persisted on save |
