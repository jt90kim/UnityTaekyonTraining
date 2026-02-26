# System Architecture — Taekkyeon Hybrid Training App

## Purpose
This document defines the high-level system architecture and authority
boundaries for the Taekkyeon Hybrid Android + Unity training application.

This is a serious martial arts training tool.
It is not a game.

## High-Level Overview

The system consists of two runtime components:

- Android Application (Kotlin)
- Unity Training Engine (C#), embedded as a Library

Android is the controlling host.
Unity is a reactive execution engine.

---

## Android Responsibilities

Android is responsible for:

- Application lifecycle
- Navigation and UI
- Drill selection
- Session start and stop
- Loading Unity only during training sessions

Android MUST NOT:

- Interpret motion data
- Modify motion data
- Control opponent movement logic

---

## Unity Responsibilities

Unity is responsible for:

- Opponent representation
- Motion playback
- Timing and pressure logic
- Executing drills as instructed

Unity MUST NOT:

- Select drills
- Generate motion
- Persist user data
- Control application navigation

---

## Integration Model

- Unity is embedded as a Library inside the Android application
- Unity runs only during active training sessions
- All communication is message-based
- Unity reacts to Android commands only

---

## Architectural Principles

- Motion is data-driven and external
- AI selects timing and technique only
- Motion clarity is prioritized over graphics
- Determinism is preferred over randomness

This architecture is non-negotiable.
