# Agent Guidelines – The Tycoon

This document outlines conventions and operational procedures for AI agents assisting in the development of **The Tycoon**.

---

## Core Tenets

1. **Precision in Simulation Logic:**
   - Always use the immutable `Money` struct for currency and ledger entries. Never introduce `float` or `double` for money or share counts.
   - Time advances solely via `GameDate` and the turn-based monthly loop.
2. **Assembly Definition Integrity:**
   - Adhere strictly to the assembly boundaries (`TheTycoon.Core`, `TheTycoon.Features`, `TheTycoon.Infrastructure`, `TheTycoon.UI`).
   - Do not add UI references or MonoBehaviour dependencies into `TheTycoon.Core`.
3. **Data vs. State Separation:**
   - Define static configurations using `ScriptableObject` assets suffixed with `SO`.
   - Keep runtime simulation data in pure C# classes/structs capable of clean JSON serialization.
4. **Verifiable Quality:**
   - Every new feature in `Core` or `Features` must be accompanied by corresponding unit tests in `TheTycoon.Tests.EditMode`.
   - Ensure all code conforms to `.editorconfig` rules.
5. **No Speculative Bloat:**
   - Do not introduce unrequested third-party packages, networking libraries, or multi-threaded ECS systems for single-player 2D simulation logic.
