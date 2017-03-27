﻿using System;
using System.Data;
using System.Data.Entity;

using Autofac.Extras.IocManager;

using Stove.Transactions;

namespace Stove.EntityFramework
{
    public class EfActiveTransactionProvider : IActiveTransactionProvider, ITransientDependency
    {
        private readonly IScopeResolver _scopeResolver;

        public EfActiveTransactionProvider(IScopeResolver scopeResolver)
        {
            _scopeResolver = scopeResolver;
        }

        public IDbTransaction GetActiveTransaction(ActiveTransactionProviderArgs args)
        {
            return GetDbContext(args).Database.CurrentTransaction.UnderlyingTransaction;
        }

        public IDbConnection GetActiveConnection(ActiveTransactionProviderArgs args)
        {
            return GetDbContext(args).Database.Connection;
        }

        private DbContext GetDbContext(ActiveTransactionProviderArgs args)
        {
            var dbContextType = (Type)args["ContextType"];
            Type dbContextProviderType = typeof(IDbContextProvider<>).MakeGenericType(dbContextType);
            object dbContextProvider = _scopeResolver.Resolve(dbContextProviderType);

            Type func = typeof(Func<>).MakeGenericType(dbContextType);
            var getDbContextFunc = (Func<DbContext>)Delegate.CreateDelegate(func, dbContextProvider, "GetDbContext");

            return getDbContextFunc();
        }
    }
}
