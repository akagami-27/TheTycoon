# The Tycoon

A single-player 2D business management strategy simulation game built with **Unity 6**, **Universal Render Pipeline (URP 2D)**, and **C#**.

Assume the role of a corporate founder and CEO. Navigate board politics, manage company finances, build executive relationships, and resolve critical event-driven dilemmas to expand your enterprise into a global empire.

---

## Project Specifications

- **Engine:** Unity 6 (`6000.6.10f1`)
- **Render Pipeline:** Universal Render Pipeline (URP) – 2D Renderer
- **Target Platform:** Windows Standalone (x64)
- **Primary Namespace:** `TheTycoon`
- **Architecture:** Feature-based modular architecture using Unity Assembly Definitions (`asmdef`)

---

## Documentation

Detailed architectural and design documentation is maintained in the [`Docs/`](Docs/) directory:

- [Project Brief](Docs/PROJECT_BRIEF.md): High-level overview, inspirations, and core gameplay loop.
- [Architecture Guide](Docs/ARCHITECTURE.md): Assembly boundaries, separation of data and state, and dependency guidelines.
- [Code Conventions](Docs/CODE_CONVENTIONS.md): C# naming standards, styling, and Unity lifecycle practices.
- [System Design](Docs/SYSTEM_DESIGN.md): Detailed specifications for Events, Financial calculations, Calendar turns, and Saves.
- [Development Roadmap](Docs/DEVELOPMENT_ROADMAP.md): Milestones, feature priorities, and implementation dependencies.
- [Setup Guide](Docs/SETUP_GUIDE.md): Developer environment configuration, VS Code settings, and running tests.

---

## Project Structure

```
Assets/
├── _Project/
│   ├── Data/                  # ScriptableObjects (Events, Characters, Balance)
│   ├── Prefabs/               # UI components and simulation widgets
│   ├── Scenes/                # Bootstrap, MainMenu, Game scenes
│   ├── Scripts/
│   │   ├── Core/              # TheTycoon.Core: Values, interfaces, primitives
│   │   ├── Features/          # TheTycoon.Features: Finance, Events, Company, Turns
│   │   ├── Infrastructure/    # TheTycoon.Infrastructure: Saves, Logging
│   │   └── UI/                # TheTycoon.UI: Dashboards, Event modals
│   └── Tests/
│       ├── EditMode/          # Fast pure unit tests
│       └── PlayMode/          # Integration and lifecycle tests
├── Scenes/                    # Template / fallback scenes
└── Settings/                  # URP 2D renderer and input settings
```

---

## License

All rights reserved. Proprietary project.
