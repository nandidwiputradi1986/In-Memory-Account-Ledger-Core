# Implementation Worklog

*   **[2026-09-12 13:51 WIB]**: Ingested prompt requirements. Identified core entities: Accounts, Append-only Ledger Events. Noted the 6-day window limitation.
*   **[2026-09-12 13:58 WIB]**: Trace simulated for E1-E10. Identified the critical edge cases: E7 is a backdated debit arriving on Day 5, affecting the available balance logic prior to E8 (Auth-B).
*   **[2026-09-12 14:07 WIB]**: Addressed acceptance criteria list. Found 5 invalid criteria (OD fee timing, Force-post rejection, cascade fee reversals, remainder discarding, and bad BHD math). Documented rejections in REJECTED.md.
*   **[2026-09-12 14:15 WIB]**: Wrote `LedgerCore.cs`. Encapsulated ledger computation in `GetLedgerBalance()` and `GetAvailableBalance()` functions to handle dynamic time-traveling (value vs. system day computations).
*   **[2026-09-12 14:26 WIB]**: Added EndOfDay processing. Overdraft fees evaluated strictly against current day ledger balance. Added Day 6 capitalization hook that recalculates the value curve.
*   **[2026-09-12 14:35 WIB]**: Built `Program.cs` to execute stream. Ran manual validation tests for Day 5 EOD. Verified Auth-B correctly triggers an NSF rejection due to E7 being ingested prior to Auth-B.
*   **[2026-09-12 14:48 WIB]**: Finalized markdown documentation and structured deliverables layout.
