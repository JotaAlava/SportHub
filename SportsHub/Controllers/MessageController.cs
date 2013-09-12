﻿using System;
using System.Web.Mvc;
using SportsHub.Infrastructure;
using SportsHub.Models;

namespace SportsHub.Controllers
{
    public class MessageController : Controller
    {
        private PlayerDb _playerDb = new PlayerDb();
        private EventDb _eventDb = new EventDb();
        private MessageDb _messageDb = new MessageDb();

        public ActionResult PostMessage(int eventId, string message)
        {
            string error = string.Empty;

            if (ModelState.IsValid)
            {
                Event ev = _eventDb.GetEventById(eventId);
                Player author = _playerDb.GetPlayerByUsername(User.Identity.Name);
                Message newMessage = new Message
                {
                    Author = author,
                    Event = ev,
                    MessageText = message,
                    Time = DateTime.Now
                };

                error = _messageDb.AddMessage(newMessage);
            }

            return RedirectToAction("Index", "Event", new {errorMessage = error});
        }

    }
}