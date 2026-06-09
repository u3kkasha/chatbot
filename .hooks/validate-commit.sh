#!/usr/bin/env bash

# Get the commit message from the file passed by Lefthook
COMMIT_MSG_FILE=$1
COMMIT_MSG=$(cat "$COMMIT_MSG_FILE")

# 1. Define Emoji Mapping (Abstract/Inanimate only)
declare -A EMOJI_MAP
EMOJI_MAP["feat"]="📦"
EMOJI_MAP["fix"]="🔧"
EMOJI_MAP["perf"]="⚡"
EMOJI_MAP["refactor"]="♻️"
EMOJI_MAP["test"]="🧪"
EMOJI_MAP["docs"]="📝"
EMOJI_MAP["style"]="🎨"
EMOJI_MAP["chore"]="⚙️"

# 2. Check for prohibited animate emojis first
BLOCKLIST="🐛|🚀|🐳|👤|🤖|👨|👩|🧒|👶|🐕|🐈"
if echo "$COMMIT_MSG" | grep -qE "$BLOCKLIST"; then
    echo "❌ Error: Commit message contains prohibited animate emojis."
    echo "Mandate: Only use abstract/inanimate gitmojis (🔧, ⚙️, ⚡, 📝, 📦, ♻️, 🧪, 🎨)."
    exit 1
fi

# 3. Regex Patterns
# Types: feat|fix|perf|refactor|test|docs|style|chore
# Optional Scope: (\([^)]+\))?
# Optional Breaking Change: !?
TYPES="feat|fix|perf|refactor|test|docs|style|chore"
CORE_REGEX="($TYPES)(\([^)]+\))?!?:[ ]+.+"

# Full format: <emoji> <type>(<scope>): <message>
FULL_CONVENTIONAL_REGEX="^[^ ]+[ ]+$CORE_REGEX"

# Bare format: <type>(<scope>): <message>
BARE_CONVENTIONAL_REGEX="^$CORE_REGEX"

# 4. Check if message is already in the FULL format
if [[ $COMMIT_MSG =~ $FULL_CONVENTIONAL_REGEX ]]; then
    exit 0
fi

# 5. Check if it's a bare conventional commit and prepend emoji
if [[ $COMMIT_MSG =~ $BARE_CONVENTIONAL_REGEX ]]; then
    # Extract type (the first alphanumeric word)
    TYPE=$(echo "$COMMIT_MSG" | grep -oE "^[a-z]+" | head -n 1)
    EMOJI=${EMOJI_MAP[$TYPE]}
    
    if [[ -n "$EMOJI" ]]; then
        NEW_MSG="$EMOJI $COMMIT_MSG"
        echo "$NEW_MSG" > "$COMMIT_MSG_FILE"
        echo "✅ Automatically added emoji: $NEW_MSG"
        exit 0
    fi
fi

# 6. Fallback: If it doesn't match any supported format
echo "❌ Error: Invalid commit message format."
echo "Expected: <type>(<scope>): <message> OR <emoji> <type>(<scope>): <message>"
echo "Example: feat(chat): message  ->  📦 feat(chat): message"
echo "Example: fix!: breaking bug   ->  🔧 fix!: breaking bug"
exit 1
