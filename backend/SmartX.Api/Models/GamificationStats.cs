namespace SmartX.Api.Models;

public class GamificationStats
{
    public int TotalCriticalAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public int QuickResolutions { get; set; } // resolved within 5 minutes
    public int StabilityStreakHours { get; set; } // hours without critical alert
    public DateTime? LastCriticalAlertTime { get; set; }
    public Dictionary<string, int> SensorResolutionCounts { get; set; } = new();
    
    public int HealthScore => CalculateHealthScore();
    
    private int CalculateHealthScore()
    {
        // Base score: start at 100
        int score = 100;
        
        // Deduct for critical alerts that weren't resolved quickly
        int unresolved = TotalCriticalAlerts - ResolvedAlerts;
        score -= unresolved * 5; // Each unresolved alert costs 5 points
        
        // Bonus for quick resolutions
        double quickRate = TotalCriticalAlerts > 0 
            ? (double)QuickResolutions / TotalCriticalAlerts 
            : 1.0;
        score += (int)(quickRate * 10);
        
        // Bonus for stability streak
        score += Math.Min(StabilityStreakHours / 2, 20); // Max 20 bonus
        
        // Clamp between 0 and 100
        return Math.Clamp(score, 0, 100);
    }
}