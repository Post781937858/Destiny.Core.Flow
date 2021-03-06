﻿using Destiny.Core.Flow.Dependency;
using Destiny.Core.Flow.Dtos.RoleDtos;
using Destiny.Core.Flow.EntityFrameworkCore;
using Destiny.Core.Flow.Enums;
using Destiny.Core.Flow.ExpressionUtil;
using Destiny.Core.Flow.Extensions;
using Destiny.Core.Flow.Filter;
using Destiny.Core.Flow.Filter.Abstract;
using Destiny.Core.Flow.IServices.IRoleServices;
using Destiny.Core.Flow.Model.Entities.Identity;
using Destiny.Core.Flow.Model.Entities.Rolemenu;
using Destiny.Core.Flow.Ui;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Destiny.Core.Flow.Services.RoleServices
{
    [Dependency(ServiceLifetime.Scoped)]
    public class RoleManagerServices: IRoleManagerServices
    {
        private readonly RoleManager<Role> _roleManager=null;
        private readonly IEFCoreRepository<RoleMenuEntity, Guid> _roleMenuRepository;
        /// <summary>
        /// 构造函数注入
        /// </summary>
        /// <param name="roleManager"></param>
        public RoleManagerServices(RoleManager<Role> roleManager, IEFCoreRepository<RoleMenuEntity, Guid> roleMenuRepository)
        {
            _roleManager = roleManager;
            _roleMenuRepository = roleMenuRepository;
        }
        /// <summary>
        /// 异步添加角色
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<OperationResponse> AddRoleAsync(RoleInputDto dto)
        {
            dto.NotNull(nameof(dto));
            var role = dto.MapTo<Role>();
            return await _roleMenuRepository.UnitOfWork.UseTranAsync(async () =>
            {
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    return result.ToOperationResponse();
                }
                if (dto.MenuIds?.Any() == true) {
                    var list = dto.MenuIds.Select(x => new RoleMenuEntity
                    {
                        MenuId = x,
                        RoleId = role.Id,
                    }).ToArray();
                    int count = await _roleMenuRepository.InsertAsync(list);
                    if (count <= 0)
                    {
                        return new OperationResponse("保存失败", OperationResponseType.Error);
                    }
                }
                return new OperationResponse("保存成功", OperationResponseType.Success);

            });
        }
        public async Task<OperationResponse> DeleteAsync(Guid id)
        {
            id.NotNull(nameof(id));
            var role =await _roleManager.FindByIdAsync(id.ToString());
            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return result.ToOperationResponse();
            }
            return new OperationResponse("删除成功!!", OperationResponseType.Success);

        }
        public async Task<OperationResponse> UpdateRoleAsync(RoleInputDto dto)
        {
            dto.NotNull(nameof(dto));
            var role = await _roleManager.FindByIdAsync(dto.Id.ToString());
            role = dto.MapTo(role);//拿到所有角色的权限
            return await _roleMenuRepository.UnitOfWork.UseTranAsync(async () =>
            {
                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                    return result.ToOperationResponse();
                if (dto.MenuIds?.Any() == true)
                {
                    var list = dto.MenuIds.Select(x => new RoleMenuEntity
                    {
                        MenuId = x,
                        RoleId = role.Id,
                    }).ToArray();
                    int count = await _roleMenuRepository.DeleteBatchAsync(x => x.RoleId == role.Id);
                    int insertcount = await _roleMenuRepository.InsertAsync(list);
                    if(count<=0 && insertcount<=0)
                        return new OperationResponse("保存失败", OperationResponseType.Error);
                }   
                return new OperationResponse("保存成功", OperationResponseType.Success);
            });
        }
        /// <summary>
        /// 分页查询角色
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IPagedResult<RoleOutputPageListDto>> GetRolePageAsync(PageRequest request)
        {
            request.NotNull(nameof(request));
         
            request.OrderConditions = new OrderCondition<Role>[] { new OrderCondition<Role>(o => o.CreatedTime, SortDirection.Descending) };
            return await _roleManager.Roles.AsNoTracking().ToPageAsync<Role, RoleOutputPageListDto>(request);
        }
        /// <summary>
        /// 得到角色把角色转成下拉
        /// </summary>
        /// <returns></returns>
        public async Task<OperationResponse<IEnumerable<SelectListItem>>> GetRolesToSelectListItemAsync()
        {
            var roles = await _roleManager.Roles.AsNoTracking().Select(r => new SelectListItem
            {
                Value = r.Id.ToString().ToLower(),
                Text = r.Name,
                Selected = false,

            }).ToListAsync();
            return new OperationResponse<IEnumerable<SelectListItem>>("得到数据成功",roles,OperationResponseType.Success);
        }
    }
}
