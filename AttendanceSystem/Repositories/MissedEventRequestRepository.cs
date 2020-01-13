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
    public class MissedEventRequestRepository
    {
        private readonly AttendanceSystemContext db;
        private readonly UserEventRepository userEventRepository;
        private readonly WorkingDayRepository workingDayRepository;
        public MissedEventRequestRepository(AttendanceSystemContext db)
        {
            this.db = db;
            this.workingDayRepository = new WorkingDayRepository(db);
            this.userEventRepository = new UserEventRepository(db);
        }

        public async Task Add(MissedEventRequest missedEventRequest)
        {
            db.MissedEventRequests.Add(missedEventRequest);
            await db.SaveChangesAsync();
        }

        public async Task RemoveMissedEventRequest(string id)
        {
            MissedEventRequest target = await db.MissedEventRequests.FindAsync(id);
            db.MissedEventRequests.Remove(target);
            await db.SaveChangesAsync();
        }

        public async Task<List<MissedEventRequest>> GetMissedEventRequestsByUserID(string userID)
        {
            return await db.MissedEventRequests
                .Where(record => record.UserID == userID)
                .OrderByDescending(record => record.IssueDate) // Last First
                .Take(20)
                .ToListAsync();
        }

        public async Task<MissedEventRequest> GetMissedEventRequestByEventID(string id)
        {
            return await db.MissedEventRequests.FirstOrDefaultAsync(x => x.ID == id); 
        }

        public Task<bool> GetMissedEventRequestOnDayForUser(string userID, DateTime day)
        {
            return db.MissedEventRequests
                .AnyAsync(mer => mer.Time.Date == day && mer.UserID == userID && mer.Approval == Approval.Pending);
        }

        public async Task<List<MissedEventRequest>> GetPendingMissedEventRequests()
        {
            return await db.MissedEventRequests
                .Where(record => record.Approval == Approval.Pending)
                .OrderBy(record => record.Time) // Oldest First
                .Include(record => record.User)
                .ToListAsync();
        }

        public async Task UpdateMissedEventRequestList(List<MissedEventRequest> requestsList)
        {
            foreach (MissedEventRequest request in requestsList)
            {
                // Create & add new user event if the request is approved
                if (request.Approval == Approval.Approved)
                    await AddMissedEvent(request);

                // Update missed event request record if approval changed
                UpdateChangedRequestApproval(request);
            }
            await db.SaveChangesAsync();
        }

        private void UpdateChangedRequestApproval(MissedEventRequest request)
        {
            if (request.Approval == Approval.Approved || request.Approval == Approval.Rejected)
                db.MissedEventRequests.Update(request);
        }

        private async Task AddMissedEvent(MissedEventRequest request)
        {
            WorkingDay workingDay = await workingDayRepository.GetWorkingDayByDate(request.UserID, request.Time.Date) ?? new WorkingDay
            {
                UserID = request.UserID,
                Date = request.Time.Date
            };

            await userEventRepository.Add(new UserEvent
            {
                WorkingDayID = workingDay.ID,
                Time = request.Time,
                Event = request.Event,
            });

            await workingDayRepository.UpdateTimes(workingDay);
            await workingDayRepository.CheckOffensesInRange(workingDay.UserID, workingDay.Date, DateTime.Today.AddDays(-1));
        }
    }
}
