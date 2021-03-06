﻿
using AutoMapper;
using Destiny.Core.Flow.Entity;
using Destiny.Core.Flow.Enums;
using Destiny.Core.Flow.Mapping;
using Destiny.Core.Flow.Model.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Destiny.Core.Flow.Dtos
{

    [AutoMapp(typeof(User))]
    public class UserInputDto : InputDto<Guid>
    {
        public UserInputDto()
        {

        }
        /// <summary>
        /// 姓名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string PasswordHash { get; set; }
        public bool IsSystem { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 姓别
        /// </summary>
        public Sex Sex { get; set; }

        public Guid[] RoleIds { get; set; }

    }
    [AutoMapp(typeof(User))]
    public class UserUpdateInputDto : InputDto<Guid>
    {
        public UserUpdateInputDto()
        {

        }
        /// <summary>
        /// 姓名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        public bool IsSystem { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 姓别
        /// </summary>
        public Sex Sex { get; set; }

        public Guid[] RoleIds { get; set; }

    }
}
