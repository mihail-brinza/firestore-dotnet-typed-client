# README Redesign

## Problem

The current README jumps straight into side-by-side code comparisons for every operation without first explaining what problem the library solves. Someone scanning the page has to read a lot of code before understanding why they should care.

## Design

Restructure the README into these sections:

### 1. Title + badges + tagline
Unchanged.

### 2. The Problem (NEW)
Two before/after snippets:
- A typo in a field name that compiles with the official client but is caught at compile time with the typed client
- A custom field name (`home_country` vs `Country`) that the typed client resolves automatically

### 3. Why use the Typed Client? (NEW)
Feature comparison table:

| | Official Client | Typed Client |
|---|---|---|
| Field references | Magic strings | Lambda expressions |
| Custom field names | Manual lookup | Automatic |
| Wrong field name | Silent runtime failure | Compile error |
| Type mismatch on update | Runtime exception | Compile error |
| API compatibility | - | Full (wraps official client) |

Plus one sentence noting full compatibility with the official client (transactions, listeners, batches).

### 4. Installation + Compatibility
Unchanged.

### 5. Quick Start
Renamed from "Sample Code". Content unchanged.

### 6. API (condensed)
Side-by-side typed vs official for:
- Updating specific fields (type safety + custom field name resolution most visible)
- Querying (lambda vs string field references)

Typed-only (no side-by-side) for:
- Access Collection
- Creating a document
- Reading documents
- Replace with merge
- Deleting documents

Each typed-only section: code block + 1-2 sentences.

### 7. Benchmarks
Unchanged.

## Goals
- Front-load the value proposition for both audiences (existing Firestore users and new evaluators)
- Cut repetitive side-by-side blocks where the contrast is minimal
- Keep high-contrast comparisons where they matter most (queries and updates)
- Make the README scannable with the feature table
