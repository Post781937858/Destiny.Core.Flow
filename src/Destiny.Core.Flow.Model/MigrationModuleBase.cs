﻿using Destiny.Core.Flow.Entity;
using Destiny.Core.Flow.EntityFrameworkCore;
using Destiny.Core.Flow.Extensions;
using Destiny.Core.Flow.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Destiny.Core.Flow.Model
{
    public abstract  class MigrationModuleBase : AppModuleBase
    {

        public override void Configure(IApplicationBuilder applicationBuilder)
        {
           var configuration=  applicationBuilder.ApplicationServices.GetService<IConfiguration>();
           var isAutoMigration=  configuration["Destiny:Migrations:IsAutoMigration"].AsTo<bool>();
            if(isAutoMigration)
            {
                applicationBuilder.ApplicationServices.CreateScoped(provider => {
                    var unitOfWork = provider.GetService<IUnitOfWork>();
                    var dbContext = unitOfWork.GetDbContext();
                    string[] migrations = dbContext.Database.GetPendingMigrations().ToArray();
                    if (migrations.Length > 0)
                    {
                        dbContext.Database.Migrate();

                    }

                });

            }

            var isAddSeedData = configuration["Destiny:Migrations:IsAddSeedData"].AsTo<bool>();
            if (isAddSeedData)
            {
                var seedDatas = applicationBuilder.ApplicationServices.GetServices<ISeedData>();

                foreach (var seed in seedDatas?.OrderBy(o => o.Order).Where(o => !o.Disable))
                {
                    seed.Initialize();
                }
            }
         

        }
    }
}
