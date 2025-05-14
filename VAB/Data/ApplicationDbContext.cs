
using System;
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
        public DbSet<Vol> Vols { get; set; }
        public DbSet<FeuilleDeLignes> FeuilleDeLignes { get; set; }
        public DbSet<FeuilleEtatDesVentesArrivee> FeuilleEtatDesVentesArrivee { get; set; }
        public DbSet<ArticleVente> ArticleVentes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FeuilleEtatDesVentesArrivee>()
              .HasKey(j => new { j.FeuilleDeLignesId, j.EtatDesVentesArriveeId });

            builder.Entity<FeuilleEtatDesVentesArrivee>()
              .HasOne(j => j.FeuilleDeLignes)
              .WithMany(f => f.FeuilleEtatDesVentes)
              .HasForeignKey(j => j.FeuilleDeLignesId);

            builder.Entity<FeuilleEtatDesVentesArrivee>()
              .HasOne(j => j.EtatDesVentesArrivee)
              .WithMany(e => e.FeuilleEtatDesVentes)
              .HasForeignKey(j => j.EtatDesVentesArriveeId);

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
            builder.Entity<ArticleVente>()
                .HasOne(av => av.Article)
                .WithMany()
                .HasForeignKey(av => av.ArticleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ArticleVente>()
                .HasOne(av => av.EtatDesVentesArrivee)
                .WithMany(e => e.ArticleVentes)
                .HasForeignKey(av => av.EtatDesVentesArriveeId)
                .OnDelete(DeleteBehavior.Cascade);



        }




    }







}

