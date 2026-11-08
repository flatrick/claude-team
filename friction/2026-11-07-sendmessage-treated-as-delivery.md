# The leader treated a successful `SendMessage` as delivery

- Old ID: F-004
- Date: 2026-11-07
- Observation: The leader treated a successful `SendMessage` result as delivery.
  The leader did not check the `read` flags in the inbox of the recipient.
  Six instructions went unread for about 15 minutes.
  The leader told the operator that work was in progress.
- Evidence: the inbox file `session-0f676c72/inboxes/claude-dir-cleanup.json` showed `read: false` for those messages.
  See the F-003 evidence folder.
- Effect: The operator received a wrong status. The work did not run.
- Cause: verified. The inbox file showed `read: false`.
  A successful send means that the message reached the inbox, not that the recipient read it.
- Status: open.
