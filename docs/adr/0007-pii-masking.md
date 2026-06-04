# ADR 0007: PII Masking & AI Compliance

## Status

Accepted

## Context

Sending raw customer data (Credit cards, PII) to third-party AI APIs (OpenAI) poses significant security and compliance risks (GDPR/SOC2).

## Decision

We will implement a mandatory **PII Masking Broker** for all AI-bound traffic.

### Implementation:

1. **Anonymization:** A specialized Broker will scan outgoing text for sensitive entities using `Microsoft.Recognizers.Text` or regex.
2. **Masking:** Sensitive data will be replaced with placeholders (e.g., `[PHONE_NUMBER]`) **before** leaving our boundary.
3. **Re-hydration:** If necessary, responses will be re-hydrated at the Orchestration level using an internal vault.

### Benefits:

- **Compliance:** Zero-leakage policy for PII to external vendors.
- **Security:** Reduces the attack surface of our AI integration.

## Consequences

- Slight latency overhead for outgoing AI requests.
- Potential for false positives/negatives in scrubbing (requires continuous refinement).
