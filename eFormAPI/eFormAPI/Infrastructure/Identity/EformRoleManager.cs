﻿using eFormAPI.Web.Infrastructure.Data;
using eFormAPI.Web.Infrastructure.Data.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace eFormAPI.Web.Infrastructure.Identity
{
    public class EformRoleManager : RoleManager<EformRole, int>
    {
        public EformRoleManager(IRoleStore<EformRole, int> roleStore)
            : base(roleStore) {}

        public static EformRoleManager Create(IdentityFactoryOptions<EformRoleManager> options, IOwinContext context)
        {
            var appRoleManager = new EformRoleManager(new EformRoleStore(context.Get<BaseDbContext>()));

            return appRoleManager;
        }
    }
}