# Product Launch Brief — DocPilot v1.0

**Prepared for:** Executive Leadership Team
**Author:** Product Marketing
**Date:** April 2026
**Status:** Approved for Launch

---

## Executive Summary

DocPilot is a native Windows desktop application that brings Claude-powered,
streaming document chat to professional users who need to analyse PDFs,
Word documents, and long-form text without leaving their desktop. Unlike
browser-based chat tools, DocPilot is DPI-aware, offline-capable in Demo
Mode, and stores credentials under Windows DPAPI — a fit for regulated
industries that cannot paste sensitive documents into web chat.

We are launching DocPilot as a **free, open-source reference application**
on GitHub to establish technical credibility in the emerging "AI desktop"
category, with a paid Pro tier to follow in Q3.

---

## Target Customers

1. **Knowledge workers in regulated industries** — legal, finance, healthcare.
   Policies often forbid pasting confidential text into browser chat
   tools; a local desktop app with encrypted credentials is acceptable.
2. **Researchers and students** — anyone who spends their day reading
   long PDFs and wants AI-assisted summarisation, translation, and Q&A.
3. **Developers** — the repo serves as a portfolio of modern WPF + MVVM
   practice, driving GitHub stars and downstream hires.

---

## Key Differentiators

- ⚡ **Streaming replies** — tokens appear in real time, the way Claude.ai
  streams in the browser. Internally this is driven by an
  `IAsyncEnumerable<string>` over Server-Sent Events.
- 🔐 **DPAPI-encrypted API key** — the Claude key is protected with
  per-user Windows DPAPI and never appears in clear text on disk.
- 🧪 **Built-in Demo Mode** — reviewers without an API key can still see
  the full streaming experience, lowering the barrier to first impression.
- 🌗 **Instant theme switching** — Dark ↔ Light at runtime, no reload.
- 📄 **Drop-in parsing** for PDF (PdfPig), DOCX (OpenXML SDK), and TXT.

---

## Success Metrics (First 90 Days)

| Metric | Goal |
|---|---|
| GitHub stars | 500 |
| Weekly active users | 2,000 |
| Median session length | ≥ 3 minutes |
| Setup-to-first-reply time | < 60 seconds |
| Demo Mode → live API conversion | ≥ 15 % |

---

## Risks and Mitigations

- **Anthropic pricing changes** — the `IAIService` abstraction already
  supports provider swaps; OpenAI / Gemini can be wired in within a day.
- **Scanned PDF support** — our v1 text extractor cannot OCR image-only
  PDFs; v2 will integrate Claude's vision endpoints for this use case.
- **Regulatory acceptance** — DPAPI + local-only processing is the
  foundation, but enterprise procurement may still require on-prem LLM
  support. Tracked for v2.

---

## Next Steps

1. Ship v1.0 to GitHub this week with full README and demo GIF.
2. Announce on Hacker News, r/dotnet, and relevant Discords.
3. Open a public roadmap issue for v1.1 feature requests.

*This document is intentionally fictional — it ships as a realistic sample
input for DocPilot so reviewers can try the app against business-style
content without signing up for the Claude API.*
