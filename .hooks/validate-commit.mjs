#!/usr/bin/env node

import fs from 'node:fs';

const commitMsgFile = process.argv[2];
if (!commitMsgFile) {
  console.error('❌ Error: No commit message file provided.');
  process.exit(1);
}

let commitMsg;
try {
  commitMsg = fs.readFileSync(commitMsgFile, 'utf8').trim();
} catch (err) {
  console.error(`❌ Error: Could not read commit message file: ${err.message}`);
  process.exit(1);
}

// 1. Define Emoji Mapping (Abstract/Inanimate only)
const EMOJI_MAP = {
  feat: '📦',
  fix: '🔧',
  perf: '⚡',
  refactor: '♻️',
  test: '🧪',
  docs: '📝',
  style: '🎨',
  chore: '⚙️',
};

// 2. Check for prohibited animate emojis first
const BLOCKLIST = /🐛|🚀|🐳|👤|🤖|👨|👩|🧒|👶|🐕|🐈/;
if (BLOCKLIST.test(commitMsg)) {
  console.error('❌ Error: Commit message contains prohibited animate emojis.');
  console.error('Mandate: Only use abstract/inanimate gitmojis (🔧, ⚙️, ⚡, 📝, 📦, ♻️, 🧪, 🎨).');
  process.exit(1);
}

// 3. Regex Patterns
const TYPES = Object.keys(EMOJI_MAP).join('|');
const CORE_PATTERN = `(${TYPES})\\s*(\\([^)]+\\))?!?:[ ]+.+`;

// Full format: <emoji> <type>(<scope>): <message>
const FULL_CONVENTIONAL_REGEX = new RegExp(`^[^ ]+[ ]+${CORE_PATTERN}`);

// Bare format: <type>(<scope>): <message>
const BARE_CONVENTIONAL_REGEX = new RegExp(`^${CORE_PATTERN}`);

// 4. Check if message is already in the FULL format
if (FULL_CONVENTIONAL_REGEX.test(commitMsg)) {
  process.exit(0);
}

// 5. Check if it's a bare conventional commit and prepend emoji
if (BARE_CONVENTIONAL_REGEX.test(commitMsg)) {
  // Extract type
  const typeMatch = commitMsg.match(/^[a-z]+/);
  const type = typeMatch ? typeMatch[0] : null;
  const emoji = EMOJI_MAP[type];

  if (emoji) {
    const newMsg = `${emoji} ${commitMsg}`;
    try {
      fs.writeFileSync(commitMsgFile, newMsg);
      console.log(`✅ Automatically added emoji: ${newMsg}`);
      process.exit(0);
    } catch (err) {
      console.error(`❌ Error: Could not write updated commit message: ${err.message}`);
      process.exit(1);
    }
  }
}

// 6. Fallback
console.error('❌ Error: Invalid commit message format.');
console.error('Expected: <type>(<scope>): <message> OR <emoji> <type>(<scope>): <message>');
console.error('Example: feat(chat): message  ->  📦 feat(chat): message');
console.error('Example: fix!: breaking bug   ->  🔧 fix!: breaking bug');
process.exit(1);
