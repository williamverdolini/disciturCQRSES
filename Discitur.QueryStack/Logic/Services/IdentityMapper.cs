using Discitur.QueryStack.Model;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Discitur.QueryStack.Logic.Services
{
    // Based on http://coding-insomnia.com/2012/05/28/a-trip-to-cqrs-commands/
    public class IdentityMapper : IIdentityMapper
    {
        private ConcurrentDictionary<Guid, Tuple<string, int>> _modelCache = new ConcurrentDictionary<Guid, Tuple<string, int>>();
        private ConcurrentDictionary<Tuple<string, int>, Guid> _aggregateCache = new ConcurrentDictionary<Tuple<string, int>, Guid>();

        /// <summary>
        /// Create a mapping between AggregateRoot's id (Guid) and Read-Model entity's id (int)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="modelId"></param>
        /// <param name="aggregateId"></param>
        public void Map<TEntity>(int modelId, Guid aggregateId)
        {
            using (var db = new DisciturContext())
            {
                var map = db.IdMap.FirstOrDefault(im => im.AggregateId == aggregateId) ??
                          new IdentityMap() { AggregateId = aggregateId };

                var typeName = typeof(TEntity).Name;
                map.TypeName = typeName;
                map.ModelId = modelId;

                db.IdMap.Add(map);
                SaveToCache(modelId, aggregateId, typeName);

                db.SaveChanges();
            }
        }


        private void SaveToCache(int modelId, Guid aggregateId, string typeName)
        {
            _modelCache[aggregateId] = new Tuple<string, int>(typeName, modelId);
            _aggregateCache[new Tuple<string, int>(typeName, modelId)] = aggregateId;
        }

        /// <summary>
        /// Gets the AggregateRoot'Id (Guid) linked to Read-Model entity's Id (int)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="modelId"></param>
        /// <returns></returns>
        public Guid GetAggregateId<TEntity>(int modelId)
        {
            using (var db = new DisciturContext())
            {
                var typeName = typeof(TEntity).Name;
                Guid aggregateId;
                if (_aggregateCache.TryGetValue(new Tuple<string, int>(typeName, modelId), out aggregateId))
                    return aggregateId;

                var map = db.IdMap.FirstOrDefault(im => im.ModelId == modelId && im.TypeName == typeName);
                if (map != null)
                {
                    SaveToCache(modelId, map.AggregateId, typeName);
                    return map.AggregateId;
                }

                return Guid.Empty;
            }
        }

        /// <summary>
        /// Gets the Read-Model entity's Id (int) linked to AggregateRoot'Id (Guid)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="aggregateId"></param>
        /// <returns></returns>
        public int GetModelId<TEntity>(Guid aggregateId)
        {
            using (var db = new DisciturContext())
            {
                var typeName = typeof(TEntity).Name;
                Tuple<string, int> model;
                if (_modelCache.TryGetValue(aggregateId, out model) && model.Item1 == typeName)
                    return model.Item2;

                var map = db.IdMap.FirstOrDefault(im => im.AggregateId == aggregateId && im.TypeName == typeName);
                if (map != null)
                {
                    SaveToCache(map.ModelId, aggregateId, typeName);
                    return map.ModelId;
                }

                return 0;
            }
        }

    }

}
