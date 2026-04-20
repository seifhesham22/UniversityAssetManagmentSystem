using FluentValidation;
using MediatR;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Campus.Behaviours
{
    public sealed class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var results = await Task.WhenAll(validators.Select(x => x.ValidateAsync(context, cancellationToken)));

            var faliures = results.SelectMany(r => r.Errors).Where(e => e is not null).ToList();

            if (faliures.Count != 0)
                throw new ValidationException(faliures);

            return await next(cancellationToken);
        }
    }
}