using Microsoft.EntityFrameworkCore;
using VAB.Data;
using VAB.Models;

namespace VAB.Services
{
    public class RedevanceAnalyticsService : IRedevanceAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public RedevanceAnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EtatDesVentesArrivee>> GetValidatedStatesForPeriodAsync(int mois, int annee)
        {
            return await _context.EtatDesVentesArrivee
                .Include(e => e.ArticleVentes)
                    .ThenInclude(av => av.Article)
                .Where(e => e.IsValidated &&
                           e.DateEdition.Month == mois &&
                           e.DateEdition.Year == annee)
                .OrderBy(e => e.DateEdition)
                .ToListAsync();
        }

        public async Task<RedevanceAnalytics> GetAdvancedAnalyticsAsync(int annee)
        {
            var redevances = await _context.Redevances
                .Where(r => r.Annee == annee)
                .Include(r => r.RedevanceDetails)
                .ToListAsync();

            var redevancesPrecedente = await _context.Redevances
                .Where(r => r.Annee == annee - 1)
                .ToListAsync();

            var totalAnnuel = redevances.Sum(r => r.MontantRedevance);
            var totalPrecedent = redevancesPrecedente.Sum(r => r.MontantRedevance);
            var croissance = totalPrecedent > 0 ? ((totalAnnuel - totalPrecedent) / totalPrecedent) * 100 : 0;

            var monthlyData = await GetMonthlyTrendsAsync(annee);
            var meilleurMois = monthlyData.OrderByDescending(m => m.Montant).FirstOrDefault();
            var pireMois = monthlyData.Where(m => m.Montant > 0).OrderBy(m => m.Montant).FirstOrDefault();

            return new RedevanceAnalytics
            {
                TotalRedevanceAnnuelle = totalAnnuel,
                MoyenneMensuelle = redevances.Any() ? totalAnnuel / 12 : 0,
                CroissanceAnnuelle = croissance,
                MeilleurMois = meilleurMois?.MoisNom ?? "N/A",
                PireMois = pireMois?.MoisNom ?? "N/A",
                RedevancesMensuelles = monthlyData,
                TopProduits = await GetTopSellingProductsAsync(null, annee, 10),
                PerformanceEquipages = await GetCrewPerformanceAsync(annee),
                TendanceVentes = await GetSalesTrendsAsync(annee),
                Predictions = await GetPredictionsAsync()
            };
        }

        public async Task<List<TopSellingProduct>> GetTopSellingProductsAsync(int? mois = null, int? annee = null, int top = 10)
        {
            var articlesVentes = await _context.ArticleVentes
                .Include(av => av.Article)
                    .ThenInclude(a => a.Categorie)
                .Include(av => av.EtatDesVentesArrivee)
                .Where(av => av.EtatDesVentesArrivee.IsValidated)
                .Where(av => !annee.HasValue || av.EtatDesVentesArrivee.DateEdition.Year == annee.Value)
                .Where(av => !mois.HasValue || av.EtatDesVentesArrivee.DateEdition.Month == mois.Value)
                .ToListAsync();

            var results = articlesVentes
                .GroupBy(av => new {
                    av.ArticleId,
                    Description = av.Article.Description,
                    Categorie = av.Article.Categorie?.Description ?? "Non catégorisé"
                })
                .Select(g => new TopSellingProduct
                {
                    ArticleId = g.Key.ArticleId,
                    Description = g.Key.Description,
                    Categorie = g.Key.Categorie,
                    QuantiteTotale = g.Sum(av => av.QuantiteVendu),
                    ValeurTotale = g.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT),
                    RedevanceGeneree = g.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT) * 0.40m,
                    NombreVentes = g.Count(),
                    PrixMoyen = g.Average(av => av.PrixUnitaireHT)
                })
                .OrderByDescending(p => p.ValeurTotale)
                .Take(top)
                .ToList();

            return results;
        }

        public async Task<List<CrewPerformance>> GetCrewPerformanceAsync(int annee)
        {
            var etatsVentes = await _context.EtatDesVentesArrivee
                .Include(e => e.Equipage)
                    .ThenInclude(eq => eq.Pnc1)
                .Include(e => e.Equipage)
                    .ThenInclude(eq => eq.Pnc1)
                .Include(e => e.ArticleVentes)
                .Where(e => e.IsValidated && e.DateEdition.Year == annee)
                .ToListAsync();

            var performances = etatsVentes
                .GroupBy(e => new {
                    e.EquipageId,
                    Cc1Name = e.Equipage.Pnc1?.Nom + " " + e.Equipage.Pnc1?.Prenom,
                    Pnc1Name = e.Equipage.Pnc1?.Nom + " " + e.Equipage.Pnc1?.Prenom
                })
                .Select(g => new CrewPerformance
                {
                    EquipageInfo = $"CC: {g.Key.Cc1Name ?? "N/A"}, PNC: {g.Key.Pnc1Name ?? "N/A"}",
                    TotalVentes = g.Sum(e => e.ArticleVentes.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT)),
                    RedevanceGeneree = g.Sum(e => e.ArticleVentes.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT)) * 0.40m,
                    NombreVols = g.Count(),
                    VenteMoyenneParVol = g.Count() > 0 ? g.Average(e => e.ArticleVentes.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT)) : 0
                })
                .OrderByDescending(p => p.TotalVentes)
                .Take(10)
                .ToList();

            return performances;
        }

        public async Task<PredictionData> GetPredictionsAsync()
        {
            var derniersMois = await _context.Redevances
                .OrderByDescending(r => r.Annee)
                .ThenByDescending(r => r.Mois)
                .Take(6)
                .ToListAsync();

            if (derniersMois.Count < 3)
            {
                return new PredictionData
                {
                    RedevancePrevueMoisProchain = 0,
                    RedevancePrevueAnnee = 0,
                    Tendance = "Données insuffisantes",
                    Confiance = 0,
                    Recommandations = new List<string> { "Collectez plus de données pour des prédictions précises" }
                };
            }

            var moyenne = derniersMois.Average(r => r.MontantRedevance);
            var tendance = derniersMois.Take(3).Average(r => r.MontantRedevance) >
                          derniersMois.Skip(3).Average(r => r.MontantRedevance) ? "Croissante" : "Décroissante";

            var recommandations = new List<string>();

            if (tendance == "Croissante")
            {
                recommandations.Add("📈 Excellente performance ! Maintenez cette dynamique");
                recommandations.Add("🎯 Concentrez-vous sur les produits les plus rentables");
                recommandations.Add("👥 Récompensez les équipages les plus performants");
            }
            else
            {
                recommandations.Add("⚠️ Tendance à la baisse détectée");
                recommandations.Add("🔍 Analysez les causes de la diminution");
                recommandations.Add("💡 Considérez de nouveaux produits ou stratégies");
            }

            return new PredictionData
            {
                RedevancePrevueMoisProchain = moyenne * (tendance == "Croissante" ? 1.05m : 0.95m),
                RedevancePrevueAnnee = moyenne * 12 * (tendance == "Croissante" ? 1.1m : 0.9m),
                Tendance = tendance,
                Confiance = 75,
                Recommandations = recommandations
            };
        }

        public async Task<List<MonthlyRedevance>> GetMonthlyTrendsAsync(int annee)
        {
            var redevances = await _context.Redevances
                .Where(r => r.Annee == annee)
                .ToListAsync();

            var monthlyData = new List<MonthlyRedevance>();

            for (int mois = 1; mois <= 12; mois++)
            {
                var redevance = redevances.FirstOrDefault(r => r.Mois == mois);
                var precedent = await _context.Redevances
                    .FirstOrDefaultAsync(r => r.Mois == mois && r.Annee == annee - 1);

                var croissance = 0m;
                if (precedent != null && redevance != null && precedent.MontantRedevance > 0)
                {
                    croissance = ((redevance.MontantRedevance - precedent.MontantRedevance) / precedent.MontantRedevance) * 100;
                }

                monthlyData.Add(new MonthlyRedevance
                {
                    Mois = mois,
                    Annee = annee,
                    MoisNom = GetMonthName(mois),
                    Montant = redevance?.MontantRedevance ?? 0,
                    TotalVentes = redevance?.TotalVentes ?? 0,
                    NombreEtats = redevance?.NombreEtats ?? 0,
                    CroissanceVsPrecedent = croissance,
                    IsValidated = redevance?.IsValidated ?? false
                });
            }

            return monthlyData;
        }

        public async Task<Dictionary<string, object>> GetRealTimeStatsAsync()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var etatsValidesAujourdhui = await _context.EtatDesVentesArrivee
                .Where(e => e.IsValidated && e.DateEdition.Date == DateTime.Today)
                .CountAsync();

            var articlesAujourdhui = await _context.ArticleVentes
                .Include(av => av.EtatDesVentesArrivee)
                .Where(av => av.EtatDesVentesArrivee.IsValidated &&
                            av.EtatDesVentesArrivee.DateEdition.Date == DateTime.Today)
                .ToListAsync();

            var ventesAujourdhui = articlesAujourdhui.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT);

            var redevanceMoisCourant = await _context.Redevances
                .Where(r => r.Mois == currentMonth && r.Annee == currentYear)
                .Select(r => r.MontantRedevance)
                .FirstOrDefaultAsync();

            return new Dictionary<string, object>
            {
                ["etatsValidesAujourdhui"] = etatsValidesAujourdhui,
                ["ventesAujourdhui"] = ventesAujourdhui,
                ["redevanceAujourdhui"] = ventesAujourdhui * 0.40m,
                ["redevanceMoisCourant"] = redevanceMoisCourant,
                ["timestamp"] = DateTime.Now
            };
        }

        public async Task<List<object>> GetChartDataAsync(string chartType, int annee)
        {
            switch (chartType.ToLower())
            {
                case "monthly":
                    var monthlyData = await GetMonthlyTrendsAsync(annee);
                    return monthlyData.Select(m => new {
                        month = m.MoisNom,
                        redevance = m.Montant,
                        ventes = m.TotalVentes,
                        croissance = m.CroissanceVsPrecedent
                    }).Cast<object>().ToList();

                case "products":
                    var products = await GetTopSellingProductsAsync(null, annee, 5);
                    return products.Select(p => new {
                        product = p.Description,
                        value = p.ValeurTotale,
                        quantity = p.QuantiteTotale
                    }).Cast<object>().ToList();

                case "crew":
                    var crews = await GetCrewPerformanceAsync(annee);
                    return crews.Take(5).Select(c => new {
                        crew = c.EquipageInfo.Split(',')[0].Replace("CC: ", ""),
                        performance = c.TotalVentes
                    }).Cast<object>().ToList();

                default:
                    return new List<object>();
            }
        }

        private async Task<List<TrendData>> GetSalesTrendsAsync(int annee)
        {
            var etatsVentes = await _context.EtatDesVentesArrivee
                .Include(e => e.ArticleVentes)
                .Where(e => e.IsValidated && e.DateEdition.Year == annee)
                .ToListAsync();

            var trends = etatsVentes
                .GroupBy(e => e.DateEdition.Month)
                .Select(g => new TrendData
                {
                    Date = new DateTime(annee, g.Key, 1),
                    Valeur = g.Sum(e => e.ArticleVentes.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT)),
                    Type = "Ventes"
                })
                .OrderBy(t => t.Date)
                .ToList();

            return trends;
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Janvier",
                2 => "Février",
                3 => "Mars",
                4 => "Avril",
                5 => "Mai",
                6 => "Juin",
                7 => "Juillet",
                8 => "Août",
                9 => "Septembre",
                10 => "Octobre",
                11 => "Novembre",
                12 => "Décembre",
                _ => "Inconnu"
            };
        }
    }
}
