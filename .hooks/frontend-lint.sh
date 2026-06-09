#!/usr/bin/env bash

# Filter files that belong to client/ and strip the prefix
files=()
for arg in "$@"; do
    if [[ "$arg" == client/* ]]; then
        files+=("${arg#client/}")
    fi
done

# If there are files to lint, run eslint from client/
if [ ${#files[@]} -gt 0 ]; then
    exec bun --cwd client node_modules/eslint/bin/eslint.js --config eslint.config.mjs --fix "${files[@]}"
fi
