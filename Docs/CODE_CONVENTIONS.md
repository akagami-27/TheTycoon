# Code Conventions – The Tycoon

## 1. Naming Standards

| Symbol / Target | Convention | Example |
| :--- | :--- | :--- |
| Classes, Structs, Enums | PascalCase | `FinancialLedger`, `Money`, `Month` |
| Interfaces | IPascalCase | `ITurnListener`, `ISaveSystem`, `ICondition` |
| Public Methods & Properties | PascalCase | `AdvanceMonth()`, `CurrentBalance` |
| Method Arguments | camelCase | `targetCompany`, `deltaAmount` |
| Local Variables | camelCase | `totalExpenses`, `isValid` |
| Private / Protected Fields | _camelCase | `_currentBalance`, `_turnManager` |
| Serialized Private Fields | _camelCase | `[SerializeField] private TMP_Text _balanceLabel;` |
| ScriptableObject Assets | Suffix `SO` | `EventDefinitionSO`, `CompanyTemplateSO` |
| Event Actions / Delegates | Past Tense / Imperative | `OnTurnCompleted`, `OnDecisionEnacted` |

---

## 2. Formatting & Code Hygiene

- **Indentation:** 4 spaces (no tabs).
- **Braces:** All statements must use explicit braces on new lines:
  ```csharp
  // Good
  if (isBankrupt)
  {
      TriggerGameOver();
  }
  
  // Forbidden
  if (isBankrupt) TriggerGameOver();
  ```
- **Namespaces:** Match directory structure underneath `TheTycoon`:
  - `TheTycoon.Core`
  - `TheTycoon.Features.Finance`
  - `TheTycoon.Features.Events`
  - `TheTycoon.Infrastructure.SaveSystem`
  - `TheTycoon.UI.Dashboard`

---

## 3. Unity & Simulation Best Practices

1. **Keep Update() Clean:** Simulation occurs on discrete monthly turns. Avoid polling simulation states inside MonoBehaviour `Update()` loops. Rely on event callbacks and state change notifications.
2. **Prevent Garbage Allocations:**
   - Avoid creating temporary arrays or LINQ queries in per-frame or high-frequency loops.
   - Use structs for lightweight, immutable value objects (`Money`, `GameDate`).
3. **No Hidden Singletons:**
   - Prefer passing dependencies explicitly via constructors or `Initialize()` methods.
   - Scene root systems should bind dependencies into a `SimulationContext`.
4. **Decouple UI from Simulation:**
   - Views should only read formatted view-models or observe state changes.
   - UI views must never mutate game state directly; all actions dispatch through feature services.

---

## 4. Error Handling & Validation

- Use explicit preconditions (`ArgumentNullException.ThrowIfNull()`, custom assertions).
- Log warnings and errors via structured logging or `UnityEngine.Debug` with class-identifying context.
- Handle data recovery gracefully in the save/load pipeline by falling back to safe defaults if fields are missing in legacy save files.
