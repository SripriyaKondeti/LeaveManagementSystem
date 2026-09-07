using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveSummaryDto> GetLeaveSummaryAsync()
    {
        // Total employees
        var totalEmployees = await _context.Employees
            .CountAsync();

        // Active employees
        var activeEmployees = await _context.Employees
            .CountAsync(e => e.IsActive);

        // Pending requests
        var pendingRequests = await _context.LeaveRequests
            .CountAsync(l => l.Status == "Pending");

        // Approved requests
        var approvedRequests = await _context.LeaveRequests
            .CountAsync(l => l.Status == "Approved");

        // Rejected requests
        var rejectedRequests = await _context.LeaveRequests
            .CountAsync(l => l.Status == "Rejected");

        // Get approved requests
        var approvedLeaves = await _context.LeaveRequests
            .Where(l => l.Status == "Approved")
            .Select(l => new
            {
                l.FromDate,
                l.ToDate
            })
            .ToListAsync();

        // Calculate total approved leave days
        var totalLeavesTaken = approvedLeaves
            .Sum(l => (l.ToDate - l.FromDate).Days + 1);

        var summary = new LeaveSummaryDto
        {
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            PendingRequests = pendingRequests,
            ApprovedRequests = approvedRequests,
            RejectedRequests = rejectedRequests,
            TotalLeavesTaken = totalLeavesTaken
        };

        return summary;
    }
}