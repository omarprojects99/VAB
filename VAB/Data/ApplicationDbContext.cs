
using System;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VAB.Models;

namespace VAB.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Categorie> Categories { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Prix> Prix { get; set; }
        public DbSet<Pnc> Pncs { get; set; }
        public DbSet<Cc> Ccs { get; set; }
        public DbSet<Equipage> Equipages { get; set; }
        public DbSet<PncEquipage> PncEquipages { get; set; }
        public DbSet<EtatDesVentesArrivee> EtatDesVentesArrivee { get; set; }
        public DbSet<EtatDesVentesDepart> EtatDesVentesDepart { get; set; }
        public DbSet<Vol> Vols { get; set; }
        public DbSet<FeuilleDeLignes> FeuilleDeLignes { get; set; }
        public DbSet<FeuilleEtatDesVentesArrivee> FeuilleEtatDesVentesArrivee { get; set; }
        public DbSet<FeuilleEtatDesVentesDepart> FeuilleEtatDesVentesDepart { get; set; }
        public DbSet<ArticleVente> ArticleVentes { get; set; }
        public DbSet<ArticleVenteDepart> ArticleVenteDepart { get; set; }
        public DbSet<EtatDesOffresArrivee> EtatDesOffresArrivee  { get; set; }
        public DbSet<EtatDesOffresDepart> EtatDesOffresDepart { get; set; }
        public DbSet<ArticleOffre> ArticleOffres { get; set; }
        public DbSet<ArticleOffreDepart> ArticleOffreDepart { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SalesComparison> SalesComparisons { get; set; }
        public DbSet<ComparisonArticle> ComparisonArticles { get; set; }
        public DbSet<OffersComparison> OffersComparisons { get; set; }
        public DbSet<ComparisonOfferArticle> ComparisonOfferArticles { get; set; }
        public DbSet<PncCommissionCalculation> PncCommissionCalculations { get; set; }
        public DbSet<PncCommissionDetail> PncCommissionDetails { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<TrouDeCaisse> TrouDeCaisses { get; set; }
        public DbSet<Redevance> Redevances { get; set; }
        public DbSet<RedevanceDetail> RedevanceDetails { get; set; }
        // New DbSets for PDF extraction
        public DbSet<ExtractedPdfData> ExtractedPdfData { get; set; }
        public DbSet<ExtractedPdfItem> ExtractedPdfItems { get; set; }
        public DbSet<Motif> Motifs { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //relation deuille arrivee
            builder.Entity<FeuilleEtatDesVentesArrivee>()
              .HasKey(j => new { j.FeuilleDeLignesId, j.EtatDesVentesArriveeId });
            //relation deuille depart
            builder.Entity<FeuilleEtatDesVentesDepart>()
             .HasKey(j => new { j.FeuilleDeLignesId, j.EtatDesVentesDepartId });
            //relationfeuille arivee 2
            builder.Entity<FeuilleEtatDesVentesArrivee>()
              .HasOne(j => j.FeuilleDeLignes)
              .WithMany(f => f.FeuilleEtatDesVentes)
              .HasForeignKey(j => j.FeuilleDeLignesId);
            //relation feuille depart 2
            builder.Entity<FeuilleEtatDesVentesDepart>()
              .HasOne(j => j.FeuilleDeLignes)
              .WithMany(f => f.FeuilleEtatDesDepart)
              .HasForeignKey(j => j.FeuilleDeLignesId);
            //relation feuille arrivee 3
            builder.Entity<FeuilleEtatDesVentesArrivee>()
              .HasOne(j => j.EtatDesVentesArrivee)
              .WithMany(e => e.FeuilleEtatDesVentes)
              .HasForeignKey(j => j.EtatDesVentesArriveeId);
            //relation feuille Depart 3
            builder.Entity<FeuilleEtatDesVentesDepart>()
              .HasOne(j => j.EtatDesVentesDepart)
              .WithMany(e => e.FeuilleEtatDesVentes)
              .HasForeignKey(j => j.EtatDesVentesDepartId);

            // PncEquipage many-to-many
            builder.Entity<PncEquipage>()
                .HasKey(pe => new { pe.PncId, pe.EquipageId });

            builder.Entity<PncEquipage>()
                .HasOne(pe => pe.Pnc)
                .WithMany()
                .HasForeignKey(pe => pe.PncId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PncEquipage>()
                .HasOne(pe => pe.Equipage)
                .WithMany(e => e.PncEquipages)
                .HasForeignKey(pe => pe.EquipageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Equipage relationships
            builder.Entity<Equipage>()
                .HasOne(e => e.Cc1)
                .WithMany()
                .HasForeignKey(e => e.Cc1Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Equipage>()
                .HasOne(e => e.Cc2)
                .WithMany()
                .HasForeignKey(e => e.Cc2Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Equipage>()
                .HasOne(e => e.Pnc1)
                .WithMany()
                .HasForeignKey(e => e.Pnc1Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Equipage>()
                .HasOne(e => e.Pnc2)
                .WithMany()
                .HasForeignKey(e => e.Pnc2Id)
                .OnDelete(DeleteBehavior.Restrict);
            // EtatDesVentesArrivee to Equipage
            builder.Entity<EtatDesVentesArrivee>()
                .HasOne(e => e.Equipage)
                .WithMany()
                .HasForeignKey(e => e.EquipageId)
                .OnDelete(DeleteBehavior.Cascade);
            // EtatDesVentesDepart to Equipage
            builder.Entity<EtatDesVentesDepart>()
                .HasOne(e => e.Equipage)
                .WithMany()
                .HasForeignKey(e => e.EquipageId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Vol>()
                .HasOne(v => v.FeuilleDeLignes)
                .WithMany(f => f.Vols)
                .HasForeignKey(v => v.FeuilleDeLignesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vol>()
                .HasOne(v => v.EtatDesVentesArrivee)
                .WithMany()
                .HasForeignKey(v => v.EtatDesVentesArriveeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vol>()
                .HasOne(v => v.EtatDesVentesDepart)
                .WithMany()
                .HasForeignKey(v => v.EtatDesVentesDepartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ArticleVente>()
                .HasOne(av => av.Article)
                .WithMany()
                .HasForeignKey(av => av.ArticleId)
                .OnDelete(DeleteBehavior.Restrict);
            //relation article vente Arrivee
            builder.Entity<ArticleVente>()
                .HasOne(av => av.EtatDesVentesArrivee)
                .WithMany(e => e.ArticleVentes)
                .HasForeignKey(av => av.EtatDesVentesArriveeId)
                .OnDelete(DeleteBehavior.Cascade);
            //relation article vente depart
            builder.Entity<ArticleVenteDepart>()
               .HasOne(av => av.EtatDesVentesDepart)
               .WithMany(e => e.ArticleVentesDepart)
               .HasForeignKey(av => av.EtatDesVentesDepartId)
               .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PncCommissionCalculation>()
    .HasOne(c => c.EtatDesVentesArrivee)
    .WithMany()
    .HasForeignKey(c => c.EtatDesVentesArriveeId)
    .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PncCommissionDetail>()
                .HasOne(d => d.PncCommissionCalculation)
                .WithMany(c => c.PncCommissionDetails)
                .HasForeignKey(d => d.PncCommissionCalculationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PncCommissionDetail>()
                .HasOne(d => d.Pnc)
                .WithMany()
                .HasForeignKey(d => d.PncId)
                .OnDelete(DeleteBehavior.Restrict);

            // Redevance relationships
            builder.Entity<RedevanceDetail>()
                .HasOne(rd => rd.Redevance)
                .WithMany(r => r.RedevanceDetails)
                .HasForeignKey(rd => rd.RedevanceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RedevanceDetail>()
                .HasOne(rd => rd.EtatDesVentesArrivee)
                .WithMany()
                .HasForeignKey(rd => rd.EtatDesVentesArriveeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Redevance>()
                .HasIndex(r => new { r.Annee, r.Mois })
                .IsUnique();

            // New relationships for PDF extraction
            builder.Entity<ExtractedPdfItem>()
                .HasOne(e => e.ExtractedPdfData)
                .WithMany(p => p.ExtractedItems)
                .HasForeignKey(e => e.ExtractedPdfDataId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal precision
            builder.Entity<ExtractedPdfItem>()
                .Property(e => e.Quantity)
            .HasPrecision(18, 2);

            builder.Entity<ExtractedPdfItem>()
                .Property(e => e.UnitPrice)
            .HasPrecision(18, 2);

            builder.Entity<ExtractedPdfItem>()
                .Property(e => e.TotalPrice)
                .HasPrecision(18, 2);

            // Add indexes for better performance
            builder.Entity<ExtractedPdfData>()
                .HasIndex(e => e.Numero)
            .IsUnique();

            builder.Entity<ExtractedPdfData>()
                .HasIndex(e => e.DateExtraction);

            builder.Entity<ExtractedPdfData>()
                .HasIndex(e => e.ExtractedBy);
            builder.Entity<Motif>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.NumeroEtatVenteArrivee)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ReadBy)
                    .HasMaxLength(100)
                    .IsRequired(false); // Explicitly set as nullable

                entity.Property(e => e.DateCreation)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Status)
                    .HasDefaultValue(MotifStatus.NonLu);

                // Indexes
                entity.HasIndex(e => e.NumeroEtatVenteArrivee)
                    .HasDatabaseName("IX_Motifs_NumeroEtatVenteArrivee");

                entity.HasIndex(e => e.DateCreation)
                    .HasDatabaseName("IX_Motifs_DateCreation");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_Motifs_Status");
            });
        }




    }







}

