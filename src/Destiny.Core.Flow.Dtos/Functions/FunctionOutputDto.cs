﻿using Destiny.Core.Flow.Entity;
using Destiny.Core.Flow.Mapping;
using Destiny.Core.Flow.Model.Entities.Function;
using System;
using System.Collections.Generic;
using System.Text;

namespace Destiny.Core.Flow.Dtos.Functions
{
    [AutoMapp(typeof(Function))]
    public class FunctionOutputDto : FunctionDtoBase, IOutputDto<Guid>
    {
        public Guid Id { get; set; }
    }
}
