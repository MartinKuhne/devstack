export const EXIT_CODE_SUCCESS = 0;
export const EXIT_CODE_ERROR = 2;
export const EXIT_CODE_PLAN_FAILED = 3;

/**
 * Output a standard message to the console.
 */
export function printMessage(message: string): void {
  console.log(message);
}

/**
 * Output an error message to stderr.
 */
export function printError(error: string): void {
  process.stderr.write(`error: ${error}\n`);
}

/**
 * Exit the process with a specified exit code.
 */
export function exitProcess(code: number): never {
  process.exit(code);
}
