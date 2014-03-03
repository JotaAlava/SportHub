﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GenericEventHub.Models;
using GenericEventHub.Services;

namespace GenericEventHub.Controllers
{
    [Authorize]
    [RoutePrefix("Events")]
    public class EventsController : BaseApiController<Event>
    {
        private IEventService _service;
        private IUserService _userService;
        private IGuestService _guestService;

        public EventsController(IEventService service,
            IUserService userService,
            IGuestService guestService) : base(service)
        {
            _service = service;
            _userService = userService;
            _guestService = guestService;
        }

        [HttpPost]
        [Route("{eventID:int}/AddUser")]
        public HttpResponseMessage AddUser(int eventID)
        {
            var user = _userService.GetUserByWindowsName(User.Identity.Name).Data;

            if (user == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var ev = _service.GetByID(eventID).Data;

            if (ev != null && !ev.UsersInEvent.Contains(user))
            {
                ev.UsersInEvent.Add(user); 
                _service.Update(ev);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("{eventID:int}/RemoveUser")]
        public HttpResponseMessage RemoveUser(int eventID)
        {
            var user = _userService.GetUserByWindowsName(User.Identity.Name).Data;

            if (user == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            // Check for null event
            var ev = _service.GetByID(eventID).Data;

            if (ev != null && ev.UsersInEvent.Contains(user))
            {
                ev.UsersInEvent.Remove(user);
                _service.Update(ev);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("{eventID:int}/AddGuest")]
        public HttpResponseMessage AddGuest(int eventID, Guest guest)
        {
            var host = _userService.GetUserByWindowsName(User.Identity.Name).Data;

            if (host == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            guest.HostID = host.UserID;
            _guestService.Create(guest);
            var ev = _service.GetByID(eventID).Data;

            guest.Event = ev;
            guest.Host = host;
            if (ev != null)
            {
                ev.GuestsInEvent.Add(guest);
                _service.Update(ev);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("{eventID:int}/RemoveGuest/{guestID:int}")]
        public HttpResponseMessage RemoveGuest(int eventID, int guestID)
        {
            var guest = _guestService.GetByID(guestID).Data;

            var user = guest.Host;
            if (user != null && !user.WindowsName.Equals(user.WindowsName))
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var ev = _service.GetByID(eventID).Data;

            if (ev != null && ev.GuestsInEvent.Contains(guest))
            {
                ev.GuestsInEvent.Remove(guest);
                _service.Update(ev);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}