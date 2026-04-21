# EARS (Easy Approach to Requirements Syntax) formatted universal non-functional requirements

## Logging

- [REQ-NF-001] The system shall write log entries to the console
- [REQ-NF-002] When the system is a binary executable, if shall write log entries to a file, one file per day, in structured JSON format.
- [REQ-NF-003] When the system is a binary executable, the log retention period is 7 days
- [REQ-NF-004] The system shall include a timestamp in UTC ISO 8601 format for every log entry.
- [REQ-NF-005] When the system starts up or shuts down gracefully, it shall write a log entry with the level INFO.
- [REQ-NF-006] When the system encounters an unexpected exception, it shall write a log entry with the level ERROR including the full stack trace.
- [REQ-NF-007] When the system attempts a retry operation, it shall write a log entry with the level WARN.
- [REQ-NF-008] While the system is operating in production mode, it shall not log sensitive data (e.g., passwords, credit card numbers, PII).

## Tracing

- [REQ-NF-100] The system shall generate a unique TraceID for every incoming request that does not already contain one.
- [REQ-NF-101] The system shall propagate the TraceID to all downstream services and external dependencies via HTTP headers.
- [REQ-NF-102] The system shall adhere to the W3C Trace Context standard for trace identifier headers.
- [REQ-NF-103] The system shall export tracing data to an OpenTelemetry-compatible collector.
- [REQ-NF-104] When the system receives a request, it shall create a "Root Span" representing the processing of that entire request.
- [REQ-NF-105] When the system calls an external dependency, it shall create a "Child Span" linked to the active "Root Span".
- [REQ-NF-106] When the system detects a transient failure (e.g., network timeout), it shall perform a retry operation using an exponential backoff strategy.
- [REQ-NF-107] While the system is processing a user request, it shall include the CorrelationID in every log entry generated during that request.
- [REQ-NF-108] While a specific trace is active, the system shall ensure that all SpanIDs are logically and hierarchically linked to the parent TraceID.

## Error handling

- [REQ-NF-200] The system shall implement a global exception handler to catch unhandled exceptions.
- [REQ-NF-201] The system shall map specific technical exceptions to appropriate HTTP status codes (e.g., 400 Bad Request, 401 Unauthorized, 404 Not Found, 500 Internal Server Error).
- [REQ-NF-202] If an unhandled system exception occurs, then the system shall not display raw stack traces or technical error messages to the end-user.
- [REQ-NF-203] If a validation error occurs, then the system shall display specific error messages indicating which fields were invalid.

