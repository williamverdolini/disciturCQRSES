﻿using System;

namespace Discitur.QueryStack.Logic.Services
{
    public interface IIdentityMapper
    {
        void Map<TEntity>(int modelId, Guid aggregateId);
        Guid GetAggregateId<TEntity>(int modelId);
        int GetModelId<TEntity>(Guid aggregateId);

    }
}
