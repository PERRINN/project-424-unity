# Security Policy

## Reporting a vulnerability

If you find a security issue in this repository — malicious content, a compromised dependency, or a vulnerability in the simulation code — please report it privately rather than opening a public issue.

Use GitHub's [private vulnerability reporting](https://github.com/PERRINN/project-424-unity/security/advisories/new). It is the only channel here that keeps the report confidential, and it reaches the maintainers directly.

## Past incidents

**2026-09-15 — unauthorized force-push, malicious payload.** A `.vscode/tasks.json` configured to auto-run on folder open, paired with obfuscated JavaScript disguised as a web font, was pushed to 33 branches including `master`. Removed from all history on 2026-09-21; the credentials used were revoked. Details and remediation steps for anyone who cloned during that window: see issue [#96](https://github.com/PERRINN/project-424-unity/issues/96).
