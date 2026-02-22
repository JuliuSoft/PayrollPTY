# Payroll rules (Panama placeholders)
- Effective-dated `StatutoryConfig` controls CSS employee/employer rates.
- ISR brackets stored as JSON array: `{ from, to, rate, fixedQuota }`.
- Rounding mode persisted per config and attached to each line item trace.
- Future legal updates: insert new effective-dated config row, keep previous immutable.
