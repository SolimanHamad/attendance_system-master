using AttendanceSystem.Models;
using AttendanceSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Repositories;
using AttendanceSystem.Models.Enums;

namespace AttendanceSystem.Controllers
{
    [Authorize]
    public class RequestController : Controller
    {
        private readonly UserEventRepository userEventRepository;
        private readonly EditEventRequestRepository editEventRequestRepository;
        private readonly MissedEventRequestRepository missedEventRequestRepository;

        public RequestController(AttendanceSystemContext context)
        {
            userEventRepository = new UserEventRepository(context);
            editEventRequestRepository = new EditEventRequestRepository(context);
            missedEventRequestRepository = new MissedEventRequestRepository(context);
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(string eventID)
        {
            UserEvent originalUserEvent = await userEventRepository.GetEventByID(eventID);
            EditEventViewModel model = new EditEventViewModel
            {
                EventID = eventID,
                WorkingDayID = originalUserEvent.WorkingDayID,
                Time = originalUserEvent.Time,
                Event = originalUserEvent.Event
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(EditEventViewModel model)
        {
            if (ModelState.IsValid)
            {
                DateTime eventDate = (await userEventRepository.GetEventByID(model.EventID)).Time;
                DateTime newTime = model.Time;
                DateTime newDateTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, newTime.Hour, newTime.Minute, newTime.Second);
                await editEventRequestRepository.Add(new EditEventRequest
                {
                    NewTime = newDateTime,
                    Comment = model.Comment,
                    UserEventID = model.EventID
                });
                TempData["StatusMessage"] = "Edit request has been sent to the admin.";
                return RedirectToAction("Index", "Home");
            }
            TempData["StatusMessage"] = "Error: Some fields are invalid!";
            return View(model);
        }

        public IActionResult MissedEventRequest()
        {
            MissedEventRequestViewModel missedEventRequestViewModel = new MissedEventRequestViewModel
            {
                UserID = User.FindFirst(ClaimTypes.NameIdentifier).Value
            };

            return View(missedEventRequestViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteRequestedMissingEvent(string eventID)
        {
            MissedEventRequest request = await missedEventRequestRepository.GetMissedEventRequestByEventID(eventID);

            if (request == null)
                return NotFound();
            if (request.Approval != Approval.Pending) // can't delete if request is already approved or rejected
                return Forbid();

            await missedEventRequestRepository.RemoveMissedEventRequest(eventID);
            TempData["StatusMessage"] = "Request has been deleted successfully.";
            return RedirectToAction("MissedEventRequests");
        }

        [HttpPost]
        public async Task<IActionResult> MissedEventRequest(MissedEventRequestViewModel model)
        {
            DateTime date = model.Time; 
            if (ModelState.IsValid && date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Friday)
            {
                if (await IsNewRequestConflicting(date , model.UserID, model.Event))
                {
                    TempData["StatusMessage"] = "already requested edit event at this date"; 
                }
                else
                {  
                    await missedEventRequestRepository.Add(new MissedEventRequest
                    { 
                        UserID = model.UserID,
                        Time = model.Time,
                        Event = model.Event,
                        Comment = model.Comment
                    });
                    TempData["StatusMessage"] = "Your request has been sent to the admin.";
                    return RedirectToAction("MissedEventRequests");
                }
            }
            TempData["StatusMessage"] = "Error: Some fields are invalid!";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditEventRequests()
        {
            string userID = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return View(await editEventRequestRepository.GetEditEventRequestsByUserID(userID));
        }

        [HttpGet]
        public async Task<IActionResult> MissedEventRequests()
        {
            string userID = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return View(await missedEventRequestRepository.GetMissedEventRequestsByUserID(userID));
        }

        [HttpGet]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> MissedEventRequestsApproval()
        {
            return View(await missedEventRequestRepository.GetPendingMissedEventRequests());
        }

        [HttpPost]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> MissedEventRequestsApproval(List<MissedEventRequest> requestsList)
        {
            if (ModelState.IsValid)
            {
                await missedEventRequestRepository.UpdateMissedEventRequestList(requestsList);
                TempData["StatusMessage"] = "Changes have been saved successfully.";
                return await MissedEventRequestsApproval();
            }
            TempData["StatusMessage"] = "Error: Invalid submission!";
            return View(requestsList);
        }

        [HttpGet]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> EditEventRequestsApproval()
        {
            return View(await editEventRequestRepository.GetPendingEditEventRequests());
        }

        [HttpPost]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> EditEventRequestsApproval(List<EditEventRequest> requestsList)
        {
            if (ModelState.IsValid)
            {
                await editEventRequestRepository.UpdateEditEventRequestList(requestsList);
                TempData["StatusMessage"] = "Changes have been saved successfully.";
                return await EditEventRequestsApproval();
            }
            TempData["StatusMessage"] = "Error: Invalid submission!";
            return View(requestsList);
        }

        private async Task<bool> IsNewRequestConflicting(DateTime time, string id, Event ev)
        {
            List<MissedEventRequest> requests = await missedEventRequestRepository.GetMissedEventRequestsByUserID(id);
            foreach (MissedEventRequest req in requests)
            {
                if (req.Time.Date == time.Date && req.Event == ev) // detect conflict by date and type in requested and existing ones
                {
                    return true;
                }
            }
            return false;
        }
    }
}
