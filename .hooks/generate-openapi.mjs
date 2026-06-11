#!/usr/bin/env node

import { execSync } from 'node:child_process';
import fs from 'node:fs';

const cacheFile = 'api/obj/Chatbot.Api.OpenApiFiles.cache';
const sourceFile = 'api/obj/Chatbot.Api.json';
const targetFile = 'client/openapi.json';

// Remove cache file if it exists to force regeneration
if (fs.existsSync(cacheFile)) {
  fs.rmSync(cacheFile);
}

// Run dotnet build to generate the OpenAPI document
try {
  execSync('dotnet build api/Chatbot.Api.csproj -p:OpenApiGenerateDocumentsOnBuild=true', { 
    stdio: 'inherit' 
  });
} catch (error) {
  console.error('❌ Failed to build and generate OpenAPI document.');
  process.exit(1);
}

// Move generated file
if (fs.existsSync(sourceFile)) {
  fs.copyFileSync(sourceFile, targetFile);
  fs.rmSync(sourceFile);
} else {
  console.error(`❌ Expected generated file at ${sourceFile} but it was not found.`);
  process.exit(1);
}
