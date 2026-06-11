#!/usr/bin/env node

import { execSync } from 'node:child_process';

console.log('Checking for API drift...');

try {
  const output = execSync('git status --porcelain client/openapi.json client/app/api-client/', {
    encoding: 'utf8',
    stdio: ['pipe', 'pipe', 'pipe'],
  }).trim();

  if (output) {
    console.error('❌ API drift detected! Run \'just client-gen\' and commit the changes.');
    console.error(output);
    process.exit(1);
  } else {
    console.log('✅ No API drift detected.');
  }
} catch (err) {
  console.error('❌ Failed to check for API drift.');
  console.error(err.message);
  process.exit(1);
}
