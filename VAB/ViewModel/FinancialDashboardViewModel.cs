using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VAB.Models;

namespace VAB.ViewModel
{
    public class FinancialDashboardViewModel
    {
        // Commission Statistics
        public int TotalCommissionCalculations { get; set; }
        public decimal TotalCommissionsAmount { get; set; }
        public decimal AverageCommissionPerPnc { get; set; }
        public int ActivePncsWithCommissions { get; set; }

        // Redevance Statistics
        public int TotalRedevanceRecords { get; set; }
        public decimal TotalRedevanceAmount { get; set; }
        public decimal CurrentMonthRedevance { get; set; }
        public decimal LastMonthRedevance { get; set; }
        public decimal RedevanceGrowthPercentage { get; set; }

        // Trou de Caisse Statistics
        public int TotalTrousDeCaisse { get; set; }
        public decimal TotalMontantManquant { get; set; }
        public int TrousEnAttente { get; set; }
        public int TrousAppliques { get; set; }
        public decimal AverageTrouAmount { get; set; }

        // Recent Data
        public List<RecentCommissionViewModel> RecentCommissions { get; set; } = new();
        public List<RecentTrouDeCaisseViewModel> RecentTrousDeCaisse { get; set; } = new();
        public List<MonthlyRevenueViewModel> MonthlyRevenues { get; set; } = new();

        // Top Performers
        public List<TopPncPerformance> TopPncs { get; set; } = new();
        public List<MonthlyTrendViewModel> CommissionTrends { get; set; } = new();
        public List<MonthlyTrendViewModel> RedevanceTrends { get; set; } = new();

        // Period Filters
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }

    public class RecentCommissionViewModel
    {
        public int Id { get; set; }
        public string PncName { get; set; } = string.Empty;
        public string NumeroEtat { get; set; } = string.Empty;
        public DateTime DateCalcul { get; set; }
        public decimal CommissionTotale { get; set; }
        public string CalculatedBy { get; set; } = string.Empty;
    }

    public class RecentTrouDeCaisseViewModel
    {
        public int Id { get; set; }
        public string PncResponsable { get; set; } = string.Empty;
        public string NumeroEtat { get; set; } = string.Empty;
        public DateTime DateDetection { get; set; }
        public decimal MontantManquant { get; set; }
        public string Statut { get; set; } = string.Empty;
        public bool EstApplique { get; set; }
    }

    public class MonthlyRevenueViewModel
    {
        public int Mois { get; set; }
        public int Annee { get; set; }
        public string MoisNom { get; set; } = string.Empty;
        public decimal TotalCommissions { get; set; }
        public decimal TotalRedevances { get; set; }
        public decimal TotalTrousDeCaisse { get; set; }
        public decimal NetRevenue { get; set; }
    }

    public class MonthlyTrendViewModel
    {
        public int Mois { get; set; }
        public int Annee { get; set; }
        public string MoisNom { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public int Count { get; set; }
        public decimal CroissanceVsPrecedent { get; set; }
    }
}
