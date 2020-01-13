using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Repositories
{
    public class EditEventRequestRepository
    {
        private readonly AttendanceSystemContext db;
        private readonly UserEventRepository userEventRepository;
        private readonly WorkingDayRepository workingDayRepository;

        public EditEventRequestRepository(AttendanceSystemContext context)
        {
            db = context;
            workingDayRepository = new WorkingDayRepository(context);
            userEventRepository = new UserEventRepository(context);
        }

        public async Task Add(EditEventRequest editEventRequest)
        {
            // Get the last edit request on the same original user event record (if exists) 
            EditEventRequest lastEditRequest = await db.EditEventRequests
                .Where(record => record.UserEventID == editEventRequest.UserEventID)
                .FirstOrDefaultAsync();
            // Check if there exists an edit request on the original user event record then remove it
            if(lastEditRequest != null)
            {
                db.EditEventRequests.Remove(lastEditRequest);
            }
            
            await db.EditEventRequests.AddAsync(editEventRequest);
            await db.SaveChangesAsync();
        }

        public async Task<List<EditEventRequest>> GetPendingEditEventRequests()
        {
            return await db.EditEventRequests
                .Where(record => record.Approval == Approval.Pending)
                .OrderBy(record => record.IssueDate) // Oldest First
                .Include(record => record.UserEvent.WorkingDay.User)
                .ToListAsync();
        }

        public async Task<List<EditEventRequest>> GetEditEventRequestsByUserID(string userID)
        {
            return await db.EditEventRequests
                .Where(record => record.UserEvent.WorkingDay.UserID == userID)
                .OrderByDescending(record => record.IssueDate)
                .Include(record => record.UserEvent)
                .Take(20)
                .ToListAsync();
        }

        public async Task UpdateEditEventRequestList(List<EditEventRequest> requestsList)
        {
            foreach (EditEventRequest request in requestsList)
            {
                // Update the original event if the edit request is approved
                if (request.Approval == Approval.Approved)
                {
                    UserEvent updatedEvent = await userEventRepository.GetEventByID(request.UserEventID);
                    WorkingDay workingDay = await workingDayRepository.GetWorkingDayByID(updatedEvent.WorkingDayID);
                    updatedEvent.Time = request.NewTime;
                    await userEventRepository.Update(updatedEvent);
                    await workingDayRepository.UpdateTimes(workingDay);
                    await workingDayRepository.CheckOffensesInRange(workingDay.UserID, workingDay.Date, DateTime.Today.AddDays(-1));
                }
                db.EditEventRequests.Update(request);
            }
            await db.SaveChangesAsync();
        }
    }
}
