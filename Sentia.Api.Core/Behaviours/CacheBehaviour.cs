using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Sentia.Api.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sentia.Api.Core.Behaviours
{
    public class CacheBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IMemoryCache _cache;

        public CacheBehaviour(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (!(request is ICacheable cacheable))
            {
                return await next();
            }

            if (request is INoCache noCache && noCache.NoCache)
            {
                return await next();
            }

            switch (cacheable.CacheOption)
            {
                case CacheOption.None:
                    return await next();
                case CacheOption.Memory:
                    return await GetFromMemoryCache(request, cacheable, next);
                case CacheOption.Distributed:
                    return await GetFromDistributedCache(request, cacheable, cancellationToken, next);
                case CacheOption.Multi:
                    return await GetFromMultiCache(request, cacheable, cancellationToken, next);
            }

            return await next();
        }

        private async Task<TResponse> GetFromMemoryCache(TRequest request, ICacheable cacheable, RequestHandlerDelegate<TResponse> next)
        {
            var cacheKey = request.GetType().Name + DateTime.Now.ToString();

            var isExist = _cache.TryGetValue(cacheKey, out TResponse response);
            if (isExist)
            {
                return response;
            }

            response = await next();
            if (response == null)
            {
                return default;
            }

            _cache.Set(cacheKey, response, cacheable.CacheSettings.Value);

            return response;
        }

        private async Task<TResponse> GetFromDistributedCache(TRequest request, ICacheable cacheable, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            throw new NotImplementedException();
        }

        private async Task<TResponse> GetFromMultiCache(TRequest request, ICacheable cacheable, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            throw new NotImplementedException();
        }
    }
}
