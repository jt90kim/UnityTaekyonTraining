# Message Protocol

This document defines all allowed messages exchanged between
Android (host) and Unity (training engine).

Messages are explicit commands.
There is no shared state.

---

## Android → Unity Messages

### LOAD_MOTION:<path>

- Sent before a session starts
- Instructs Unity to load motion data from the given file path
- Does not start playback

---

### START_SESSION

- Signals the beginning of a training session
- Unity begins executing motion and timing logic

---

### STOP_SESSION

- Signals an early or normal termination of a session
- Unity stops execution and prepares to unload

---

## Unity → Android Messages (Future)

### UNITY_READY

- Unity runtime initialized
- Ready to receive commands

---

### SESSION_COMPLETE

- Training session completed normally

---

## Protocol Rules

- Messages are one-directional commands
- Messages are processed sequentially
- Messages do not imply state transitions unless documented
- Unknown messages must be ignored or logged as errors

Any message not listed here is invalid.
