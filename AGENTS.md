# Project versioning

For every completed change request that fixes a bug or adds a feature, increment
the project version once. If a request includes multiple changes, use the
highest applicable increment:

- Bug fix: increment patch, e.g. `0.10.2` to `0.10.3`.
- Backward-compatible feature: increment minor and reset patch, e.g. `0.10.2`
  to `0.11.0`.
- Breaking change: increment major and reset minor and patch, e.g. `0.10.2`
  to `1.0.0`.

Documentation-only, test-only, and other changes that do not alter product
behavior do not require a version increment.

Keep the version in the `MelonInfo` attribute in `Passthrough.cs` and the
`Version` property in `BigscreenPassthrough.csproj` identical. The SDK generates
assembly, file, and informational version attributes from the project version;
do not edit generated files under `obj`. If these sources disagree, use the
`MelonInfo` version as the starting point for the next increment, then sync both.
Build the project and verify the generated version attributes match the new
version (assembly and file versions use a fourth `.0` component).
