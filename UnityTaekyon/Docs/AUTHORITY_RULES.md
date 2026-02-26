# Authority Rules

This document defines explicit authority boundaries.
Any violation is considered a bug.

---

## Decision Authority

### Android Decides

- When a session starts
- When a session ends
- Which drill is active
- Which motion files are loaded

### Unity Decides

- When a technique is executed
- Timing and hesitation within a drill
- Pressure behavior during a session

---

## Forbidden Actions

Unity MUST NOT:

- Select or change drills
- Generate or alter motion data
- Access Android UI or navigation logic

Android MUST NOT:

- Interpret skeletal motion data
- Control joint positions
- Contain training or AI logic

---

## Communication Rules

- All cross-boundary communication is explicit
- No implicit state sharing
- No shared mutable state
- Messages are treated as commands, not suggestions

---

## Enforcement

If implementation convenience conflicts with these rules,
the rules take priority.

Refactor the implementation, not the architecture.
