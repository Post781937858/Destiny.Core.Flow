﻿using Destiny.Core.Flow.Dependency;
using Destiny.Core.Flow.IServices.IRoleMenu;
using Destiny.Core.Flow.Model.Entities.Rolemenu;
using Destiny.Core.Flow.Repository.RoleMenuRepository;
using Destiny.Core.Flow.Ui;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Destiny.Core.Flow.Services.RoleMenu
{
    [Dependency(ServiceLifetime.Scoped)]
    public class RoleMenuServices : IRoleMenuServices
    {
        private readonly IRoleMenuRepository _roleMenuRepository = null;

        public RoleMenuServices(IRoleMenuRepository roleMenuRepository)
        {
            _roleMenuRepository = roleMenuRepository;
        }

        public async Task<OperationResponse> CareateAsync()
        {
            var response = await _roleMenuRepository.InsertAsync(new RoleMenuEntity());

            return new OperationResponse()
            {
                Data = response
            };
        }

        public async Task<OperationResponse> DeleteAsync()
        {
            var response = await _roleMenuRepository.DeleteAsync(new Guid());
            return response;
        }
    }
}