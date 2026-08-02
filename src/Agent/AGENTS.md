# Agent Principles for src/Agent

## Quality Gates

You MUST pass all quality gates before you mark any work as complete in the `src/Agent` project.
Run the following checks and verify that all checks pass without errors:

1. **Type Checking:** Run `npm run typecheck` to verify that TypeScript compilation succeeds.
2. **Linting:** Run `npm run lint` to verify that ESLint reports no errors or warnings.
3. **Unit Tests:** Run `npm run test` to verify that all unit tests pass.
4. **Build:** Run `npm run build` to verify that the project bundles successfully.

You MUST NOT commit any code that fails these checks unless the user explicitly instructs you to pause testing.

## TypeScript Best Practices

### Type Safety

You MUST NOT use `any` type.
Always declare explicit types.
Create a proper interface or type when the type is unknown.

You MUST enable strict mode.
Set `"strict": true`, `"noImplicitAny": true`, `"strictNullChecks": true`, `"noImplicitReturns": true`, `"forceConsistentCasingInFileNames": true`, and `"noUnusedLocals": true` in `tsconfig.json`.

You SHOULD prefer union and literal types over plain strings.
Use `'value1' | 'value2'` instead of `string` when a variable holds only certain values.

You SHOULD use utility types.
Use `Partial<T>`, `Readonly<T>`, `Pick<T, K>`, and `Omit<T, K>` instead of manually rewriting interfaces.

You SHOULD initialize all variables on declaration.
This practice avoids undefined values.

### Code Structure

A function SHOULD have less than 48 lines of code.
Extract smaller functions when the function exceeds this length.
Each function does one thing only.

You SHOULD limit parameters to 5 arguments.
You MUST consolidate arguments into an options object when the count exceeds 8.

Keep public and internal methods briefly documented.

You SHOULD use `const` and `let`.
You SHOULD NOT use `var`.

You SHOULD use `===` and `!==`.
You SHOULD NOT use `==` or `!=`.

### Immutability and Purity

You SHOULD prefer pure functions.
A pure function returns values determined solely by inputs and has no side effects.

You SHOULD prefer immutability.
Use spread operators and `Readonly<T>` to create new objects instead of mutating existing ones.

You MUST centralize side effects.
Isolate mutations such as file I/O, network calls, and global state changes to specific modules.

You MUST NOT use magic numbers.
Extract meaningful constants instead of hardcoding values.

### Modern Syntax

You SHOULD use parameter defaults.
Write `function logNumber(num = 25): void {}` instead of checking for `undefined`.

You SHOULD use spread and destructuring.
Use `{ ...obj }` and `{ key } = obj` instead of manual copying.

You SHOULD use template literals.
Use `` `Hello ${name}` `` instead of string concatenation.

You SHOULD prefer array methods.
Use `map`, `filter`, and `reduce` instead of `for` loops when readability matters.

You SHOULD prefer `async/await` syntax.
Avoid callbacks and `.then()` chains.

### Naming and Readability

You SHOULD name for intent, not implementation.
Write `isLegalDrinkingAge()` instead of `isOverEighteen()` because constants change but intent remains stable.

You SHOULD prefix booleans with `is` or `has`.
Use `isValid` and `hasPermission`.

You SHOULD use meaningful names.
Avoid `x1`, `temp`, and `data`.
Names MUST explain purpose without comments.

You MUST remove unused imports.
Run the linter to clean dependencies automatically.

### Comments

You SHOULD write informative comments only.
Explain *why*, not *what*.
Refactor the code first when it needs a comment to be understood.

You MUST NOT leave commented-out code.
Delete it because git history preserves it.

You MUST NOT write noise or redundant comments.
A comment like `x = 5 // assign 5 to x` adds no value.

### Error Handling

You MUST throw proper `Error` objects.
Write `throw new Error('message')` and avoid `throw 'string'` or `throw 42`.

You MUST handle promise rejections.
Use `try/catch` with `async/await` or `.catch()` on promises.

You MUST NOT trust external data.
Validate all user input, API responses, and URL parameters before use.

### Architecture

You MUST NOT use globals.
Encapsulate logic in modules and classes.

You SHOULD keep functions small and focused.
Modular design makes debugging and testing easier.

You SHOULD create helper functions for repeated logic.
Apply the DRY principle without over-abstraction.

You SHOULD use configuration objects.
Externalize changing values into config instead of hardcoding them.
