# ValidateRequest Test Cases

Tests for `MounteBankAdapter.ValidateRequest` in `ValidationTest.cs`, based on [validation_spec.md](../../src/PaymentGateway.Api/MounteBank/validation_spec.md).

**Total:** 45 test executions (27 test methods, including parameterized `[Theory]` cases)

## Test Helpers

| Helper | Purpose |
| --- | --- |
| `CreateValidRequest()` | Builds a fully valid `PostPaymentRequest` with a future expiry date |
| `AssertValidationFails(expectedField, action)` | Asserts `PaymentValidationException` is thrown and `Field` matches |

### Baseline Valid Request

| Field | Value |
| --- | --- |
| CardNumber | `"4242424242424242"` |
| ExpiryMonth | Next month (or `1` when current month is December) |
| ExpiryYear | Current year (or next year when current month is December) |
| Currency | `"GBP"` |
| Amount | `100` |
| Cvv | `"123"` |

---

## Success Cases

These tests assert `ValidateRequest` returns `true`.

| Test | Spec | Input change |
| --- | --- | --- |
| `ValidateRequest_ReturnsTrue_WhenRequestIsValid` | All | Baseline valid request |
| `ValidateRequest_ReturnsTrue_WhenCardNumberIs14Characters` | 1.2 | CardNumber = `"42424242424242"` (14 chars) |
| `ValidateRequest_ReturnsTrue_WhenCardNumberIs19Characters` | 1.2 | CardNumber = `"4242424242424242424"` (19 chars) |
| `ValidateRequest_ReturnsTrue_WhenCardNumberHasLeadingAndTrailingWhitespace` | 1.1 | CardNumber = `"  4242424242424242  "` |
| `ValidateRequest_ReturnsTrue_WhenCurrencyIsSupportedCaseInsensitive` | 4.2 | Currency = `GBP`, `gbp`, `CNY`, `cny`, `EUR`, `eur` |
| `ValidateRequest_ReturnsTrue_WhenCurrencyHasLeadingAndTrailingWhitespace` | 4.1 | Currency = `"  GBP  "` |
| `ValidateRequest_ReturnsTrue_WhenCvvHasValidLength` | 5.2 | Cvv = `"123"`, `"1234"` |
| `ValidateRequest_ReturnsTrue_WhenCvvHasLeadingAndTrailingWhitespace` | 5.1 | Cvv = `"  123  "` |
| `ValidateRequest_ReturnsTrue_WhenCvvHasLeadingZeros` | 5.4 | Cvv = `"012"`, `"0012"` |

---

## Failure Cases

Each failure test asserts:

1. `PaymentValidationException` is thrown
2. `Field` on the exception equals the expected value

### Request

| Test | Expected Field | Input |
| --- | --- | --- |
| `ValidateRequest_Throws_WhenRequestIsNull` | `Request` | `null` |

### CardNumber

| Test | Spec | Expected Field | Input |
| --- | --- | --- | --- |
| `ValidateRequest_Throws_WhenCardNumberIsMissing` | 1.1 | `CardNumber` | `null`, `""`, `"   "` |
| `ValidateRequest_Throws_WhenCardNumberIsTooShort` | 1.2 | `CardNumber` | `"4242424242424"` (13 chars) |
| `ValidateRequest_Throws_WhenCardNumberIsTooLong` | 1.2 | `CardNumber` | `"42424242424242424242"` (20 chars) |
| `ValidateRequest_Throws_WhenCardNumberContainsNonNumericCharacters` | 1.3 | `CardNumber` | `"42424242424242A2"` |

### ExpiryMonth

| Test | Spec | Expected Field | Input |
| --- | --- | --- | --- |
| `ValidateRequest_Throws_WhenExpiryMonthIsMissing` | 2.1 | `ExpiryMonth` | `null` |
| `ValidateRequest_Throws_WhenExpiryMonthIsOutOfRange` | 2.2 | `ExpiryMonth` | `0`, `13`, `-1` |

### ExpiryYear / Expiry Date

| Test | Spec | Expected Field | Input |
| --- | --- | --- | --- |
| `ValidateRequest_Throws_WhenExpiryYearIsMissing` | 3.1 | `ExpiryYear` | `null` |
| `ValidateRequest_Throws_WhenExpiryDateIsInPastYear` | 3.2 | `ExpiryYear` | ExpiryYear = `UtcNow.Year - 1`, ExpiryMonth = `12` |
| `ValidateRequest_Throws_WhenExpiryDateIsCurrentMonth` | 3.2 | `ExpiryYear` | ExpiryYear = current year, ExpiryMonth = current month |

### Currency

| Test | Spec | Expected Field | Input |
| --- | --- | --- | --- |
| `ValidateRequest_Throws_WhenCurrencyIsMissing` | 4.1 | `Currency` | `null`, `""`, `"   "` |
| `ValidateRequest_Throws_WhenCurrencyLengthIsNotThreeCharacters` | 4.1 | `Currency` | `"GB"`, `"GBPP"` |
| `ValidateRequest_Throws_WhenCurrencyIsNotSupported` | 4.2 | `Currency` | `"USD"` |

Supported currencies: `GBP`, `CNY`, `EUR` (case insensitive).

### Amount

| Test | Spec | Expected Field | Input |
| --- | --- | --- | --- |
| `ValidateRequest_Throws_WhenAmountIsMissing` | 5.1 | `Amount` | `null` |

### Cvv

| Test | Spec | Expected Field | Input |
| --- | --- | --- | --- |
| `ValidateRequest_Throws_WhenCvvIsMissing` | 5.1 | `Cvv` | `null`, `""`, `"   "` |
| `ValidateRequest_Throws_WhenCvvIsTooShort` | 5.2 | `Cvv` | `"12"`, `"1"` |
| `ValidateRequest_Throws_WhenCvvIsTooLong` | 5.2 | `Cvv` | `"12345"` |
| `ValidateRequest_Throws_WhenCvvContainsNonNumericCharacters` | 5.3 | `Cvv` | `"12A"`, `"abc"` |

---

## Spec Coverage Summary

| Requirement | Success | Failure |
| --- | --- | --- |
| Request required | — | Yes |
| 1.1 CardNumber required | Trim whitespace | null, empty, whitespace |
| 1.2 CardNumber length 14–19 | 14 and 19 chars | 13 and 20 chars |
| 1.3 CardNumber numeric only | — | Non-numeric character |
| 2.1 ExpiryMonth required | — | null |
| 2.2 ExpiryMonth 1–12 | — | 0, 13, -1 |
| 3.1 ExpiryYear required | — | null |
| 3.2 Expiry date in future | Valid request uses future date | Past year, current month |
| 4.1 Currency required | Trim whitespace | null, empty, whitespace |
| 4.1 Currency exactly 3 chars | — | 2 and 4 chars |
| 4.2 Supported currency | GBP/CNY/EUR (any case) | USD |
| 5.1 Amount required | — | null |
| 5.1 Cvv required | Trim whitespace | null, empty, whitespace |
| 5.2 Cvv length 3–4 | `"123"` and `"1234"` | `"1"`, `"12"`, `"12345"` |
| 5.3 Cvv numeric only | — | `"12A"`, `"abc"` |
| 5.4 Cvv leading zeros allowed | `"012"`, `"0012"` | — |

## Running the Tests

```bash
dotnet test test/PaymentGateway.Api.Tests/PaymentGateway.Api.Tests.csproj --filter "FullyQualifiedName~ValidationTest"
```
