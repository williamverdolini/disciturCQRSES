﻿using Discitur.Infrastructure;
using Discitur.Legacy.Migration.Infrastructure;
using Discitur.QueryStack;
using Discitur.QueryStack.Logic.Services;
using Discitur.QueryStack.Model;
using NEventStore;
using System;
using System.Linq;

namespace Discitur.Legacy.Migration.Model
{
    public class UserMigration : IMigrationStep, IUserMigration
    {
        private readonly IDatabase _db;
        private readonly IStoreEvents _store;
        private readonly IImageConverter _imageConverter;

        public UserMigration(IDatabase database, IStoreEvents storeEvent, IImageConverter imageConverter)
        {
            Contract.Requires<ArgumentNullException>(database != null, "database");
            Contract.Requires<ArgumentNullException>(storeEvent != null, "storeEvent");
            Contract.Requires<ArgumentNullException>(imageConverter != null, "imageConverter");
            _db = database;
            _store = storeEvent;
            _imageConverter = imageConverter;
        }

        public void Execute()
        {
            // There's NO UserActivation migration strategy (to avoid complexity in this phase).
            // Before User migration begins, it checks for this assumption
            CanUserMigrationBegin(
                ExecuteUserMigration
                );
        }

        private void CanUserMigrationBegin(Action yesAction)
        {
            var pendingUserActivations = _db.UserActivations.Count();
            if (pendingUserActivations.Equals(0))
                yesAction();
            else
            {
                string message = pendingUserActivations > 1 ?
                    "User Migration Not started: there are {0} pending User activations. Please wait till these accounts will be activated." :
                    "User Migration Not started: there is {0} pending User activation. Please wait till this account will be activated.";
                Console.WriteLine(message, pendingUserActivations);
            }
        }

        private void ExecuteUserMigration()
        {
            foreach (var user in _db.Users)
            {
                if (_db.IdMaps.GetAggregateId<User>(user.UserId).Equals(Guid.Empty))
                {
                    // Get fresh new ID
                    Guid entityId = Guid.NewGuid();
                    while (!_db.IdMaps.GetModelId<User>(entityId).Equals(0))
                        entityId = Guid.NewGuid();

                    //Check for User Activation
                    //bool isActivated = !_db.UserActivations.Any(a => a.UserName.Equals(user.UserName));
                    // Migration starts only if no pending user activations exists (all users are already activated)
                    bool isActivated = true;
                    // Create Memento from ReadModel
                    var entity = new Discitur.Domain.Model.UserMemento(entityId, 1, user.UserName, user.Surname, user.Email, user.UserName, _imageConverter.ToPictureBytes(user.Picture), isActivated);

                    // Create a fake External event
                    using (var stream = _store.OpenStream(entityId, 0, int.MaxValue))
                    {
                        stream.Add(new EventMessage { Body = entity });
                        stream.CommitChanges(Guid.NewGuid());
                    }

                    // Save Snapshot from entity's Memento image
                    _store.Advanced.AddSnapshot(new Snapshot(entity.Id.ToString(), entity.Version, entity));

                    // Save Ids mapping
                    _db.IdMaps.Map<User>(user.UserId, entityId);

                    System.Diagnostics.Debug.WriteLine("Successfully migrated User id: {0}, Guid: {1}, UserName:{2}", new object[] { user.UserId, entityId.ToString(), user.UserName });
                }
            }
        }

    }

}