﻿using System.Globalization;

namespace KrasnyyOktyabr.JsonTransform.Expressions;

/// <summary>
/// Casts inner expression result to <see cref="double"/> or translates it to <see cref="string"/> and parses.
/// </summary>
public sealed class DoubleCastExpression(IExpression<Task> innerExpression) : AbstractCastExpression<double>(innerExpression)
{
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="DoubleCastExpressionException"></exception>
    public override double Cast(object? innerExpressionTaskResult)
    {
        if (innerExpressionTaskResult == null)
        {
            throw new ArgumentNullException(nameof(innerExpressionTaskResult));
        }

        if (innerExpressionTaskResult is double doubleResult)
        {
            return doubleResult;
        }
        else if (double.TryParse(
            innerExpressionTaskResult?.ToString(),
            style: NumberStyles.Any,
            provider: CultureInfo.InvariantCulture,
            out double parseResult))
        {
            return parseResult;
        }
        else
        {
            throw new DoubleCastExpressionException(innerExpressionTaskResult, Mark);
        }
    }

    public class DoubleCastExpressionException : AbstractCastExpressionException
    {
        internal DoubleCastExpressionException(object? value, string? mark) : base(value, typeof(double), mark)
        {
        }
    }
}
