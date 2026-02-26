# Session Lifecycle

This document defines the runtime lifecycle of a training session.

Android is the authority.
Unity reacts to state transitions.

---

## Session States

IDLE
- No training session active
- Unity is not loaded

PREPARE_SESSION
- Drill selected
- Motion data resolved
- Unity is about to be launched

UNITY_LOADED
- Unity runtime initialized
- Waiting for explicit start command

SESSION_RUNNING
- Training session active
- Unity executing motion and timing logic

SESSION_END
- Training session completed or aborted
- Unity preparing to unload

RETURN_TO_ANDROID
- Control returned to Android UI
- Unity no longer active

---

## Authority Rules

- Android owns all state transitions
- Unity must not initiate state changes
- Unity reacts to START and STOP commands only

Any deviation is a bug.
