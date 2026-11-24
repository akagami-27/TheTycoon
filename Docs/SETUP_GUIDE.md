# Setup Guide – The Tycoon

## 1. System Requirements

- **Operating System:** Windows 10 / Windows 11 (64-bit)
- **Unity Version:** Unity 6 (`6000.6.10f1` with revision `40e81493c67d`)
- **Render Pipeline:** Universal Render Pipeline (URP 2D)
- **IDE:** Visual Studio Code (recommended) or JetBrains Rider / Visual Studio 2022

---

## 2. Setting Up Visual Studio Code

1. Install the following VS Code extensions:
   - **C# Dev Kit** (`ms-dotnettools.csdevkit`)
   - **C#** (`ms-dotnettools.csharp`)
   - **Unity Extension for Visual Studio Code** (`visualstudiotools.unity`)
2. The repository already includes configured `.vscode/settings.json`, `.vscode/launch.json`, and `.vscode/extensions.json`.
3. Open the repository root folder in VS Code.
4. VS Code will automatically detect `TheTycoon.slnx` and provide full IntelliSense, code navigation, and debugging capabilities.

---

## 3. Opening the Project in Unity

1. Open **Unity Hub**.
2. Click **Add** -> **Add project from disk**.
3. Select the `c:\Users\Acer\TheTycoon` directory.
4. Ensure the Editor Version is set to **6000.6.10f1**.
5. Click the project to open. Unity will initialize the Package Cache and Library files.

---

## 4. Opening and Playing the Game Dashboard Scene

1. In the Unity Project window, navigate to:
   `Assets/_Project/Scenes/Game/Game.unity`
2. Double-click **Game.unity** to open the scene.
3. Click the **Play** button at the top of the Unity Editor.
4. The dashboard will initialize automatically via `SimulationBootstrapper`:
   - Company Header displays "Nexus Corporation", "January 2026 (Q1)", and "Reputation: 50%".
   - Corporate Treasury displays starting cash of `RM250,000.00`.
   - Previous Month Performance displays placeholders ("Not yet available").
5. Click the **ADVANCE MONTH** button:
   - January accounts settle with `RM45,000.00` revenue and `RM30,000.00` expenses (`+RM15,000.00` net profit).
   - Date increments to "February 2026 (Q1)".
   - Cash updates to `RM265,000.00`.
   - A modal dialog appears displaying the completed January 2026 Financial Report.
6. Click **DISMISS** to close the report modal.

---

## 5. Running Validation & Tests

### Via Unity Editor
1. In the Unity Editor, open **Window** -> **General** -> **Test Runner**.
2. Select **EditMode** tab.
3. Click **Run All** to execute all 71 unit tests for `TheTycoon.Core`, `TheTycoon.Features`, and `TheTycoon.UI`.

### Via Command Line (Batchmode)
Run the following PowerShell command from the repository root:

```powershell
Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.6.10f1\Editor\Unity.exe" `
    -ArgumentList "-batchmode -projectPath `"$PWD`" -runTests -testPlatform editmode -testResults editmode-results.xml -logFile unity_test.log" `
    -Wait -NoNewWindow
```
Inspect `editmode-results.xml` for test execution results.
