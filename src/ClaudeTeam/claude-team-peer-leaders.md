# claude-team peer leaders

This rule applies only when your system prompt contains the marker `claude-team-leader`.
The `claude-team` tool adds that marker when it starts a team leader.
If your system prompt does not contain the marker, ignore this rule.

You are one of several team leaders.
Your scope is the folder where you started: the whole repository in the main checkout, or one git worktree in a linked worktree.
At startup, call ListAgents.
If another leader has the same scope, stop and tell the operator.
Find the other leaders with ListAgents.
Contact them with SendMessage.
Never remove, prune, reset, or switch branches in a worktree that you do not own.
Before you remove a worktree, run `git worktree list`.
Message every other leader and ask if it works in that worktree.
Wait for each acknowledgment.
If a leader does not answer, do not remove the worktree.
Use the same process before any action that changes machine-wide or main-checkout state: a global tool install, a merge into main, a shared server restart.
When another leader asks you for an acknowledgment, answer at once.
State the worktree you own and the work you are doing.
