﻿using Destiny.Core.Flow.Dependency;
using Destiny.Core.Flow.Dtos;
using Destiny.Core.Flow.Dtos.Menu;
using Destiny.Core.Flow.Entity;
using Destiny.Core.Flow.EntityFrameworkCore;
using Destiny.Core.Flow.Enums;
using Destiny.Core.Flow.ExpressionUtil;
using Destiny.Core.Flow.Extensions;
using Destiny.Core.Flow.Filter;
using Destiny.Core.Flow.Filter.Abstract;
using Destiny.Core.Flow.IServices.IMenu;
using Destiny.Core.Flow.Model.Entities.Identity;
using Destiny.Core.Flow.Model.Entities.Menu;
using Destiny.Core.Flow.Model.Entities.Rolemenu;
using Destiny.Core.Flow.Repository.MenuRepository;
using Destiny.Core.Flow.Ui;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Destiny.Core.Flow.Services.Menu
{
    [Dependency(ServiceLifetime.Scoped)]
    public class MenuServices : IMenuServices
    {
        private readonly IMenuRepository _menuRepository = null;
        private readonly IEFCoreRepository<RoleMenuEntity, Guid> _roleMenuRepository;
        private readonly IMenuFunctionRepository _menuFunction=null;
        private readonly IUnitOfWork _unitOfWork = null;
        private readonly IIdentity _iIdentity = null;
        private readonly IEFCoreRepository<UserRole, Guid> _repositoryUserRole = null;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        public MenuServices(IMenuRepository menuRepository, IUnitOfWork unitOfWork, IEFCoreRepository<RoleMenuEntity, Guid> roleMenuRepository, IMenuFunctionRepository menuFunction, IPrincipal principal, UserManager<User> userManager, RoleManager<Role>  roleManager, IEFCoreRepository<UserRole, Guid> repositoryUserRole)
        {
            _menuRepository = menuRepository;
            _roleMenuRepository = roleMenuRepository;
            this._menuFunction = menuFunction;
            _unitOfWork = unitOfWork;
            _iIdentity = principal.Identity;
            _userManager = userManager;
            _roleManager = roleManager;
            _repositoryUserRole=repositoryUserRole;
        }

        public async Task<OperationResponse> CreateAsync(MenuInputDto input)
        {
            input.NotNull(nameof(input));
            return await _unitOfWork.UseTranAsync(async () =>
            {
                var result = await _menuRepository.InsertAsync(input);
                if(input.FunctionId?.Any()==true)
                {
                    int count= await _menuFunction.InsertAsync(input.FunctionId.Select(x => new MenuFunction
                    {
                        MenuId = input.Id,
                        FunctionId = x
                    }).ToArray());
                }
                return new OperationResponse("保存成功", OperationResponseType.Success);
            });
        }

        public async Task<OperationResponse> DeleteAsync(Guid id)
        {
            return await _menuRepository.DeleteAsync(id);
        }

        public async Task<OperationResponse> UpdateAsync(MenuInputDto input)
        {
            input.NotNull(nameof(input));
            return await _unitOfWork.UseTranAsync(async () =>
            {
                var result = await _menuRepository.UpdateAsync(input);
                await _menuFunction.DeleteBatchAsync(x => x.MenuId == input.Id);
                if (input.FunctionId?.Any() == true)
                {
                    int count = await _menuFunction.InsertAsync(input.FunctionId.Select(x => new MenuFunction
                    {
                        MenuId = input.Id,
                        FunctionId = x
                    }).ToArray());
                }
                return new OperationResponse("保存成功", OperationResponseType.Success);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleid">角色Id</param>
        /// <returns></returns>
        public async Task<OperationResponse<SelectedItem<MenuTreeOutDto, Guid>>> GetMenuTreeAsync(Guid? roleId)
        {
           
            var rolelist = new List<RoleMenuEntity>();
            var list = await _menuRepository.Entities.ToTreeResultAsync<MenuEntity, MenuTreeOutDto>(
                (r, c) =>
                {
                    return c.ParentId == null || c.ParentId == Guid.Empty;
                }, (p, q) =>
                {
                    return p.Id == q.ParentId;
                }, (p, q) =>
                {
                    if (p.children == null)
                    {
                        p.children = new List<MenuTreeOutDto>();
                    }

                    p.children.AddRange(q);
                });
            SelectedItem<MenuTreeOutDto, Guid> selectedItem = new SelectedItem<MenuTreeOutDto, Guid>();
            selectedItem.ItemList = list.ItemList.ToList();
            selectedItem.Selected = new List<Guid>();
            if (roleId.HasValue)
            {
                selectedItem.Selected= await _roleMenuRepository.Entities.Where(o => o.RoleId == roleId.Value).Select(o => o.MenuId).ToListAsync();
            }
            OperationResponse<SelectedItem<MenuTreeOutDto, Guid>> operationResponse = new OperationResponse<SelectedItem<MenuTreeOutDto, Guid>>();
            operationResponse.Type = OperationResponseType.Success;
            operationResponse.Data= selectedItem;

            return operationResponse;
        }

        //public async Task<OperationResponse> GetTreeSelectTreeDataAsync()
        //{
        //    OperationResponse response = new OperationResponse();
        //    var list=  await _menuRepository.Entities.ToListAsync();
        //    var permissionTreeItems = await GetMenuTreeAsync();


        //    response.IsSuccess("查询成功", new
        //    {
        //        TreeItemData = list,
        //        SelectTreeItems = permissionTreeItems
        //    });
        //    return response;
        //}
        /// <summary>
        /// 根据ID获取一个菜单
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<OperationResponse<MenuOutputLoadDto>> LoadFormMenuAsync(Guid Id)
        {
            var menu = await _menuRepository.GetByIdAsync(Id);
            var menudto = menu.MapTo<MenuOutputLoadDto>();
            menudto.FunctionIds = (await _menuFunction.Entities.Where(x => x.MenuId == Id && x.IsDeleted == false).ToListAsync()).Select(x => x.FunctionId).ToArray();
            return new OperationResponse<MenuOutputLoadDto>(MessageDefinitionType.LoadSucces, menudto,OperationResponseType.Success);
        }
      
        public async Task<TreeResult<MenuTableOutDto>> GetMenuTableAsync()
        {
            return await _menuRepository.Entities.ToTreeResultAsync<MenuEntity, MenuTableOutDto>(
                (p, c) =>
                {
                    return c.ParentId == null || c.ParentId == Guid.Empty;
                },
                (p, c) =>
                {
                    return p.Id == c.ParentId;
                },
                (p, children) =>
                {
                    if (p.children == null)
                        p.children = new List<MenuTableOutDto>();
                    p.children.AddRange(children);
                }
                );
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IPagedResult<MenuPermissionsOutDto>> GetMenuAsync()
        {
            var menulist = new List<MenuPermissionsOutDto>();
            var userId= _iIdentity.GetUesrId<Guid>();
            var usermodel= await  _userManager.FindByIdAsync(userId.ToString());
            var roleids = (await _repositoryUserRole.Entities.Where(x => x.UserId == userId).ToListAsync()).Select(x => x.RoleId);
            var menuId = (await _roleMenuRepository.Entities.Where(x => roleids.Contains(x.RoleId)).ToListAsync()).Select(x => x.MenuId);
            if (usermodel.IsSystem && _roleManager.Roles.Where(x=>x.IsAdmin==true && roleids.Contains(x.Id)).Any())
            {
                menulist = await _menuRepository.Entities.Select(x => new MenuPermissionsOutDto
                {
                    Name = x.Name,
                    RouterPath = x.Path,
                    Id = x.Id,
                    Sort = x.Sort,
                }).ToListAsync();
                return new PageResult<MenuPermissionsOutDto>()
                {
                    ItemList = menulist,
                    Total = menulist.Count,
                };
            }
            menulist= await _menuRepository.Entities.Where(x => menuId.Contains(x.Id)).Select(x => new MenuPermissionsOutDto
            {
                Name = x.Name,
                RouterPath = x.Path,
                Id = x.Id,
                Sort = x.Sort,
            }).ToListAsync();
            return new PageResult<MenuPermissionsOutDto>()
            {
                ItemList = menulist,
                Total = menulist.Count,
            };
        }
    }
}