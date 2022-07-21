// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Database
{
    public abstract class RepositoryBase
    {
        public async Task<long> CountAsync<T>(IMongoCollection<T> collection, Expression<Func<T, bool>>? filterFunction)
            => await collection.CountDocumentsAsync(filterFunction ?? Builders<T>.Filter.Empty);

        /// <summary>
        /// Get All T that match filters provided.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">Collection to run against.</param>
        /// <param name="filterFunction">Filter function you can filter on properties of T.</param>
        /// <param name="sortFunction">Function used to sort data.</param>
        /// <param name="skip">Items to skip.</param>
        /// <param name="limit">Items to limit results by.</param>
        /// <returns></returns>
        public async Task<IList<T>> GetAllAsync<T>(IMongoCollection<T> collection, Expression<Func<T, bool>>? filterFunction, SortDefinition<T> sortFunction, int? skip = null, int? limit = null)
        {
            return await collection
                .Find(filterFunction ?? Builders<T>.Filter.Empty)
                .Skip(skip)
                .Limit(limit)
                .Sort(sortFunction)
                .ToListAsync();
        }

        public async Task<IList<T>> GetAllAsync<T>(IMongoCollection<T> collection, FilterDefinition<T> filterFunction, SortDefinition<T> sortFunction, int? skip = null, int? limit = null)
        {
            return await collection
                .Find(filterFunction)
                .Skip(skip)
                .Limit(limit)
                .Sort(sortFunction)
                .ToListAsync();
        }
    }
}
