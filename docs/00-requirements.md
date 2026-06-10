# Requirements

## Original (Polish)

> Zgodnie z naszą rozmową przesyłam zadanie rekrutacyjne.
>
> Przygotuj prosty system składający się z backendu oraz frontendowego UI, który umożliwia użytkownikowi wysyłanie wielu promptów do przetworzenia oraz śledzenie ich statusu.
>
> Backend powinien być napisany w C# i udostępniać API do dodawania promptów oraz pobierania ich aktualnych stanów. Każdy prompt powinien zostać zapisany w bazie danych.
>
> Oddzielny proces przetwarzający powinien obsługiwać zadania i wykonywać je przy użyciu jednej z dostępnych bibliotek do komunikacji z modelami językowymi, np. z lokalnym modelem lub usługą zewnętrzną. Każde zadanie musi przechodzić przez stany: oczekujące, przetwarzane, zakończone lub nieudane.
>
> Frontend w React lub Next.js ma umożliwiać dodanie wielu promptów oraz wyświetlać listę wszystkich z aktualnymi statusami i wynikami. Odświeżanie może odbywać się za pomocą prostego pollingu.
>
> Mile widziana jest orkiestracja projektu, tak aby cały system dało się łatwo uruchomić jednym poleceniem, oraz dołączenie krótkiej instrukcji, mini dokumentacji, wyjaśniającej jak uruchomić środowisko i poszczególne komponenty.

## English summary

Build a simple full-stack system for submitting multiple LLM prompts and tracking their status:

- **Backend (C#):** API to add prompts and fetch current states; persist every prompt in a database.
- **Worker (separate process):** Process jobs via an LLM library (local or external). States: Pending → Processing → Completed or Failed.
- **Frontend (React/Next.js):** Submit prompts, list all with statuses and results. Polling is acceptable.
- **Orchestration:** One command to run the whole system; short run documentation.

## Tracker

| Requirement | Phase | Status |
|-------------|-------|--------|
| C# API — add prompts, get statuses | 04 | done |
| Persist prompts in DB | 04 | done |
| Separate LLM processor process | 04 | done |
| States: Pending / Processing / Completed / Failed | 04 | done |
| Next.js UI | 05 | deferred |
| Polling or real-time refresh | 04 (SignalR) | done |
| One-command orchestration | 06 | done |
| Run instructions | README + 06 | done |
| Real LLM provider (OpenAI / Ollama / Mock) | 04 | done |

## Out of scope (for now)

- GitHub access for reviewers
- Frontend implementation
