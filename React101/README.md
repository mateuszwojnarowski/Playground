# React 101 – From TypeScript to React

A hands-on learning path that takes you from TypeScript fundamentals to building a real-world React application. Each section contains explanations, working examples, and exercises to practice.

## Prerequisites

- [Node.js](https://nodejs.org/) v18+ and npm
- A code editor (VS Code recommended)
- Basic JavaScript knowledge

## Learning Path

| Section | Topic | Description |
|---------|-------|-------------|
| [01 – TypeScript Fundamentals](./01-typescript-fundamentals/) | TypeScript | Types, interfaces, generics, utility types, type guards |
| [02 – React Fundamentals](./02-react-fundamentals/) | React + TypeScript | Components, props, state, effects, event handling, forms |
| [03 – Weather Dashboard](./03-weather-dashboard/) | Real-World App | Complete React app connecting to the Open-Meteo weather API |

## How to Use This Project

Each section is a standalone project with its own `package.json`. To get started:

```bash
cd React101/<section-folder>
npm install
```

### TypeScript section

```bash
# Run examples
npx tsx src/examples/<file>.ts

# Run exercises tests (they will fail until you complete the exercises)
npm test
```

### React sections

```bash
# Start the dev server
npm run dev

# Run tests
npm test
```

## Structure

Each section follows the same pattern:

- **README.md** – Concepts to learn, with explanations and code snippets
- **src/examples/** – Working, runnable examples that demonstrate each concept
- **src/exercises/** – Exercises with `TODO` comments for you to complete
- **Tests** – Automated tests that validate your exercise solutions

## Approach

This project mimics a real-world development experience:

1. **Start with TypeScript** – You can't write good React without understanding TypeScript. Section 01 builds a solid foundation.
2. **Learn React fundamentals** – Section 02 introduces React concepts one at a time, each building on the previous.
3. **Build something real** – Section 03 is a complete weather dashboard that connects to a live API, handles loading/error states, and uses everything you've learned.

Happy coding! 🚀
