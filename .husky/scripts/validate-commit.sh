#!/usr/bin/env bash

# Get the commit message from the file passed by Husky
COMMIT_MSG_FILE=$1
COMMIT_MSG=$(cat "$COMMIT_MSG_FILE")

# 1. Check for Conventional Commit Format with Scope: <emoji> <type>(<scope>): <message>
# Regex explanation:
# ^[^ ]+ : Starts with an emoji (or any non-space char)
# [ ]+ : Followed by spaces
# (feat|fix|perf|refactor|test|docs|style|chore) : Valid types
# \([^)]+\) : Mandatory scope in parentheses
# :[ ]+ : Colon followed by spaces
# .+ : The message
CONVENTIONAL_REGEX="^[^ ]+[ ]+(feat|fix|perf|refactor|test|docs|style|chore)\([^)]+\):[ ]+.+"

if [[ ! $COMMIT_MSG =~ $CONVENTIONAL_REGEX ]]; then
    echo "❌ Error: Invalid commit message format."
    echo "Expected: <emoji> <type>(<scope>): <message>"
    echo "Example: 📦 feat(chat): implement persistence"
    exit 1
fi

# 2. Strict Blocklist for Animate Gitmojis/Emojis
# Prohibited: 🐛 (bug), 🚀 (rocket), 🐳 (docker), 👤 (person), 🤖 (robot), 👨, 👩, 🧒, 👶, 🐕, 🐈
BLOCKLIST="🐛|🚀|🐳|👤|🤖|👨|👩|🧒|👶|🐕|🐈"

if echo "$COMMIT_MSG" | grep -qE "$BLOCKLIST"; then
    echo "❌ Error: Commit message contains prohibited animate emojis."
    echo "Mandate: Only use abstract/inanimate gitmojis (🔧, ⚙️, ⚡, 📝, 📦, ♻️, 🧪, 🎨)."
    exit 1
fi

exit 0
