import { execSync } from 'node:child_process';
import { copyFileSync } from 'node:fs';

/**
 * Environment Initialization Script
 * 
 * 1. Checks if the user is logged into Infisical.
 * 2. If not, provides instructions to log in.
 * 3. Copies .env.local.example to .env.local.
 */

const EXAMPLE_FILE = '.env.local.example';
const TARGET_FILE = '.env.local';

function checkInfisicalLogin() {
  try {
    // 'infisical status' returns 0 if logged in, non-zero otherwise
    execSync('infisical status', { stdio: 'ignore' });
    return true;
  } catch {
    return false;
  }
}

console.log('🔍 Checking Infisical authentication status...');

if (!checkInfisicalLogin()) {
  console.error('\n❌ Error: Not logged into Infisical.');
  console.error('This project requires an active Infisical session to source primary secrets.');
  console.error('\n👉 Please run the following command to authenticate:');
  console.error('\n    infisical login\n');
  console.error('After logging in, please run this setup command again.\n');
  process.exit(1);
}

try {
  copyFileSync(EXAMPLE_FILE, TARGET_FILE);
  console.log(`\n✅ Successfully initialized ${TARGET_FILE} from ${EXAMPLE_FILE}`);
  console.log('Environment setup is complete. You can now proceed with "just setup" or "just run".\n');
} catch (error) {
  console.error(`\n❌ Error: Failed to copy ${EXAMPLE_FILE} to ${TARGET_FILE}`);
  console.error(error.message);
  process.exit(1);
}
