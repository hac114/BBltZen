using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BBltZen;

public partial class BubbleTeaContext : DbContext
{
    public BubbleTeaContext()
    {
    }

    public BubbleTeaContext(DbContextOptions<BubbleTeaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Articolo> Articolo { get; set; }

    public virtual DbSet<BevandaCustom> BevandaCustom { get; set; }

    public virtual DbSet<BevandaStandard> BevandaStandard { get; set; }

    public virtual DbSet<CategoriaIngrediente> CategoriaIngrediente { get; set; }

    public virtual DbSet<Cliente> Cliente { get; set; }

    public virtual DbSet<ConfigSoglieTempi> ConfigSoglieTempi { get; set; }

    public virtual DbSet<DimensioneBicchiere> DimensioneBicchiere { get; set; }

    public virtual DbSet<DimensioneQuantitaIngredienti> DimensioneQuantitaIngredienti { get; set; }

    public virtual DbSet<Dolce> Dolce { get; set; }

    public virtual DbSet<Ingrediente> Ingrediente { get; set; }

    public virtual DbSet<IngredientiPersonalizzazione> IngredientiPersonalizzazione { get; set; }

    public virtual DbSet<LogAccessi> LogAccessi { get; set; }

    public virtual DbSet<LogAttivita> LogAttivita { get; set; }

    public virtual DbSet<MigrationAudit> MigrationAudit { get; set; }

    public virtual DbSet<NotificheOperative> NotificheOperative { get; set; }

    public virtual DbSet<OrderItem> OrderItem { get; set; }

    public virtual DbSet<Ordine> Ordine { get; set; }

    public virtual DbSet<Personalizzazione> Personalizzazione { get; set; }

    public virtual DbSet<PersonalizzazioneCustom> PersonalizzazioneCustom { get; set; }

    public virtual DbSet<PersonalizzazioneIngrediente> PersonalizzazioneIngrediente { get; set; }

    public virtual DbSet<PreferitiCliente> PreferitiCliente { get; set; }

    public virtual DbSet<SessioniQr> SessioniQr { get; set; }

    public virtual DbSet<StatisticheCache> StatisticheCache { get; set; }

    public virtual DbSet<StatoOrdine> StatoOrdine { get; set; }

    public virtual DbSet<StatoPagamento> StatoPagamento { get; set; }

    public virtual DbSet<StatoStoricoOrdine> StatoStoricoOrdine { get; set; }

    public virtual DbSet<Tavolo> Tavolo { get; set; }

    public virtual DbSet<TaxRates> TaxRates { get; set; }

    public virtual DbSet<TempPriceCalculations> TempPriceCalculations { get; set; }

    public virtual DbSet<TriggerLogs> TriggerLogs { get; set; }

    public virtual DbSet<UnitaDiMisura> UnitaDiMisura { get; set; }

    public virtual DbSet<Utenti> Utenti { get; set; }

    public virtual DbSet<VwArticoliCompleti> VwArticoliCompleti { get; set; }

    public virtual DbSet<VwBevandePreferiteClienti> VwBevandePreferiteClienti { get; set; }

    public virtual DbSet<VwCombinazioniPopolari> VwCombinazioniPopolari { get; set; }

    public virtual DbSet<VwDashboardAmministrativa> VwDashboardAmministrativa { get; set; }

    public virtual DbSet<VwDashboardSintesi> VwDashboardSintesi { get; set; }

    public virtual DbSet<VwDashboardStatistiche> VwDashboardStatistiche { get; set; }

    public virtual DbSet<VwIngredientiPopolari> VwIngredientiPopolari { get; set; }

    public virtual DbSet<VwIngredientiPopolariAdvanced> VwIngredientiPopolariAdvanced { get; set; }

    public virtual DbSet<VwMenuDinamico> VwMenuDinamico { get; set; }

    public virtual DbSet<VwMonitoraggioOperativo> VwMonitoraggioOperativo { get; set; }

    public virtual DbSet<VwNotifichePendenti> VwNotifichePendenti { get; set; }

    public virtual DbSet<VwOrderCalculationSupport> VwOrderCalculationSupport { get; set; }

    public virtual DbSet<VwOrdiniAnnullati> VwOrdiniAnnullati { get; set; }

    public virtual DbSet<VwOrdiniSospesi> VwOrdiniSospesi { get; set; }

    public virtual DbSet<VwStatisticheCache> VwStatisticheCache { get; set; }

    public virtual DbSet<VwStatisticheGiornaliere> VwStatisticheGiornaliere { get; set; }

    public virtual DbSet<VwStatisticheMensili> VwStatisticheMensili { get; set; }

    public virtual DbSet<VwStatisticheOrdiniAvanzate> VwStatisticheOrdiniAvanzate { get; set; }

    public virtual DbSet<VwStatisticheRecenti> VwStatisticheRecenti { get; set; }

    public virtual DbSet<VwTempiStato> VwTempiStato { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=DESKTOP-U1ADL0N;Database=BubbleTea;Trusted_Connection=true;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Articolo>(entity =>
        {
            //HO AGGIUNTO QUESTA RIGA PER GESTIRE CONFIGURAZIONE UTENTI
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BubbleTeaContext).Assembly);

            entity.HasKey(e => e.ArticoloId).HasName("PK__ARTICOLO__2902CE994A30025C");

            entity.ToTable("ARTICOLO");

            entity.Property(e => e.ArticoloId).HasColumnName("articolo_id");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.Tipo)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("tipo");
        });

        modelBuilder.Entity<BevandaCustom>(entity =>
        {
            entity.HasKey(e => e.ArticoloId).HasName("PK_BevandaCustom_ARTICOLO");

            entity.ToTable("BEVANDA_CUSTOM", tb =>
                {
                    tb.HasTrigger("TR_BEVANDA_CUSTOM_CalcolaPrezzo");
                    tb.HasTrigger("trg_BEVANDA_CUSTOM_UpdatePrice");
                });

            entity.Property(e => e.ArticoloId)
                .ValueGeneratedNever()
                .HasColumnName("articolo_id");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.PersCustomId).HasColumnName("pers_custom_id");
            entity.Property(e => e.Prezzo)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("prezzo");

            entity.HasOne(d => d.Articolo).WithOne(p => p.BevandaCustom)
                .HasForeignKey<BevandaCustom>(d => d.ArticoloId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BEVANDA_CUSTOM_ARTICOLO");

            entity.HasOne(d => d.PersCustom).WithMany(p => p.BevandaCustom)
                .HasForeignKey(d => d.PersCustomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BEVANDA_CUSTOM_PERSONALIZZAZIONE");
        });

        modelBuilder.Entity<BevandaStandard>(entity =>
        {
            entity.HasKey(e => e.ArticoloId).HasName("PK_BevandaStandard_ARTICOLO");

            entity.ToTable("BEVANDA_STANDARD");

            entity.Property(e => e.ArticoloId)
                .ValueGeneratedNever()
                .HasColumnName("articolo_id");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.DimensioneBicchiereId).HasColumnName("dimensione_bicchiere_id");
            entity.Property(e => e.Disponibile)
                .HasDefaultValue(true)
                .HasColumnName("disponibile");
            entity.Property(e => e.ImmagineUrl)
                .HasMaxLength(500)
                .HasColumnName("immagine_url");
            entity.Property(e => e.PersonalizzazioneId).HasColumnName("personalizzazione_id");
            entity.Property(e => e.Prezzo)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("prezzo");
            entity.Property(e => e.Priorita)
                .HasDefaultValue(1)
                .HasColumnName("priorita");
            entity.Property(e => e.SempreDisponibile).HasColumnName("sempre_disponibile");

            entity.HasOne(d => d.Articolo).WithOne(p => p.BevandaStandard)
                .HasForeignKey<BevandaStandard>(d => d.ArticoloId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BEVANDA_S__artic__3DB3258D");

            entity.HasOne(d => d.DimensioneBicchiere).WithMany(p => p.BevandaStandard)
                .HasForeignKey(d => d.DimensioneBicchiereId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BEVANDA_STANDARD_Dimensione_bicchiere");

            entity.HasOne(d => d.Personalizzazione).WithMany(p => p.BevandaStandard)
                .HasForeignKey(d => d.PersonalizzazioneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BEVANDA_STANDARD_Personalizzazione");
        });

        modelBuilder.Entity<CategoriaIngrediente>(entity =>
        {
            entity.HasKey(e => e.CategoriaId);

            entity.ToTable("Categoria_ingrediente");

            entity.HasIndex(e => e.Categoria, "UQ_Categoria_ingrediente_Categoria").IsUnique();

            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.Categoria)
                .HasMaxLength(50)
                .HasColumnName("categoria");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.ClienteId).HasName("PK__CLIENTE__47E34D64C3A9E5CD");

            entity.ToTable("CLIENTE");

            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.TavoloId).HasColumnName("tavolo_id");

            entity.HasOne(d => d.Tavolo).WithMany(p => p.Cliente)
                .HasForeignKey(d => d.TavoloId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CLIENTE__tavolo___6497E884");
        });

        modelBuilder.Entity<ConfigSoglieTempi>(entity =>
        {
            entity.HasKey(e => e.SogliaId).HasName("PK__CONFIG_S__8D105E6D4CEE88F6");

            entity.ToTable("CONFIG_SOGLIE_TEMPI");

            entity.HasIndex(e => e.DataAggiornamento, "IX_CONFIG_SOGLIE_TEMPI_data_aggiornamento");

            entity.HasIndex(e => e.StatoOrdineId, "UQ_CONFIG_SOGLIE_TEMPI_stato_ordine_id").IsUnique();

            entity.Property(e => e.SogliaId).HasColumnName("soglia_id");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.SogliaAttenzione).HasColumnName("soglia_attenzione");
            entity.Property(e => e.SogliaCritico).HasColumnName("soglia_critico");
            entity.Property(e => e.StatoOrdineId).HasColumnName("stato_ordine_id");
            entity.Property(e => e.UtenteAggiornamento)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("utente_aggiornamento");

            entity.HasOne(d => d.StatoOrdine).WithOne(p => p.ConfigSoglieTempi)
                .HasForeignKey<ConfigSoglieTempi>(d => d.StatoOrdineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CONFIG_SO__stato__5CC1BC92");
        });

        modelBuilder.Entity<DimensioneBicchiere>(entity =>
        {
            entity.ToTable("Dimensione_bicchiere");

            entity.HasIndex(e => new { e.Sigla, e.Descrizione }, "UQ_DimensioneBicchiere_SiglaDescrizione").IsUnique();

            entity.Property(e => e.DimensioneBicchiereId).HasColumnName("dimensione_bicchiere_id");
            entity.Property(e => e.Capienza)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("capienza");
            entity.Property(e => e.Descrizione)
                .HasMaxLength(50)
                .HasColumnName("descrizione");
            entity.Property(e => e.Moltiplicatore)
                .HasDefaultValue(100m)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("moltiplicatore");
            entity.Property(e => e.PrezzoBase)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("prezzo_base");
            entity.Property(e => e.Sigla)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("sigla");
            entity.Property(e => e.UnitaMisuraId).HasColumnName("unita_misura_id");

            entity.HasOne(d => d.UnitaMisura).WithMany(p => p.DimensioneBicchiere)
                .HasForeignKey(d => d.UnitaMisuraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Dimensione_bicchiere_Unita_di_misura");
        });

        modelBuilder.Entity<DimensioneQuantitaIngredienti>(entity =>
        {
            entity.HasKey(e => e.DimensioneId);

            entity.ToTable("Dimensione_quantita_ingredienti");

            entity.HasIndex(e => new { e.DimensioneBicchiereId, e.PersonalizzazioneIngredienteId }, "IX_DimensioneQuantitaIngredienti_CombinazioneUnica").IsUnique();

            entity.Property(e => e.DimensioneId).HasColumnName("dimensione_id");
            entity.Property(e => e.DimensioneBicchiereId).HasColumnName("dimensione_bicchiere_id");
            entity.Property(e => e.Moltiplicatore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("moltiplicatore");
            entity.Property(e => e.PersonalizzazioneIngredienteId).HasColumnName("personalizzazione_ingrediente_id");

            entity.HasOne(d => d.DimensioneBicchiere).WithMany(p => p.DimensioneQuantitaIngredienti)
                .HasForeignKey(d => d.DimensioneBicchiereId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Dimensione_quantita_ingredienti_Dimensione_bicchiere");

            entity.HasOne(d => d.PersonalizzazioneIngrediente).WithMany(p => p.DimensioneQuantitaIngredienti)
                .HasForeignKey(d => d.PersonalizzazioneIngredienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Dimensione_quantita_ingredienti_Personalizzazione_ingrediente");
        });

        modelBuilder.Entity<Dolce>(entity =>
        {
            entity.HasKey(e => e.ArticoloId).HasName("PK__DOLCE__2902CE99651286CB");

            entity.ToTable("DOLCE", tb =>
                {
                    tb.HasTrigger("trg_DolceAfterInsert");
                    tb.HasTrigger("trg_DolceAfterUpdate");
                    tb.HasTrigger("trg_UpdateDolce");
                });

            entity.Property(e => e.ArticoloId)
                .ValueGeneratedNever()
                .HasColumnName("articolo_id");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.Descrizione)
                .HasMaxLength(255)
                .HasColumnName("descrizione");
            entity.Property(e => e.Disponibile)
                .HasDefaultValue(true)
                .HasColumnName("disponibile");
            entity.Property(e => e.ImmagineUrl)
                .HasMaxLength(500)
                .HasColumnName("immagine_url");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.Prezzo)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("prezzo");
            entity.Property(e => e.Priorita)
                .HasDefaultValue(1)
                .HasColumnName("priorita");

            entity.HasOne(d => d.Articolo).WithOne(p => p.Dolce)
                .HasForeignKey<Dolce>(d => d.ArticoloId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DOLCE__articolo___4924D839");
        });

        modelBuilder.Entity<Ingrediente>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_Ingrediente_Update_Disponibilita"));

            entity.HasIndex(e => e.Ingrediente1, "UQ_Ingrediente_Nome").IsUnique();

            entity.Property(e => e.IngredienteId).HasColumnName("ingrediente_id");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataInserimento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_inserimento");
            entity.Property(e => e.Disponibile)
                .HasDefaultValue(true)
                .HasColumnName("disponibile");
            entity.Property(e => e.Ingrediente1)
                .HasMaxLength(50)
                .HasColumnName("ingrediente");
            entity.Property(e => e.PrezzoAggiunto)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("prezzo_aggiunto");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Ingrediente)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ingrediente_Categoria_ingrediente");
        });

        modelBuilder.Entity<IngredientiPersonalizzazione>(entity =>
        {
            entity.HasKey(e => e.IngredientePersId).HasName("PK__INGREDIE__115C7C85D6FAB05C");

            entity.ToTable("INGREDIENTI_PERSONALIZZAZIONE");

            entity.HasIndex(e => new { e.PersCustomId, e.IngredienteId }, "UC_INGREDIENTE_PERSONALIZZAZIONE").IsUnique();

            entity.Property(e => e.IngredientePersId).HasColumnName("ingrediente_pers_id");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.IngredienteId).HasColumnName("ingrediente_id");
            entity.Property(e => e.PersCustomId).HasColumnName("pers_custom_id");

            entity.HasOne(d => d.Ingrediente).WithMany(p => p.IngredientiPersonalizzazione)
                .HasForeignKey(d => d.IngredienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_INGREDIENTE");

            entity.HasOne(d => d.PersCustom).WithMany(p => p.IngredientiPersonalizzazione)
                .HasForeignKey(d => d.PersCustomId)
                .HasConstraintName("FK_PERS_CUSTOM");
        });

        modelBuilder.Entity<LogAccessi>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__LogAcces__9E2397E007CE0209");

            entity.HasIndex(e => e.DataCreazione, "IX_LogAccessi_DataCreazione");

            entity.HasIndex(e => e.Esito, "IX_LogAccessi_Esito");

            entity.HasIndex(e => e.UtenteId, "IX_LogAccessi_UtenteID");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.Dettagli)
                .HasMaxLength(1000)
                .HasColumnName("dettagli");
            entity.Property(e => e.Esito)
                .HasMaxLength(20)
                .HasColumnName("esito");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.TipoAccesso)
                .HasMaxLength(20)
                .HasColumnName("tipo_accesso");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
            entity.Property(e => e.UtenteId).HasColumnName("utente_id");

            entity.HasOne(d => d.Cliente).WithMany(p => p.LogAccessi)
                .HasForeignKey(d => d.ClienteId)
                .HasConstraintName("FK_LogAccessi_CLIENTE");

            entity.HasOne(d => d.Utente).WithMany(p => p.LogAccessi)
                .HasForeignKey(d => d.UtenteId)
                .HasConstraintName("FK_LogAccessi_UTENTI");
        });

        modelBuilder.Entity<LogAttivita>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__LOG_ATTI__9E2397E0530D85AD");

            entity.ToTable("LOG_ATTIVITA");

            entity.HasIndex(e => e.DataEsecuzione, "IX_LOG_ATTIVITA_DataEsecuzione");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.DataEsecuzione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_esecuzione");
            entity.Property(e => e.Descrizione)
                .HasMaxLength(500)
                .HasColumnName("descrizione");
            entity.Property(e => e.Dettagli).HasColumnName("dettagli");
            entity.Property(e => e.TipoAttivita)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_attivita");
            entity.Property(e => e.UtenteId).HasColumnName("utente_id");

            entity.HasOne(d => d.Utente).WithMany(p => p.LogAttivita)
                .HasForeignKey(d => d.UtenteId)
                .HasConstraintName("FK_LOG_ATTIVITA_UTENTI");
        });

        modelBuilder.Entity<MigrationAudit>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK__Migratio__5AF33E334196E2F6");

            entity.ToTable("Migration_Audit");

            entity.Property(e => e.AuditId).HasColumnName("audit_id");
            entity.Property(e => e.ExecutedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("executed_at");
            entity.Property(e => e.ExecutedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(suser_sname())")
                .HasColumnName("executed_by");
            entity.Property(e => e.MigrationPhase)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("migration_phase");
            entity.Property(e => e.Notes)
                .HasMaxLength(1000)
                .HasColumnName("notes");
            entity.Property(e => e.ObjectName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("object_name");
            entity.Property(e => e.ObjectType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("object_type");
            entity.Property(e => e.OperationType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("operation_type");
        });

        modelBuilder.Entity<NotificheOperative>(entity =>
        {
            entity.HasKey(e => e.NotificaId).HasName("PK__NOTIFICH__815AD28D318491A2");

            entity.ToTable("NOTIFICHE_OPERATIVE");

            entity.Property(e => e.NotificaId).HasColumnName("notifica_id");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.DataGestione)
                .HasColumnType("datetime")
                .HasColumnName("data_gestione");
            entity.Property(e => e.Messaggio)
                .HasMaxLength(500)
                .HasColumnName("messaggio");
            entity.Property(e => e.OrdiniCoinvolti)
                .IsUnicode(false)
                .HasColumnName("ordini_coinvolti");
            entity.Property(e => e.Priorita)
                .HasDefaultValue(1)
                .HasColumnName("priorita");
            entity.Property(e => e.Stato)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("da_gestire")
                .HasColumnName("stato");
            entity.Property(e => e.TipoNotifica)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("sistema")
                .HasColumnName("tipo_notifica");
            entity.Property(e => e.UtenteGestione)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("utente_gestione");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__ORDER_IT__3764B6BC4BD27D7B");

            entity.ToTable("ORDER_ITEM", tb =>
                {
                    tb.HasTrigger("trg_ORDER_ITEM_Calculate");
                    tb.HasTrigger("trg_ORDER_ITEM_CreateTime");
                    tb.HasTrigger("trg_ORDINE_UpdateTotal");
                    tb.HasTrigger("trg_OrderItemAfterInsert");
                });

            entity.HasIndex(e => e.ArticoloId, "IX_ORDER_ITEM_articolo_id");

            entity.HasIndex(e => new { e.PrezzoUnitario, e.Quantita }, "IX_ORDER_ITEM_prezzo_quantita_composite");

            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.ArticoloId).HasColumnName("articolo_id");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.Imponibile)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("imponibile");
            entity.Property(e => e.OrdineId).HasColumnName("ordine_id");
            entity.Property(e => e.PrezzoUnitario)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("prezzo_unitario");
            entity.Property(e => e.Quantita).HasColumnName("quantita");
            entity.Property(e => e.ScontoApplicato)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("sconto_applicato");
            entity.Property(e => e.TaxRateId).HasColumnName("tax_rate_id");
            entity.Property(e => e.TipoArticolo)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("tipo_articolo");
            entity.Property(e => e.TotaleIvato)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("totale_ivato");

            entity.HasOne(d => d.Articolo).WithMany(p => p.OrderItem)
                .HasForeignKey(d => d.ArticoloId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItem_Articolo");

            entity.HasOne(d => d.Ordine).WithMany(p => p.OrderItem)
                .HasForeignKey(d => d.OrdineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ORDER_ITEM_ORDINE");

            entity.HasOne(d => d.TaxRate).WithMany(p => p.OrderItem)
                .HasForeignKey(d => d.TaxRateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ORDER_ITEM_TAX_RATES");
        });

        modelBuilder.Entity<Ordine>(entity =>
        {
            entity.HasKey(e => e.OrdineId).HasName("PK__ORDINE__B828CB6AE214FF1E");

            entity.ToTable("ORDINE", tb =>
                {
                    tb.HasTrigger("trg_ORDINE_AfterUpdate");
                    tb.HasTrigger("trg_TrackingStatiOrdine");
                });

            entity.Property(e => e.OrdineId).HasColumnName("ordine_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.Priorita)
                .HasDefaultValue(1)
                .HasColumnName("priorita");
            entity.Property(e => e.SessioneId).HasColumnName("sessione_id");
            entity.Property(e => e.StatoOrdineId)
                .HasDefaultValue(8)
                .HasColumnName("stato_ordine_id");
            entity.Property(e => e.StatoPagamentoId)
                .HasDefaultValue(5)
                .HasColumnName("stato_pagamento_id");
            entity.Property(e => e.Totale)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("totale");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Ordine)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDINE__cliente___6C390A4C");

            entity.HasOne(d => d.Sessione).WithMany(p => p.Ordine)
                .HasForeignKey(d => d.SessioneId)
                .HasConstraintName("FK_ORDINE_SessioniQR");

            entity.HasOne(d => d.StatoOrdine).WithMany(p => p.Ordine)
                .HasForeignKey(d => d.StatoOrdineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDINE__stato_or__6D2D2E85");

            entity.HasOne(d => d.StatoPagamento).WithMany(p => p.Ordine)
                .HasForeignKey(d => d.StatoPagamentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDINE__stato_pa__6E2152BE");
        });

        modelBuilder.Entity<Personalizzazione>(entity =>
        {
            entity.HasIndex(e => e.Nome, "UQ_Personalizzazione_Nome").IsUnique();

            entity.Property(e => e.PersonalizzazioneId).HasColumnName("personalizzazione_id");
            entity.Property(e => e.Descrizione).HasColumnName("descrizione");
            entity.Property(e => e.DtCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("dt_creazione");
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<PersonalizzazioneCustom>(entity =>
        {
            entity.HasKey(e => e.PersCustomId).HasName("PK__PERSONAL__776FA86624F5A943");

            entity.ToTable("PERSONALIZZAZIONE_CUSTOM");

            entity.Property(e => e.PersCustomId).HasColumnName("pers_custom_id");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.DimensioneBicchiereId).HasColumnName("dimensione_bicchiere_id");
            entity.Property(e => e.GradoDolcezza).HasColumnName("grado_dolcezza");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasDefaultValue("Bevanda Custom")
                .HasColumnName("nome");

            entity.HasOne(d => d.DimensioneBicchiere).WithMany(p => p.PersonalizzazioneCustom)
                .HasForeignKey(d => d.DimensioneBicchiereId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DIMENSIONE_BICCHIERE");
        });

        modelBuilder.Entity<PersonalizzazioneIngrediente>(entity =>
        {
            entity.ToTable("Personalizzazione_ingrediente");

            entity.HasIndex(e => new { e.PersonalizzazioneId, e.IngredienteId }, "UQ_Personalizzazione_Ingrediente").IsUnique();

            entity.Property(e => e.PersonalizzazioneIngredienteId).HasColumnName("personalizzazione_ingrediente_id");
            entity.Property(e => e.IngredienteId).HasColumnName("ingrediente_id");
            entity.Property(e => e.PersonalizzazioneId).HasColumnName("personalizzazione_id");
            entity.Property(e => e.Quantita)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("quantita");
            entity.Property(e => e.UnitaMisuraId).HasColumnName("unita_misura_id");

            entity.HasOne(d => d.Ingrediente).WithMany(p => p.PersonalizzazioneIngrediente)
                .HasForeignKey(d => d.IngredienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personalizzazione_ingrediente_Ingrediente");

            entity.HasOne(d => d.Personalizzazione).WithMany(p => p.PersonalizzazioneIngrediente)
                .HasForeignKey(d => d.PersonalizzazioneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personalizzazione_ingrediente_Personalizzazione");

            entity.HasOne(d => d.UnitaMisura).WithMany(p => p.PersonalizzazioneIngrediente)
                .HasForeignKey(d => d.UnitaMisuraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personalizzazione_ingrediente_Unita_di_misura");
        });

        modelBuilder.Entity<PreferitiCliente>(entity =>
        {
            entity.HasKey(e => e.PreferitoId).HasName("PK__PREFERIT__5001C707488D07D4");

            entity.ToTable("PREFERITI_CLIENTE");

            entity.HasIndex(e => new { e.ClienteId, e.TipoArticolo }, "IX_PREFERITI_ClienteTipo");

            entity.HasIndex(e => e.DataAggiunta, "IX_PREFERITI_DataAggiunta");

            entity.Property(e => e.PreferitoId).HasColumnName("preferito_id");
            entity.Property(e => e.BevandaId).HasColumnName("bevanda_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.DataAggiunta)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiunta");
            entity.Property(e => e.DimensioneBicchiereId).HasColumnName("dimensione_bicchiere_id");
            entity.Property(e => e.GradoDolcezza).HasColumnName("grado_dolcezza");
            entity.Property(e => e.IngredientiJson).HasColumnName("ingredienti_json");
            entity.Property(e => e.NomePersonalizzato)
                .HasMaxLength(100)
                .HasColumnName("nome_personalizzato");
            entity.Property(e => e.NotePersonali)
                .HasMaxLength(500)
                .HasColumnName("note_personali");
            entity.Property(e => e.TipoArticolo)
                .HasMaxLength(2)
                .HasColumnName("tipo_articolo");

            entity.HasOne(d => d.Cliente).WithMany(p => p.PreferitiCliente)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PREFERITI__clien__68F2894D");

            entity.HasOne(d => d.DimensioneBicchiere).WithMany(p => p.PreferitiCliente)
                .HasForeignKey(d => d.DimensioneBicchiereId)
                .HasConstraintName("FK_PREFERITI_DimensioneBicchiere");
        });

        modelBuilder.Entity<SessioniQr>(entity =>
        {
            entity.HasKey(e => e.SessioneId).HasName("PK__Sessioni__4E8FD248FEDB2562");

            entity.ToTable("SessioniQR");

            entity.HasIndex(e => e.ClienteId, "IX_SessioniQR_ClienteID");

            entity.HasIndex(e => e.QrCode, "IX_SessioniQR_QRCode");

            entity.HasIndex(e => e.DataScadenza, "IX_SessioniQR_Scadenza").HasFilter("([utilizzato]=(0))");

            entity.Property(e => e.SessioneId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("sessione_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.CodiceSessione)
                .HasMaxLength(100)
                .HasDefaultValue("TEMP")
                .HasColumnName("codice_sessione");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.DataScadenza)
                .HasColumnType("datetime")
                .HasColumnName("data_scadenza");
            entity.Property(e => e.DataUtilizzo)
                .HasColumnType("datetime")
                .HasColumnName("data_utilizzo");
            entity.Property(e => e.QrCode)
                .HasMaxLength(500)
                .HasColumnName("qr_code");
            entity.Property(e => e.Stato)
                .HasMaxLength(20)
                .HasDefaultValue("Attiva")
                .HasColumnName("stato");
            entity.Property(e => e.TavoloId)
                .HasDefaultValue(1)
                .HasColumnName("tavolo_id");
            entity.Property(e => e.Utilizzato)
                .HasDefaultValue(false)
                .HasColumnName("utilizzato");

            entity.HasOne(d => d.Tavolo).WithMany(p => p.SessioniQr)
                .HasForeignKey(d => d.TavoloId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SessioniQR_TAVOLO");
        });

        modelBuilder.Entity<StatisticheCache>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__STATISTI__3213E83FBC3EBA94");

            entity.ToTable("STATISTICHE_CACHE");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.Metriche).HasColumnName("metriche");
            entity.Property(e => e.Periodo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("periodo");
            entity.Property(e => e.TipoStatistica)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_statistica");
        });

        modelBuilder.Entity<StatoOrdine>(entity =>
        {
            entity.ToTable("STATO_ORDINE");

            entity.HasIndex(e => e.StatoOrdine1, "UQ_stato_ordine_valore").IsUnique();

            entity.Property(e => e.StatoOrdineId).HasColumnName("stato_ordine_id");
            entity.Property(e => e.StatoOrdine1)
                .HasMaxLength(50)
                .HasColumnName("stato_ordine");
            entity.Property(e => e.Terminale).HasColumnName("terminale");
        });

        modelBuilder.Entity<StatoPagamento>(entity =>
        {
            entity.ToTable("STATO_PAGAMENTO");

            entity.HasIndex(e => e.StatoPagamento1, "UQ_STATO_PAGAMENTO_stato_pagamento").IsUnique();

            entity.Property(e => e.StatoPagamentoId).HasColumnName("stato_pagamento_id");
            entity.Property(e => e.StatoPagamento1)
                .HasMaxLength(50)
                .HasColumnName("stato_pagamento");
        });

        modelBuilder.Entity<StatoStoricoOrdine>(entity =>
        {
            entity.HasKey(e => e.StatoStoricoOrdineId).HasName("PK__STATO_ST__10AE0582261A0ADA");

            entity.ToTable("STATO_STORICO_ORDINE");

            entity.Property(e => e.StatoStoricoOrdineId).HasColumnName("stato_storico_ordine_id");
            entity.Property(e => e.Fine)
                .HasColumnType("datetime")
                .HasColumnName("fine");
            entity.Property(e => e.Inizio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("inizio");
            entity.Property(e => e.OrdineId).HasColumnName("ordine_id");
            entity.Property(e => e.StatoOrdineId).HasColumnName("stato_ordine_id");

            entity.HasOne(d => d.Ordine).WithMany(p => p.StatoStoricoOrdine)
                .HasForeignKey(d => d.OrdineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__STATO_STO__ordin__5708E33C");

            entity.HasOne(d => d.StatoOrdine).WithMany(p => p.StatoStoricoOrdine)
                .HasForeignKey(d => d.StatoOrdineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__STATO_STO__stato__57FD0775");
        });

        modelBuilder.Entity<Tavolo>(entity =>
        {
            entity.HasKey(e => e.TavoloId).HasName("PK__TAVOLO__72A0F5647A82716B");

            entity.ToTable("TAVOLO");

            entity.HasIndex(e => e.Numero, "UQ__TAVOLO__FC77F211931F5723").IsUnique();

            entity.Property(e => e.TavoloId).HasColumnName("tavolo_id");
            entity.Property(e => e.Disponibile)
                .HasDefaultValue(true)
                .HasColumnName("disponibile");
            entity.Property(e => e.Numero).HasColumnName("numero");
            entity.Property(e => e.Zona)
                .HasMaxLength(50)
                .HasColumnName("zona");
        });

        modelBuilder.Entity<TaxRates>(entity =>
        {
            entity.HasKey(e => e.TaxRateId).HasName("PK__TAX_RATE__4B78B333D9257EB6");

            entity.ToTable("TAX_RATES");

            entity.HasIndex(e => new { e.Aliquota, e.Descrizione }, "UQ_TAX_RATES_Aliquota_Descrizione").IsUnique();

            entity.Property(e => e.TaxRateId).HasColumnName("tax_rate_id");
            entity.Property(e => e.Aliquota)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("aliquota");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.Descrizione)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("descrizione");
        });

        modelBuilder.Entity<TempPriceCalculations>(entity =>
        {
            entity.HasKey(e => e.TempId).HasName("PK__Temp_Pri__FEEC6BDB2AF05344");

            entity.ToTable("Temp_Price_Calculations");

            entity.HasIndex(e => e.ArticoloId, "IX_Temp_Price_Calculations_articolo");

            entity.Property(e => e.TempId).HasColumnName("temp_id");
            entity.Property(e => e.ArticoloId).HasColumnName("articolo_id");
            entity.Property(e => e.CalcolatoDa)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(suser_sname())")
                .HasColumnName("calcolato_da");
            entity.Property(e => e.DataCalcolo)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_calcolo");
            entity.Property(e => e.PersCustomId).HasColumnName("pers_custom_id");
            entity.Property(e => e.PrezzoCalcolato)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("prezzo_calcolato");
        });

        modelBuilder.Entity<TriggerLogs>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__TRIGGER___9E2397E06F281AFD");

            entity.ToTable("TRIGGER_LOGS");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.Details)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("details");
            entity.Property(e => e.ExecutionDurationMs).HasColumnName("execution_duration_ms");
            entity.Property(e => e.ExecutionTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("execution_time");
            entity.Property(e => e.OrdersUpdated).HasColumnName("orders_updated");
            entity.Property(e => e.TriggerName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("trigger_name");
        });

        modelBuilder.Entity<UnitaDiMisura>(entity =>
        {
            entity.HasKey(e => e.UnitaMisuraId).HasName("PK_Unita_di_misura_1");

            entity.ToTable("Unita_di_misura");

            entity.HasIndex(e => e.Descrizione, "UQ_Unita_di_misura_descrizione").IsUnique();

            entity.HasIndex(e => e.Sigla, "UQ_Unita_di_misura_sigla").IsUnique();

            entity.Property(e => e.UnitaMisuraId).HasColumnName("unita_misura_id");
            entity.Property(e => e.Descrizione)
                .HasMaxLength(10)
                .HasColumnName("descrizione");
            entity.Property(e => e.Sigla)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("sigla");
        });

        modelBuilder.Entity<Utenti>(entity =>
        {
            entity.HasKey(e => e.UtenteId).HasName("PK__UTENTI__758675A89F3207EE");

            entity.ToTable("UTENTI", tb => tb.HasTrigger("trg_UTENTI_UpdateDate"));

            entity.HasIndex(e => e.ClienteId, "IX_UTENTI_ClienteID").HasFilter("([cliente_id] IS NOT NULL)");

            entity.HasIndex(e => e.Email, "IX_UTENTI_Email");

            entity.HasIndex(e => e.TipoUtente, "IX_UTENTI_TipoUtente");

            entity.HasIndex(e => e.Email, "UQ__UTENTI__AB6E6164408A3EDD").IsUnique();

            entity.Property(e => e.UtenteId).HasColumnName("utente_id");
            entity.Property(e => e.Attivo)
                .HasDefaultValue(true)
                .HasColumnName("attivo");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Cognome)
                .HasMaxLength(100)
                .HasColumnName("cognome");
            entity.Property(e => e.DataAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataCreazione)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(512)
                .HasColumnName("password_hash");
            entity.Property(e => e.SessioneGuest).HasColumnName("sessione_guest");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoUtente)
                .HasMaxLength(20)
                .HasColumnName("tipo_utente");
            entity.Property(e => e.UltimoAccesso)
                .HasColumnType("datetime")
                .HasColumnName("ultimo_accesso");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Utenti)
                .HasForeignKey(d => d.ClienteId)
                .HasConstraintName("FK_UTENTI_CLIENTE");
        });

        modelBuilder.Entity<VwArticoliCompleti>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_articoli_completi");

            entity.Property(e => e.AliquotaIva)
                .HasColumnType("numeric(4, 2)")
                .HasColumnName("aliquota_iva");
            entity.Property(e => e.ArticoloId).HasColumnName("articolo_id");
            entity.Property(e => e.Categoria)
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasColumnName("categoria");
            entity.Property(e => e.DataAggiornamento)
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataCreazione)
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.Disponibile).HasColumnName("disponibile");
            entity.Property(e => e.NomeArticolo)
                .HasMaxLength(100)
                .HasColumnName("nome_articolo");
            entity.Property(e => e.PrezzoBase)
                .HasColumnType("decimal(38, 3)")
                .HasColumnName("prezzo_base");
            entity.Property(e => e.TipoArticolo)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("tipo_articolo");
        });

        modelBuilder.Entity<VwBevandePreferiteClienti>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_BEVANDE_PREFERITE_CLIENTI");

            entity.Property(e => e.BevandaDisponibile).HasColumnName("bevanda_disponibile");
            entity.Property(e => e.BevandaId).HasColumnName("bevanda_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.DataAggiunta)
                .HasColumnType("datetime")
                .HasColumnName("data_aggiunta");
            entity.Property(e => e.Descrizione).HasColumnName("descrizione");
            entity.Property(e => e.ImmagineUrl)
                .HasMaxLength(500)
                .HasColumnName("immagine_url");
            entity.Property(e => e.IvaPercentuale)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("iva_percentuale");
            entity.Property(e => e.NomeBevanda)
                .HasMaxLength(50)
                .HasColumnName("nome_bevanda");
            entity.Property(e => e.PreferitoId).HasColumnName("preferito_id");
            entity.Property(e => e.PrezzoLordo)
                .HasColumnType("decimal(15, 8)")
                .HasColumnName("prezzo_lordo");
            entity.Property(e => e.PrezzoNetto)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("prezzo_netto");
            entity.Property(e => e.TavoloId).HasColumnName("tavolo_id");
        });

        modelBuilder.Entity<VwCombinazioniPopolari>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_combinazioni_popolari");

            entity.Property(e => e.Combinazione)
                .HasMaxLength(4000)
                .HasColumnName("combinazione");
            entity.Property(e => e.GiorniAttivita).HasColumnName("giorni_attivita");
            entity.Property(e => e.NumeroIngredienti).HasColumnName("numero_ingredienti");
            entity.Property(e => e.OrdiniPerGiorno)
                .HasColumnType("numeric(24, 12)")
                .HasColumnName("ordini_per_giorno");
            entity.Property(e => e.PrimaDataOrdine)
                .HasColumnType("datetime")
                .HasColumnName("prima_data_ordine");
            entity.Property(e => e.UltimaDataOrdine)
                .HasColumnType("datetime")
                .HasColumnName("ultima_data_ordine");
            entity.Property(e => e.VolteOrdinate).HasColumnName("volte_ordinate");
        });

        modelBuilder.Entity<VwDashboardAmministrativa>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_dashboard_amministrativa");

            entity.Property(e => e.LivelloPriorita).HasColumnName("livello_priorita");
            entity.Property(e => e.Mediana).HasColumnName("mediana");
            entity.Property(e => e.Percentile90).HasColumnName("percentile_90");
            entity.Property(e => e.SogliaAttenzione).HasColumnName("soglia_attenzione");
            entity.Property(e => e.SogliaCritico).HasColumnName("soglia_critico");
            entity.Property(e => e.StatoOrdine)
                .HasMaxLength(50)
                .HasColumnName("stato_ordine");
            entity.Property(e => e.StatoOrdineId).HasColumnName("stato_ordine_id");
            entity.Property(e => e.TempoMassimo).HasColumnName("tempo_massimo");
            entity.Property(e => e.TempoMedioMinuti)
                .HasColumnType("decimal(38, 6)")
                .HasColumnName("tempo_medio_minuti");
            entity.Property(e => e.TooltipStatistiche)
                .HasMaxLength(151)
                .IsUnicode(false)
                .HasColumnName("tooltip_statistiche");
            entity.Property(e => e.TotaleOrdini).HasColumnName("totale_ordini");
        });

        modelBuilder.Entity<VwDashboardSintesi>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_dashboard_sintesi");

            entity.Property(e => e.InRitardoCritico).HasColumnName("in_ritardo_critico");
            entity.Property(e => e.MaxMinuti).HasColumnName("max_minuti");
            entity.Property(e => e.Quantita).HasColumnName("quantita");
            entity.Property(e => e.Stato)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("stato");
        });

        modelBuilder.Entity<VwDashboardStatistiche>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_dashboard_statistiche");

            entity.Property(e => e.DataRiferimento).HasColumnName("data_riferimento");
            entity.Property(e => e.FatturatoTotale)
                .HasMaxLength(4000)
                .HasColumnName("fatturato_totale");
            entity.Property(e => e.GiorniMesiPassati).HasColumnName("giorni_mesi_passati");
            entity.Property(e => e.OrdiniAnnullati)
                .HasMaxLength(4000)
                .HasColumnName("ordini_annullati");
            entity.Property(e => e.OrdiniConsegnati)
                .HasMaxLength(4000)
                .HasColumnName("ordini_consegnati");
            entity.Property(e => e.Periodo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("periodo");
            entity.Property(e => e.TempoMedioMinuti)
                .HasMaxLength(4000)
                .HasColumnName("tempo_medio_minuti");
            entity.Property(e => e.TipoStatistica)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_statistica");
            entity.Property(e => e.TotaleOrdini)
                .HasMaxLength(4000)
                .HasColumnName("totale_ordini");
        });

        modelBuilder.Entity<VwIngredientiPopolari>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ingredienti_popolari");

            entity.Property(e => e.Categoria)
                .HasMaxLength(50)
                .HasColumnName("categoria");
            entity.Property(e => e.IngredienteId).HasColumnName("ingrediente_id");
            entity.Property(e => e.NomeIngrediente)
                .HasMaxLength(50)
                .HasColumnName("nome_ingrediente");
            entity.Property(e => e.NumeroOrdiniContenenti).HasColumnName("numero_ordini_contenenti");
            entity.Property(e => e.NumeroSelezioni).HasColumnName("numero_selezioni");
            entity.Property(e => e.PercentualeTotale)
                .HasColumnType("numeric(26, 12)")
                .HasColumnName("percentuale_totale");
        });

        modelBuilder.Entity<VwIngredientiPopolariAdvanced>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ingredienti_popolari_advanced");

            entity.Property(e => e.Categoria)
                .HasMaxLength(50)
                .HasColumnName("categoria");
            entity.Property(e => e.GiorniConSelezioni).HasColumnName("giorni_con_selezioni");
            entity.Property(e => e.IngredienteId).HasColumnName("ingrediente_id");
            entity.Property(e => e.NomeIngrediente)
                .HasMaxLength(50)
                .HasColumnName("nome_ingrediente");
            entity.Property(e => e.NumeroOrdiniContenenti).HasColumnName("numero_ordini_contenenti");
            entity.Property(e => e.NumeroSelezioni).HasColumnName("numero_selezioni");
            entity.Property(e => e.PercentualeSuOrdiniTotali)
                .HasColumnType("numeric(26, 12)")
                .HasColumnName("percentuale_su_ordini_totali");
            entity.Property(e => e.PercentualeTotale)
                .HasColumnType("numeric(26, 12)")
                .HasColumnName("percentuale_totale");
        });

        modelBuilder.Entity<VwMenuDinamico>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_MENU_DINAMICO");

            entity.Property(e => e.Descrizione).HasColumnName("descrizione");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ImmagineUrl)
                .HasMaxLength(500)
                .HasColumnName("immagine_url");
            entity.Property(e => e.IvaPercentuale)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("iva_percentuale");
            entity.Property(e => e.NomeBevanda)
                .HasMaxLength(100)
                .HasColumnName("nome_bevanda");
            entity.Property(e => e.PrezzoLordo)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("prezzo_lordo");
            entity.Property(e => e.PrezzoNetto)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("prezzo_netto");
            entity.Property(e => e.Priorita).HasColumnName("priorita");
            entity.Property(e => e.TaxRateId).HasColumnName("tax_rate_id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasColumnName("tipo");
        });

        modelBuilder.Entity<VwMonitoraggioOperativo>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_monitoraggio_operativo");

            entity.Property(e => e.LivelloAllerta)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("livello_allerta");
            entity.Property(e => e.Messaggio)
                .HasMaxLength(47)
                .IsUnicode(false)
                .HasColumnName("messaggio");
            entity.Property(e => e.MinutiInStato).HasColumnName("minuti_in_stato");
            entity.Property(e => e.OrdineId).HasColumnName("ordine_id");
            entity.Property(e => e.RichiedeInterventoImmediato).HasColumnName("richiede_intervento_immediato");
            entity.Property(e => e.SogliaAttenzione).HasColumnName("soglia_attenzione");
            entity.Property(e => e.SogliaCritico).HasColumnName("soglia_critico");
            entity.Property(e => e.StatoOrdine)
                .HasMaxLength(50)
                .HasColumnName("stato_ordine");
        });

        modelBuilder.Entity<VwNotifichePendenti>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_notifiche_pendenti");

            entity.Property(e => e.DataCreazione)
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.Messaggio)
                .HasMaxLength(500)
                .HasColumnName("messaggio");
            entity.Property(e => e.MinutiDaCreazione).HasColumnName("minuti_da_creazione");
            entity.Property(e => e.NotificaId)
                .ValueGeneratedOnAdd()
                .HasColumnName("notifica_id");
            entity.Property(e => e.OrdiniCoinvolti)
                .IsUnicode(false)
                .HasColumnName("ordini_coinvolti");
            entity.Property(e => e.Priorita).HasColumnName("priorita");
        });

        modelBuilder.Entity<VwOrderCalculationSupport>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_Order_Calculation_Support");

            entity.Property(e => e.ArticoloId).HasColumnName("articolo_id");
            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.OrdineId).HasColumnName("ordine_id");
            entity.Property(e => e.PrezzoBase)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("prezzo_base");
            entity.Property(e => e.Quantita).HasColumnName("quantita");
            entity.Property(e => e.TaxRate)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("tax_rate");
            entity.Property(e => e.TipoArticolo)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("tipo_articolo");
        });

        modelBuilder.Entity<VwOrdiniAnnullati>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ordini_annullati");

            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.DataAnnullamento)
                .HasColumnType("datetime")
                .HasColumnName("data_annullamento");
            entity.Property(e => e.DataCreazione)
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.MinutiPrimaAnnullamento).HasColumnName("minuti_prima_annullamento");
            entity.Property(e => e.OrdineId).HasColumnName("ordine_id");
            entity.Property(e => e.Totale)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("totale");
        });

        modelBuilder.Entity<VwOrdiniSospesi>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ordini_sospesi");

            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.DataCreazione)
                .HasColumnType("datetime")
                .HasColumnName("data_creazione");
            entity.Property(e => e.LivelloAllerta)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("livello_allerta");
            entity.Property(e => e.MinutiSospeso).HasColumnName("minuti_sospeso");
            entity.Property(e => e.OrdineId).HasColumnName("ordine_id");
            entity.Property(e => e.Totale)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("totale");
        });

        modelBuilder.Entity<VwStatisticheCache>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_statistiche_cache");

            entity.Property(e => e.DataAggiornamento)
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataRiferimento).HasColumnName("data_riferimento");
            entity.Property(e => e.FatturatoTotale)
                .HasMaxLength(4000)
                .HasColumnName("fatturato_totale");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.OrdiniAnnullati)
                .HasMaxLength(4000)
                .HasColumnName("ordini_annullati");
            entity.Property(e => e.OrdiniConsegnati)
                .HasMaxLength(4000)
                .HasColumnName("ordini_consegnati");
            entity.Property(e => e.Periodo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("periodo");
            entity.Property(e => e.TempoMedioMinuti)
                .HasMaxLength(4000)
                .HasColumnName("tempo_medio_minuti");
            entity.Property(e => e.TipoStatistica)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_statistica");
            entity.Property(e => e.TotaleOrdini)
                .HasMaxLength(4000)
                .HasColumnName("totale_ordini");
        });

        modelBuilder.Entity<VwStatisticheGiornaliere>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_statistiche_giornaliere");

            entity.Property(e => e.Data).HasColumnName("data");
            entity.Property(e => e.OrdiniAnnullati).HasColumnName("ordini_annullati");
            entity.Property(e => e.OrdiniConsegnati).HasColumnName("ordini_consegnati");
            entity.Property(e => e.TempoMedioCompletamentoMinuti).HasColumnName("tempo_medio_completamento_minuti");
            entity.Property(e => e.TotaleOrdini).HasColumnName("totale_ordini");
        });

        modelBuilder.Entity<VwStatisticheMensili>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_statistiche_mensili");

            entity.Property(e => e.Anno).HasColumnName("anno");
            entity.Property(e => e.FatturatoTotale)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("fatturato_totale");
            entity.Property(e => e.Mese).HasColumnName("mese");
            entity.Property(e => e.OrdiniAnnullati).HasColumnName("ordini_annullati");
            entity.Property(e => e.TempoMedioCompletamentoMinuti).HasColumnName("tempo_medio_completamento_minuti");
            entity.Property(e => e.TotaleOrdini).HasColumnName("totale_ordini");
        });

        modelBuilder.Entity<VwStatisticheOrdiniAvanzate>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_statistiche_ordini_avanzate");

            entity.Property(e => e.LivelloAllerta)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("livello_allerta");
            entity.Property(e => e.MessaggioAllerta)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("messaggio_allerta");
            entity.Property(e => e.MinutiInStato).HasColumnName("minuti_in_stato");
            entity.Property(e => e.OrdineId).HasColumnName("ordine_id");
            entity.Property(e => e.SogliaAttenzione).HasColumnName("soglia_attenzione");
            entity.Property(e => e.SogliaCritico).HasColumnName("soglia_critico");
            entity.Property(e => e.StatoOrdine)
                .HasMaxLength(50)
                .HasColumnName("stato_ordine");
        });

        modelBuilder.Entity<VwStatisticheRecenti>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_statistiche_recenti");

            entity.Property(e => e.DataAggiornamento)
                .HasColumnType("datetime")
                .HasColumnName("data_aggiornamento");
            entity.Property(e => e.DataRiferimento).HasColumnName("data_riferimento");
            entity.Property(e => e.FatturatoTotale)
                .HasMaxLength(4000)
                .HasColumnName("fatturato_totale");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.OrdiniAnnullati)
                .HasMaxLength(4000)
                .HasColumnName("ordini_annullati");
            entity.Property(e => e.OrdiniConsegnati)
                .HasMaxLength(4000)
                .HasColumnName("ordini_consegnati");
            entity.Property(e => e.Periodo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("periodo");
            entity.Property(e => e.TempoMedioMinuti)
                .HasMaxLength(4000)
                .HasColumnName("tempo_medio_minuti");
            entity.Property(e => e.TipoStatistica)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_statistica");
            entity.Property(e => e.TotaleOrdini)
                .HasMaxLength(4000)
                .HasColumnName("totale_ordini");
        });

        modelBuilder.Entity<VwTempiStato>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_tempi_stato");

            entity.Property(e => e.NumeroOrdini).HasColumnName("numero_ordini");
            entity.Property(e => e.StatoOrdine)
                .HasMaxLength(50)
                .HasColumnName("stato_ordine");
            entity.Property(e => e.TempoMassimoMinuti).HasColumnName("tempo_massimo_minuti");
            entity.Property(e => e.TempoMedioMinuti).HasColumnName("tempo_medio_minuti");
            entity.Property(e => e.TempoMinimoMinuti).HasColumnName("tempo_minimo_minuti");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
