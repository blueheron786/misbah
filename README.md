
# Misbah

![.NET Core CI](https://github.com/blueheron786/misbah/actions/workflows/dotnet-tests.yml/badge.svg?branch=main)

An open-source note-taking app focused on storing and linking ideas.

## What is Misbah?

It's like Obsidian, but better? Built in C#, and FOSS.

## Getting Started

To get started, create a "hub," a directory that will contain folders and files for your notes.

If you are looking to switch over from Obsidian, you can open your Obsidian vault and start working with it directly. All notes and folders will exist in the same structure, albeit plugins and custom code won't execute.

## Markdown

Misbah supports Markdown, which means it supports arbitrary HTML. Use this to your advantage.

Custom Markdown syntax includes:

- `==x==`: highlights `x` (the same as `<mark>x</mark>`)
- `- [ ]` creates a checkbox in a list. `- [x]` checks it off. You can also click to toggle it.
- `[[Page|Display]]` links to `[[Page]]` but appears as `Display`

# Architecture

Misbah tries to follow Clean Architecture. See `.github/copilot-instructions.md` for a detailed look at the current architecture and design.