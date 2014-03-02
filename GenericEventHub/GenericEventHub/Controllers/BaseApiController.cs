﻿using GenericEventHub.Models;
using GenericEventHub.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace GenericEventHub.Controllers
{
    public class BaseApiController<TEntity> : ApiController where TEntity : Entity
    {
        private IBaseService<TEntity> _service;

        public BaseApiController(IBaseService<TEntity> service)
        {
            _service = service;
        }

        public HttpResponseMessage Get()
        {
            var serviceResponse = _service.GetAll();

            HttpResponseMessage controllerResponse = null;
            if (serviceResponse.Success)
                controllerResponse = Request.CreateResponse(HttpStatusCode.OK, serviceResponse.Data);
            else
                controllerResponse = Request.CreateResponse(HttpStatusCode.InternalServerError, serviceResponse.Message);

            return controllerResponse;

        }

        [Route("{id:int}")]
        public TEntity Get(int id)
        {
            var TEntity = _service.GetByID(id).Data;
            if (TEntity == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return TEntity;
        }

        public HttpResponseMessage Put(int id, TEntity TEntity)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != TEntity.GetID())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var response = _service.Update(TEntity);

            if (!response.Success)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, response.Message);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage Post(TEntity TEntity)
        {
            if (ModelState.IsValid)
            {
                var res = _service.Create(TEntity);

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, TEntity);
                //response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = TEntity.TEntityID }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        public HttpResponseMessage Delete(int id)
        {
            var TEntity = _service.GetByID(id).Data;
            if (TEntity == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var response = _service.Delete(TEntity);


            if (!response.Success)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, response.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, TEntity);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _service.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}