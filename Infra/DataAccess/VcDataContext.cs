using Microsoft.EntityFrameworkCore;
using VocabularyCards.Domain;

namespace VocabularyCards.Infra.DataAccess;
public class VcDataContext : DbContext
{
    public VcDataContext(DbContextOptions<VcDataContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Phrase>()
            .Property(p => p.Text)
            .IsUnicode()
            .IsRequired();
        modelBuilder.Entity<Phrase>()
            .HasAlternateKey(p => new { p.Text, p.Language });
        modelBuilder.Entity<Phrase>()
            .HasKey(p => p.Id);
        modelBuilder.Entity<Phrase>()
            .Property(p => p.ViewsСount).HasDefaultValue(0);
        modelBuilder.Entity<Phrase>()
            .Property(p => p.TranslationViewsСount).HasDefaultValue(0);
        modelBuilder.Entity<Phrase>()
            .Property(p => p.LatestShowUnixTimestamp).HasDefaultValue(0);

        modelBuilder.Entity<Translation>()
            .HasAlternateKey(p => new { p.PhraseId, p.Text, p.Language });
        modelBuilder.Entity<Translation>()
           .HasKey(p => p.Id);
        modelBuilder.Entity<Translation>()
           .HasOne<Phrase>()
           .WithMany()
           .HasForeignKey(t => t.PhraseId);

        modelBuilder.Entity<PhraseExample>()
           .HasAlternateKey(p => new { p.PhraseId, p.Text });
        modelBuilder.Entity<PhraseExample>()
           .HasKey(p => p.Id);
        modelBuilder.Entity<PhraseExample>()
           .HasOne<Phrase>()
           .WithMany()
           .HasForeignKey(t => t.PhraseId);

        modelBuilder.Entity<PhraseCollection>()
            .Property(c => c.Name)
            .HasMaxLength(128)
            .IsRequired();
        modelBuilder.Entity<PhraseCollection>()
            .HasAlternateKey(p => new { p.Name, p.Language });
        modelBuilder.Entity<PhraseCollection>()
           .HasKey(p => p.Id);
        modelBuilder.Entity<PhraseCollection>()
            .HasMany<Phrase>()
            .WithMany()
            .UsingEntity<CollectionToPhrase>(
            l => l.HasOne<Phrase>().WithMany().HasForeignKey(l => l.PhaseId),
            r => r.HasOne<PhraseCollection>().WithMany().HasForeignKey(l => l.CollectionId)
            );
    }
}
