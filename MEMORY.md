# Project Memory

## Product Priorities

- Content is the core product. Prioritize high-quality study content: topics, linked videos/resources, questions, exercises, and note-taking flows over infrastructure polish.
- Do not create tests for incomplete features. Finish a usable feature first, then add tests that protect the completed behavior.
- Avoid asking the user to manually type or generate study questions. The platform should ship with rich, curated, high-quality questions and linked learning resources.
- The app should minimize context switching. Prefer in-app reading, embedded videos, in-app questions, notes, and coding over page references that require leaving the platform.
- For user-owned local PDFs, support local-only ingestion/display inside the app when possible. Keep original PDFs and extracted content out of GitHub.
- For video curation, prefer the user's trusted author/channel pool over generic course aggregation. Include respected channels such as Nick Chapsas, Milan Jovanovic, Patrick God, Scott Hanselman, Dev Leader, Basarat Ali Syed, dotnet, Microsoft Visual Studio, IAmTimCorey, Microsoft Developer, NeetCode, MIT OpenCourseWare, Web Dev Simplified, Raw Coding, Code Maze, Ardalis, Jack Herrington, and similar high-signal sources.

## Frontend Structure

- Do not let `App.tsx` become a god file. Keep React code split by feature folders and shared components; `App.tsx` should compose workflows and own only top-level app state.
