GuardCore
===
[![NuGet](https://img.shields.io/nuget/v/GuardCore)](https://www.nuget.org/packages/GuardCore)

Zero-allocation guard clauses and railway-oriented `Result` handling for .NET and Unity, built for hot paths where exceptions and heap allocation are not an option.

Unlike general-purpose Result libraries, GuardCore is intentionally narrow: `GuardState<TError>` is a `ref struct` that never leaves the stack, and errors are `Enum` values instead of heap-allocated objects or strings. The tradeoff is deliberate — this is not a drop-in replacement for FluentResults or ErrorOr, it's a genuinely free (zero-GC) alternative for the slice of C# that can't afford what those libraries cost: game loops, editor tooling, real-time/embedded code, and anywhere a `Debug.Log` + silent `return false` was doing the job of an exception without any of the safety.

```bash
dotnet add package GuardCore
```

```csharp
using GuardCore;
using static GuardCore.Guard;

var guard = Expect(user != null, UserError.NotFound)
    .And(user.IsActive, UserError.Deactivated)
    .And(user.HasPermission(Permission.Edit), UserError.Forbidden);

guard.OnFailure(err => Log(err));
```

* **Zero allocation** guard chains via `ref struct` `GuardState<TError>` — never boxed, never heap-allocated, inlined aggressively
* **Enum-based errors** instead of exceptions or string messages — comparable, switchable, allocation-free
* **Railway-oriented `Result<TValue, TError>`** for composing fallible operations that actually produce a value (`Bind`, `Map`, `Match`)
* **No exceptions in the hot path** — failures are data, not control flow, so a bad frame never means a stack unwind
* Built for **Unity, embedded, and real-time C#** where GC pauses are visible stutter, not an abstraction

Why this exists
---
Most C# codebases handle expected, recoverable failures (a null reference, an out-of-range value, a missing asset) the same way they handle actual bugs: throwing an exception. That's fine once per HTTP request. It's a real cost when it happens 60 times a second inside `Update()`, or on a thread with a hard real-time deadline where an unhandled `try/catch` is the difference between a clean frame and an audible glitch.

The existing Result-style libraries in the .NET ecosystem (FluentResults, ErrorOr, LanguageExt) solve the "stop using exceptions for control flow" problem well, but they solve it by allocating — lists of reasons, rich error objects, boxed values. That's a reasonable price in a web API. It's not a price you can pay in a per-frame gameplay system or an audio callback thread.

GuardCore exists for that second category: the same railway-oriented idea, with every allocation stripped out, at the cost of being deliberately narrower in scope (sync-only, enum errors, no multi-error accumulation).

Two Types, Two Jobs
---
GuardCore separates **"is it safe to proceed"** from **"what do I do with the value that came out of it"** — and enforces that separation with the type system, not just convention.

```csharp
public readonly ref struct GuardState<TError> where TError : Enum { /* ... */ }
public readonly struct Result<TValue, TError> where TError : Enum { /* ... */ }
```

* **`GuardState<TError>` — the gate.** Answers pass/fail plus a reason. Nothing needs to survive past the `if`. It's a `ref struct` on purpose: a decision never needs to outlive the stack frame that made it, so the compiler enforces "don't stash this in a field, don't capture it in a closure, don't hold it across `await`."
* **`Result<TValue, TError>` — the carrier.** Use this the moment an operation needs to hand a *value* across a boundary — return it from a method, compose it with `Bind`/`Map`, pattern-match it in a caller. It's a normal struct because values legitimately need to travel.

```csharp
// Gate: no value needs to leave, mutation happens in-place
public GuardState<CastError> CastAbility(AbilityId id, Transform target)
{
    var guard = Expect(_abilityDatabase.TryGetValue(id, out _pendingAbility), CastError.UnknownAbility)
        .And(_mana >= _pendingAbility.ManaCost, CastError.InsufficientMana)
        .And(!_pendingAbility.RequiresTarget || target != null, CastError.TargetRequired);

    guard.OnSuccess(ApplyCast); // method group — no closure allocation
    return guard;
}

// Carrier: a Sprite needs to come back out to the caller
public Result<Sprite, LoadError> LoadCachedSprite(string path)
{
    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
    return Expect(sprite, LoadError.AssetMissing).ToResult(sprite);
}
```

If nothing ever reads `.Value` or `.Error` after the check, you don't need `Result` — `GuardState` alone, consumed via `OnSuccess`/`OnFailure`, is the whole answer and costs nothing.

Getting Started
---
Install from [NuGet](https://www.nuget.org/packages/GuardCore), define your error enum, and chain checks with `Expect`/`And`/`Or`/`Not`.

```csharp
public enum ValidationError { None, NullInput, OutOfRange, Empty }

var result = Expect(input != null, ValidationError.NullInput)
    .And(input.Length > 0, ValidationError.Empty)
    .And(input.Value is >= 0 and <= 100, ValidationError.OutOfRange);

if (result.Failed)
{
    HandleError(result.Error);
    return;
}
```

Core API
---

### `Guard.Expect(condition, error)`
Starts a guard chain. Returns a `GuardState<TError>` that is either `Success` or carries `error`.

### `.And(condition, error)` / `.And(Func<bool>, error)`
Conjunction. Short-circuits — if the chain already failed, the new condition is never evaluated (the `Func<bool>` overload is for expensive checks you don't want run unless necessary).

### `.Or(condition, error)`
Disjunction. **If the chain already succeeded, this is skipped entirely** — meaning a failed `Or` only ever reports the *most recent* attempted condition's error, not the original one. Useful for "any one of these is acceptable" checks; be careful relying on the specific error value coming out of a long `Or` chain.

### `.Not(error)`
Inverts the *whole accumulated chain's* success/failure, not just the last check. On success → failure, it applies `error`; on failure → success, the prior error is discarded. Think of it as De Morgan's law over the entire conjunction, not a per-condition negation.

### `.OnSuccess(Action)` / `.OnFailure(Action<TError>)`
Consumes the result with a reaction. **Always prefer method groups over lambdas here** (`.OnSuccess(ApplyCast)` not `.OnSuccess(() => ApplyCast(x, y))`) — a lambda that captures locals allocates a closure on the heap, which defeats the entire point of `GuardState` being a `ref struct`. If you need per-call state, store it in fields on the containing object instead.

### `.Then<T>(value)` / `.Then<T>(Func<T>)` / `.ToResult()` / `.ToResult<T>(value)`
Promotes a `GuardState<TError>` into a `Result<T, TError>` once you actually need a value to leave the current scope.

### `Result<TValue, TError>`
`Map`, `Bind`, `Match`, `OnSuccess`, `OnFailure`, `Deconstruct` — standard railway-oriented composition. `Bind` short-circuits on the first failure in the chain; `Map` transforms the success value while passing errors through untouched.

### `Guard.Ensure(condition, message)`
The one exception-throwing escape hatch in the library, intentionally separate from everything above. Use it for invariants — conditions that indicate a bug, not an expected outcome (e.g. "this should be structurally impossible given prior checks"). Never call `Ensure` on a condition you're also checking with `Expect`/`Result` in the same place — pick one strategy per fact, or you'll get dead branches and a masked exception risk at the same time.

Comparison
---

| | **GuardCore** | **FluentResults / ErrorOr** | **LanguageExt** |
|---|---|---|---|
| Error type | `Enum` only | rich object / string, list-based | any type, usually rich |
| Allocation | zero (`ref struct` + struct) | heap (reason lists, boxed errors) | heap for `Either`/`Fin` |
| Async support | sync-only, by design | first-class | first-class |
| Multi-error accumulation | no — single error per chain | yes | yes (`Validation<T>`) |
| Scope | guard clause + result, narrow | general-purpose Result library | full FP toolkit |
| Best fit | game loops, editors, real-time, embedded | web APIs, backend services | codebases already committed to FP |

GuardCore is not trying to replace these libraries for backend work — for a web API, one allocation per request is noise, and you'll want their richer error objects and async support. GuardCore's entire reason to exist is the case where the allocation *isn't* noise: a per-frame check, a real-time audio callback, an editor tool re-running every repaint.

Difference and Limitations
---
`GuardState<TError>` is a `ref struct`. It cannot be stored in a field, boxed, captured in a closure, or held live across an `await` point. In practice this is rarely a real restriction — build and consume the chain in one statement (`Expect(...).And(...).OnFailure(...)`), and it works fine inside `async` methods, Unity coroutines, and anywhere else, as long as you don't try to stash a half-finished chain and resume it later. If you need to defer a decision across an async boundary, call `.ToResult()` first and carry the (heap-friendly) `Result<T, TError>` forward instead.

`Or` and `Not` operate on the whole accumulated chain, not the single condition being written on that line — see the API reference above. Read the doc comments before relying on the specific error value they produce in a multi-condition chain.

Reading `.Value` or `.Error` on the wrong branch of `Result<TValue, TError>` currently returns `default` rather than throwing. Prefer `Match`/`Deconstruct` to access the payload safely rather than reading the properties directly.

This library assumes single-threaded, non-reentrant use per guarded operation. It is not a substitute for proper concurrency control — `GuardState`/`Result` describe an outcome, they don't synchronize access to the data behind it.

License
---
This library is under MIT License.
