# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow [SemVer](https://semver.org/).

## [1.0.0-preview.1] - Unreleased

### Added

- Etherscan API V2 client for .NET 11. It covers every endpoint published in the Etherscan OpenAPI specs (97 operations): accounts, tokens, blocks, contracts and verification, gas tracker, Geth/Parity proxy, logs, stats, transactions, L2, ENS, name tags, legacy metadata, usage and the chain list.
- `ChainId` value type with well-known chains, plus a per-call chain override.
- Typed models generated from the OpenAPI response schemas, with flexible converters for decimal, hex and number encodings.
- Typed exceptions mapped from Etherscan error envelopes. The API key is masked in all exception text.
- Client-side rate limiting (plan presets and per-endpoint limits) and retries for rate-limit and timeout errors. Submissions are never retried.
- `IAsyncEnumerable` pagination helpers.
- `AddEtherscanApi` dependency injection extensions, with reflection-free configuration binding.
- Native AOT and trimming compatibility.
- Module interfaces (`IAccountsModule`, `ITokensModule`, …) so consumers can mock the client.
- Fail-fast validation of options. A missing API key fails at construction, or at host start via `ValidateOnStart`.
- Single package dependency: `Microsoft.Extensions.Http`. Rate limiting is built in, with no `System.Threading.RateLimiting` dependency.
- Robustness. The client timeout also covers reading the response body; stream endpoints map timeouts to `EtherscanHttpException`; base addresses without a trailing slash keep their path; a missing required result raises `EtherscanResponseException` instead of returning `0` or `""`; page sizes are validated when the method is called (1–10,000, getLogs 1–1,000); `EtherscanRateLimitException.IsClientSide` marks local queue rejections.
- API shape for 1.0. `BlockTag` defaults to `Latest` and its implicit conversion from `long` never throws. `ChainId` → `ulong` is explicit. The page size parameter is `int? pageSize = null`. Modules are interfaces (`IAccountsModule`, …) meant to be consumed, not implemented.
- Single-pass parsing. The envelope is scanned with `Utf8JsonReader` and `result` is deserialized from the UTF-8 buffer. Numeric converters parse from stack buffers, with a `long` fast path before the hex/BigInteger fallback, and the JSON context is metadata-only.
