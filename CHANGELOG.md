# Changelog

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added
- `TableOptions.MaxColumnWidth` — cap cell width and truncate longer cells with an
  ellipsis, so a stray long value can't blow out the whole table.

## [0.1.0]

### Added
- `TableRenderer` rendering rows as aligned ASCII or GitHub-Markdown tables.
- Per-column alignment, optional header, deterministic `\n` line endings.
- RFC 4180 CSV/TSV parsing and a CLI front end.
