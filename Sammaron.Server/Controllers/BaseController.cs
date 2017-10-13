using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Sammaron.Core.Data;
using Sammaron.Core.Interfaces;

namespace Sammaron.Server.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    public class BaseController<TEntity, TKey> : Controller where TEntity : class, IEntity<TKey>
    {
        public BaseController(ApiContext context)
        {
            Context = context;
            Entities = context.Set<TEntity>();
        }

        public ApiContext Context { get; set; }
        public IQueryable<TEntity> Entities { get; set; }
        public Expression<Func<TEntity, TEntity>> DetailsSelector { get; set; }
        public Expression<Func<TEntity, TEntity>> LightSelector { get; set; }

        // GET api/[controller]
        [HttpGet]
        public IEnumerable<TEntity> Get()
        {
            return Entities.Select(LightSelector).ToList();
        }

        // GET api/[controller]/id
        [HttpGet]
        [Route("{id}")]
        public IEnumerable<TEntity> Get(TKey id)
        {
            return Entities.Where(e => e.Id.Equals(id)).Select(LightSelector).ToList();
        }
    }
}