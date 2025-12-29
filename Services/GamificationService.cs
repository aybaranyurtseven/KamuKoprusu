using KamuKoprusu.Data;
using KamuKoprusu.Enums;
using KamuKoprusu.Models;
using Microsoft.EntityFrameworkCore;

namespace KamuKoprusu.Services;

public interface IGamificationService
{
    Task CheckAndAwardBadgesAsync(string userId);
    Task UpdateUserLevelAsync(string userId);
    Task<int> CalculateReputationScoreAsync(string userId);
}

public class GamificationService : IGamificationService
{
    private readonly ApplicationDbContext _context;

    public GamificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Check all badge criteria and award any earned badges to the user
    /// </summary>
    public async Task CheckAndAwardBadgesAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.UserBadges)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return;

        var allBadges = await _context.Badges.ToListAsync();
        var userBadgeIds = user.UserBadges.Select(ub => ub.BadgeId).ToHashSet();

        foreach (var badge in allBadges)
        {
            // Skip if user already has this badge
            if (userBadgeIds.Contains(badge.Id)) continue;

            // Check if user meets criteria
            bool earned = await CheckBadgeCriteriaAsync(userId, badge);

            if (earned)
            {
                var userBadge = new UserBadge
                {
                    UserId = userId,
                    BadgeId = badge.Id,
                    EarnedAt = DateTime.UtcNow
                };
                _context.UserBadges.Add(userBadge);

                // Award reputation points for earning badge
                user.ReputationScore += GetBadgePoints(badge);
            }
        }

        await _context.SaveChangesAsync();

        // Update level after awarding badges
        await UpdateUserLevelAsync(userId);
    }

    /// <summary>
    /// Check if user meets specific badge criteria
    /// </summary>
    private async Task<bool> CheckBadgeCriteriaAsync(string userId, Badge badge)
    {
        switch (badge.CriteriaType)
        {
            case "ComplaintSubmitted":
                var submittedCount = await _context.Complaints
                    .CountAsync(c => c.UserId == userId);
                return submittedCount >= badge.RequiredCount;

            case "ComplaintResolved":
                var resolvedCount = await _context.Complaints
                    .CountAsync(c => c.UserId == userId && c.Status == ComplaintStatus.Resolved);
                return resolvedCount >= badge.RequiredCount;

            case "MediaUploaded":
                var complaintsWithMedia = await _context.Complaints
                    .Where(c => c.UserId == userId && c.MediaFiles.Any())
                    .CountAsync();
                return complaintsWithMedia >= badge.RequiredCount;

            case "QuickResolution":
                var quickResolutions = await _context.Complaints
                    .Where(c => c.UserId == userId && 
                           c.Status == ComplaintStatus.Resolved &&
                           c.ResolvedAt.HasValue &&
                           EF.Functions.DateDiffDay(c.CreatedAt, c.ResolvedAt.Value) <= 3)
                    .CountAsync();
                return quickResolutions >= badge.RequiredCount;

            default:
                return false;
        }
    }

    /// <summary>
    /// Get points awarded for earning a badge
    /// </summary>
    private int GetBadgePoints(Badge badge)
    {
        return badge.CriteriaType switch
        {
            "ComplaintSubmitted" => badge.RequiredCount * 5,
            "ComplaintResolved" => badge.RequiredCount * 10,
            "MediaUploaded" => badge.RequiredCount * 3,
            "QuickResolution" => 25,
            _ => 10
        };
    }

    /// <summary>
    /// Update user level based on reputation score
    /// </summary>
    public async Task UpdateUserLevelAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        // Calculate reputation if not set
        if (user.ReputationScore == 0)
        {
            user.ReputationScore = await CalculateReputationScoreAsync(userId);
        }

        // Level thresholds
        user.Level = user.ReputationScore switch
        {
            >= 500 => UserLevel.Diamond,
            >= 200 => UserLevel.Platinum,
            >= 100 => UserLevel.Gold,
            >= 50 => UserLevel.Silver,
            _ => UserLevel.Bronze
        };

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Calculate total reputation score for a user
    /// </summary>
    public async Task<int> CalculateReputationScoreAsync(string userId)
    {
        int score = 0;

        // Points for submitted complaints (5 points each)
        var submittedCount = await _context.Complaints.CountAsync(c => c.UserId == userId);
        score += submittedCount * 5;

        // Points for resolved complaints (20 points each)
        var resolvedCount = await _context.Complaints
            .CountAsync(c => c.UserId == userId && c.Status == ComplaintStatus.Resolved);
        score += resolvedCount * 20;

        // Points for complaints with media (3 points each)
        var mediaCount = await _context.Complaints
            .Where(c => c.UserId == userId && c.MediaFiles.Any())
            .CountAsync();
        score += mediaCount * 3;

        // Points from badges
        var badgePoints = await _context.UserBadges
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Badge)
            .SumAsync(ub => GetBadgePoints(ub.Badge));
        score += badgePoints;

        // Negative points for warnings (-20 each)
        var warningCount = await _context.Warnings.CountAsync(w => w.UserId == userId);
        score -= warningCount * 20;

        return Math.Max(0, score); // Minimum 0
    }
}
