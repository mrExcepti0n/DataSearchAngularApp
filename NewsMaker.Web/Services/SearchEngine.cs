﻿using Domain.Core.Model;
using Microsoft.Extensions.Logging;
using Nest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsMaker.Web.Services
{
    public class SearchEngine
    {
        readonly IElasticClient _client;
        private readonly ILogger _logger;

        public SearchEngine(ILogger<SearchEngine> logger, IElasticClient elasticClient)
        {
            _client = elasticClient;

            _logger = logger;
        }

        public async Task Remove<T>(int newsId) where T: class
        {
            var  res = await _client.DeleteAsync<T>(newsId);
        }

        public async Task Update<T>(T entity) where T: class
        {
            var result = await _client.UpdateAsync<T, T>(new DocumentPath<T>(entity), u =>
                      u.Doc(entity));
        }

        public async Task Add(News news)
        {
            var result = await _client.IndexDocumentAsync(news);

            if (!result.IsValid)
            {
                _logger.LogCritical(result.Result.ToString());
            }
        }

        public async Task<IReadOnlyCollection<News>> SearchAsync(string pattern)
        {
            var searchResponse = await _client.SearchAsync<News>(s => s.From(0).Size(10)
                .Query(q => q.Match(m => m.Field(f => f.Content).Query(pattern))));
            return searchResponse.Documents;
        }
    }
}
