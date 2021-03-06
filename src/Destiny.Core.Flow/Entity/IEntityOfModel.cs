﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Destiny.Core.Flow.Entity
{
    /// <summary>
    /// 实体接口
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntity<out TKey>: IEntity
    {
        [Description("主键")]
        TKey Id { get;}
    }



}
