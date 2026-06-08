# The spec for validating the PostPaymentRequest

1. CardNumber
  * 1.1 Required
  * 1.2 The characters count MUST >= 14 and <= 19
  * 1.3 MUST only contain numeric characters

2. ExpiryMonth
  * 2.1 Required
  * 2.2 Value must between 1 - 12

3. ExpiryYear
  * 3.1 Required
  * 3.2 The date ExpiryMonth/ExpiryYear MUST be in the future. The current month is excluded.

4. Currency
  * 4.1 Required
  * 4.1 MUST have exactly 3 characters.
  * 4.2 MUST be in the SupportedCurrencies list, case insensitive.

5. Amount
  * 5.1 Required

5. Cvv
  * 5.1 Required
  * 5.2 The characters count MUST >= 3 and <= 4
  * 5.3 MUST only contain numeric characters
  * 5.4 Cvv MAY have leading zeros

`ValidateRequest` returns true on success, throws `PaymentValidationException` with field and message set.
Write clear comment with requirement number for each validation.