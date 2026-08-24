# Spec: Workflow Engine & Automation (module `workflow-automation`)

Descreve o motor de grafo de workflow (`shared/workflow-control-flow.mjs`,
`shared/workflow-sequence.mjs`) e a automação de auto-claim
(`shared/taskboard-automation*.mjs`). Parte do domínio `workflow_workspaces`
(`SPEC-001`) e dos endpoints `/api/projects/:id/workflow-workspace`,
`/api/workflow-capabilities`.

## Workflow control-flow (grafo visual)
- **Node types**: inclui `condition` (nós de decisão).
- **Contrato de dados**:
  - nodes: `{id, parentId?, position:{x,y}, data:{kind,...}}`
  - edges: `{id, source, target, sourceHandle?, data?:{conditionId, conditionOutcome}}`
- **Funções principais**:
  - `orderedWorkflowStepIds(nodes,edges)` — ordenação topológica (Kahn por
    in-degree; desempate por position y/x/id).
  - `insertWorkflowStep`, `reorderWorkflowStep(stepIds,stepId,targetIndex,pinnedStepId)`
  - `workflowSequenceEdges(stepIds)` — arestas em cadeia linear.
  - `workflowConditionEdges(trunkStepIds,conditionId,branches{true,false})`
    — arestas de branch com `conditionOutcome`.
  - `normalizeWorkflowConditionBranches(nodes,edges)` — migra linear legado →
    `{trunkStepIds, conditionId, branches, migrated}`.
  - `layoutWorkflowSteps(nodes,stepIds,heights,{top,gap})` — layout vertical
    com agrupamento de parent.
- `workflow-sequence.mjs` re-exporta/duplica a mesma lógica de sequenciamento.
- Persistido em `workflow_workspaces.workspace` (JSON) com `version` próprio
  (conflito 409 igual a tasks).

## Automação de auto-claim (`taskboard-automation*.mjs`)
- `AUTOMATION_MODELS` (em `options.mjs`): gpt-5.6-sol/terra/luna, gpt-5.5,
  gpt-5.4, gpt-5.4-mini — cada um com `label, slug, defaultEffort, efforts[]`.
  Helpers: `getAutomationModel`, `isAutomationModel`, `isAutomationReasoningEffort`,
  `isSupportedModelEffort`, `withAutomationModel`.
- `parseTaskboardAutomationHostRequest(value)` — valida strict request RPC do host:
  `id, requestId, operation(ensure-active|pause|list|apply-policy),
  taskboardProjectId, codexProject(Id/Kind/HostId), workspacePath, skillPath,
  automationId, enabledByUser, quotaAware, intervalMinutes(∈{5,10,15,30,60}),
  model, reasoningEffort, remoteProjects[]`.
- `buildTaskboardAutomationName` / `buildTaskboardAutomationPrompt` (instrução
  em chinês p/ claim/dispatch de `todo` para sessões Codex SSH remotas com
  handoff de `threadBinding`) / `buildTaskboardAutomationSpec`
  (`{kind:'cron', name, prompt, projectId, executionEnvironment:'local', model,
  reasoningEffort, rrule:'RRULE:FREQ=MINUTELY;INTERVAL=N'}`).
- `taskboardAutomationPolicyOperation` (pause/list/ensure-active com quota).
- `reconcileTaskboardAutomation(request,rpc)` → via `list-automations`,
  `automation-create`, `automation-update` RPCs; `sanitizeAutomation`.

## Endpoints (ver `SPEC-002`)
- `GET/PUT /api/projects/:id/workflow-workspace` (PUT `{version,workspace}`)
- `GET /api/workflow-capabilities?workspacePath=<abs>`

## .NET mapping (`Taskboard.Workflow`)
- `WorkflowGraph` (C#): mesmas structs `Node`/`Edge`; implementar
  `OrderedStepIds` (Kahn), `InsertStep`, `ReorderStep`, `SequenceEdges`,
  `ConditionEdges`, `NormalizeBranches`, `LayoutSteps`. Paridade de ordenação/
  desempate é crítica p/ render idêntico.
- `WorkflowWorkspaceService`: CRUD do JSON `workspace` + `version` lock 409.
- `Automation` (C#): modelos/validação de `AUTOMATION_MODELS`; builder de prompt
  e spec cron; reconciliação via RPC de automação Codex (manter contrato).
- Persistir em `workflow_workspaces` (EF Core) ou JSON raw.
