module.exports = {
  extends: ["@commitlint/config-conventional"],
  parserPreset: {
    parserOpts: {
      // Allows an optional prefix (like an emoji) followed by a space, then the conventional commit header.
      // Group 1: Optional prefix (ignored in correspondence)
      // Group 2: Type
      // Group 3: Optional Scope
      // Group 4: Optional Breaking Change indicator (!)
      // Group 5: Subject
      headerPattern: /^(?:.*?\s+)?(\w+)(?:\(([\w\$\.\-\* ]+)\))?(!?): (.*)$/,
      headerCorrespondence: ["type", "scope", "breaking", "subject"],
    },
  },
  rules: {
    "type-enum": [
      2,
      "always",
      ["feat", "fix", "perf", "refactor", "test", "docs", "style", "chore"],
    ],
    "subject-empty": [2, "never"],
    "type-empty": [2, "never"],
  },
};
