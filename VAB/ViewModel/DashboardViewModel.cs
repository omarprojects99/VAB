using VAB.ViewModel;

namespace VAB.ViewModel
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();
        public List<UserManagementViewModel> RecentUsers { get; set; } = new List<UserManagementViewModel>();

        // Additional properties for charts
        public List<ChartDataPoint> UserStatusData { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> RoleDistributionData { get; set; } = new List<ChartDataPoint>();
    }
    public class ChartDataPoint
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public string Color { get; set; }
    }

}