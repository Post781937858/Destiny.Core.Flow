﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Destiny.Core.Flow.AspNetCore.Api;
using Destiny.Core.Flow.AspNetCore.Ui;
using Destiny.Core.Flow.IServices.Permission;
using Destiny.Core.Flow.API.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Destiny.Core.Flow.API.Controllers
{
    /// <summary>
    /// 权限管理
    /// </summary>
    [Description("权限管理")]
    [Authorize]
    public class PermissionController :ApiControllerBase
    {

        private IPermissionService _permissionService = null;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }


        /// <summary>
        /// 异步得到权限集合
        /// </summary>
        [Description("异步得到权限集合")]
        [HttpGet]
        public async Task<AjaxResult> GetPermissionListAsync()
        {
           return (await _permissionService.GetRolePermissionListAsync()).ToAjaxResult();
        }
    }
}
