// RepositoryTest/BaseTestClean.cs
using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace RepositoryTest
{
    public class BaseTestClean : IDisposable
    {
        protected readonly BubbleTeaContext _context;
        protected readonly IConfiguration _configuration;
        protected static ILogger<T> GetTestLogger<T>() where T : class
        {
            return NullLogger<T>.Instance;
        }

        public BaseTestClean()
        {
            // ✅ Configurazione minimale per InMemory
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.test.json", optional: false)
                .Build();

            // ✅ Database InMemory isolato per test clean
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"CleanTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            InitializeCleanDatabase();
        }

        private void InitializeCleanDatabase()
        {
            _context.Database.EnsureCreated();            
            SeedEssentialTables();
        }

        private void SeedEssentialTables()
        {
            var now = DateTime.UtcNow;            

            // 1. Unità di misura (per ingredienti, articoli, etc.)
            if (!_context.UnitaDiMisura.Any())
            {
                _context.UnitaDiMisura.AddRange(
                    new UnitaDiMisura
                    {
                        UnitaMisuraId = 1,
                        Sigla = "ML",
                        Descrizione = "Millilitri"
                    },
                    new UnitaDiMisura
                    {
                        UnitaMisuraId = 2,
                        Sigla = "GR",
                        Descrizione = "Grammi"
                    },
                    new UnitaDiMisura
                    {
                        UnitaMisuraId = 3,
                        Sigla = "PZ",
                        Descrizione = "Pezzi"
                    }
                );
            }

            // 2. Categorie ingredienti
            if (!_context.CategoriaIngrediente.Any())
            {
                _context.CategoriaIngrediente.AddRange(
                    new CategoriaIngrediente
                    {
                        CategoriaId = 1,
                        Categoria = "tea"
                    },
                    new CategoriaIngrediente
                    {
                        CategoriaId = 2,
                        Categoria = "latte"
                    },
                    new CategoriaIngrediente
                    {
                        CategoriaId = 3,
                        Categoria = "dolcificante"
                    },
                    new CategoriaIngrediente
                    {
                        CategoriaId = 4,
                        Categoria = "topping"
                    },
                    new CategoriaIngrediente
                    {
                        CategoriaId = 5,
                        Categoria = "aroma"
                    },
                    new CategoriaIngrediente
                    {
                        CategoriaId = 6,
                        Categoria = "speciale"
                    }
                );
            }

            // 3. TaxRates
            if (!_context.TaxRates.Any())
            {
                _context.TaxRates.AddRange(
                    new TaxRates
                    {
                        TaxRateId = 1,
                        Aliquota = 22.00m,
                        Descrizione = "IVA Standard",
                        DataCreazione = now,
                        DataAggiornamento = now
                    },
                    new TaxRates
                    {
                        TaxRateId = 2,
                        Aliquota = 10.00m,
                        Descrizione = "IVA Ridotta",
                        DataCreazione = now,
                        DataAggiornamento = now
                    }
                );
            }

            // 4. LogAttivita
            if (!_context.LogAttivita.Any())
            {
                _context.LogAttivita.AddRange(
                    new LogAttivita
                    {
                        LogId = 1,
                        TipoAttivita = "Sistema",
                        Descrizione = "Avvio applicazione",
                        DataEsecuzione = now.AddHours(-4),
                        Dettagli = "Sistema avviato correttamente",
                        UtenteId = null
                    },
                    new LogAttivita
                    {
                        LogId = 2,
                        TipoAttivita = "Database",
                        Descrizione = "Pulizia cache",
                        DataEsecuzione = now.AddHours(-3),
                        Dettagli = "Cache pulita automaticamente",
                        UtenteId = null
                    },
                    new LogAttivita
                    {
                        LogId = 3,
                        TipoAttivita = "Ordine",
                        Descrizione = "Nuovo ordine creato",
                        DataEsecuzione = now.AddHours(-2),
                        Dettagli = "Ordine #1 creato dal cliente",
                        UtenteId = 1
                    },
                    new LogAttivita
                    {
                        LogId = 4,
                        TipoAttivita = "Ordine",
                        Descrizione = "Stato ordine aggiornato",
                        DataEsecuzione = now.AddHours(-1),
                        Dettagli = "Ordine #1 passato in preparazione",
                        UtenteId = 2
                    },
                    new LogAttivita
                    {
                        LogId = 5,
                        TipoAttivita = "Sistema",
                        Descrizione = "Backup automatico",
                        DataEsecuzione = now.AddMinutes(-30),
                        Dettagli = "Backup database completato",
                        UtenteId = null
                    }
                );
            }

                // 5. Stati Ordine
            if (!_context.StatoOrdine.Any())
            {
                _context.StatoOrdine.AddRange(
                    new StatoOrdine
                    {
                        StatoOrdineId = 1,
                        StatoOrdine1 = "bozza",
                        Terminale = false
                    },
                    new StatoOrdine
                    {
                        StatoOrdineId = 2,
                        StatoOrdine1 = "in carrello",
                        Terminale = false
                    },
                    new StatoOrdine
                    {
                        StatoOrdineId = 3,
                        StatoOrdine1 = "in coda",
                        Terminale = false
                    },
                    new StatoOrdine
                    {
                        StatoOrdineId = 4,
                        StatoOrdine1 = "in preparazione",
                        Terminale = false
                    },
                    new StatoOrdine
                    {
                        StatoOrdineId = 5,
                        StatoOrdine1 = "pronta consegna",
                        Terminale = false
                    },
                    new StatoOrdine
                    {
                        StatoOrdineId = 6,
                        StatoOrdine1 = "consegnato",
                        Terminale = true
                    },
                    new StatoOrdine
                    {
                        StatoOrdineId = 7,
                        StatoOrdine1 = "sospeso",
                        Terminale = false
                    },
                    new StatoOrdine
                    {
                        StatoOrdineId = 8,
                        StatoOrdine1 = "annullato",
                        Terminale = true
                    }
                );
            }

            // 6. Stati pagamento (per Stripe/ordini)
            if (!_context.StatoPagamento.Any())
            {
                _context.StatoPagamento.AddRange(
                    new StatoPagamento
                    {
                        StatoPagamentoId = 1,
                        StatoPagamento1 = "non richiesto"
                    },
                    new StatoPagamento
                    {
                        StatoPagamentoId = 2,
                        StatoPagamento1 = "pendente"
                    },
                    new StatoPagamento
                    {
                        StatoPagamentoId = 3,
                        StatoPagamento1 = "completato"
                    },
                    new StatoPagamento
                    {
                        StatoPagamentoId = 4,
                        StatoPagamento1 = "fallito"
                    },
                    new StatoPagamento
                    {
                        StatoPagamentoId = 5,
                        StatoPagamento1 = "rimborsato"
                    }
                );
            }           

            // 7. DimensioneBicchiere
            if (!_context.DimensioneBicchiere.Any())
            {
                _context.DimensioneBicchiere.AddRange(
                    new DimensioneBicchiere
                    {
                        DimensioneBicchiereId = 1,
                        Sigla = "M",
                        Descrizione = "Medium",
                        Capienza = 500.00m,
                        UnitaMisuraId = 1,
                        PrezzoBase = 3.50m,
                        Moltiplicatore = 1.00m
                    },
                    new DimensioneBicchiere
                    {
                        DimensioneBicchiereId = 2,
                        Sigla = "L",
                        Descrizione = "Large",
                        Capienza = 700.00m,
                        UnitaMisuraId = 1,
                        PrezzoBase = 5.00m,
                        Moltiplicatore = 1.30m
                    }
                );               
            }

            // 9. Articoli (solo se vuoi dati di base per tutti i test)
            if (!_context.Articolo.Any())
            {
                _context.Articolo.AddRange(
                    new Articolo
                    {
                        ArticoloId = 1,
                        Tipo = "BC",
                        DataCreazione = now,
                        DataAggiornamento = now
                    },
                    new Articolo
                    {
                        ArticoloId = 2,
                        Tipo = "BS",
                        DataCreazione = now,
                        DataAggiornamento = now
                    },
                    new Articolo
                    {
                        ArticoloId = 3,
                        Tipo = "D",
                        DataCreazione = now,
                        DataAggiornamento = now
                    }
                );
            }

            // 10. Personalizzazioni
            if (!_context.Personalizzazione.Any())
            {
                _context.Personalizzazione.AddRange(
                    new Personalizzazione
                    {
                        PersonalizzazioneId = 1,
                        Nome = "classic milk tea",
                        Descrizione = "Preparazione tradizionale con tè nero e latte",
                        DtCreazione = now.AddHours(-5)
                    },
                    new Personalizzazione
                    {
                        PersonalizzazioneId = 2,
                        Nome = "taro milk tea",
                        Descrizione = "tea al latte di taro",
                        DtCreazione = now.AddHours(-4)
                    },
                    new Personalizzazione
                    {
                        PersonalizzazioneId = 3,
                        Nome = "matcha latte",
                        Descrizione = "bubble tea al matcha",
                        DtCreazione = now.AddHours(-3)
                    },
                    new Personalizzazione
                    {
                        PersonalizzazioneId = 4,
                        Nome = "fruit tea",
                        Descrizione = "tea alla frutta secca",
                        DtCreazione = now.AddHours(-2)
                    },
                    new Personalizzazione
                    {
                        PersonalizzazioneId = 5,
                        Nome = "brown sugar boba milk",
                        Descrizione = "latte con perle di zucchero bruno",
                        DtCreazione = now.AddHours(-1)
                    }
                );
            }

            // 12. PersonalizzazioniCustom
            if (!_context.PersonalizzazioneCustom.Any())
            {
                _context.PersonalizzazioneCustom.AddRange(
                    new PersonalizzazioneCustom
                    {
                        PersCustomId = 1,
                        Nome = "Bubble Tea Classico",
                        GradoDolcezza = 2,
                        DimensioneBicchiereId = 1, // Medium
                        DataCreazione = now,
                        DataAggiornamento = now
                    },
                    new PersonalizzazioneCustom
                    {
                        PersCustomId = 2,
                        Nome = "Bubble Tea Fruttato",
                        GradoDolcezza = 1,
                        DimensioneBicchiereId = 2, // Large
                        DataCreazione = now,
                        DataAggiornamento = now
                    }
                );
            }

            // 13. Utenti
            if (!_context.Utenti.Any())
            {
                _context.Utenti.AddRange(
                    new Utenti
                    {
                        UtenteId = 1, 
                        Email = "gestore@bubbleteazen.com",
                        PasswordHash = "$2a$10$N9qo8uLOickgx2ZMRZoMye3s3B9yX7U7Jq.7c6q8q7q6q8q7q6q8q7",
                        TipoUtente = "gestore",
                        Nome = "Mario",
                        Cognome = "Rossi",
                        Telefono = "+39123456789",
                        DataCreazione = now,
                        DataAggiornamento = now,
                        Attivo = true
                    },
                    new Utenti
                    {
                        UtenteId = 2, 
                        ClienteId = 1,
                        Email = "cliente@email.com",
                        PasswordHash = "$2a$10$N9qo8uLOickgx2ZMRZoMye3s3B9yX7U7Jq.7c6q8q7q6q8q7q6q8q7",
                        TipoUtente = "cliente",
                        Nome = "Luigi",
                        Cognome = "Verdi",
                        Telefono = "+39987654321",
                        DataCreazione = now,
                        DataAggiornamento = now,
                        Attivo = true
                    },
                    new Utenti
                    {
                        UtenteId = 3, 
                        TipoUtente = "guest",
                        SessioneGuest = Guid.NewGuid(),
                        DataCreazione = now,
                        DataAggiornamento = now,
                        Attivo = true
                    }
                );
            }
                    _context.SaveChanges();
        }

        // ✅ METODI HELPER PER TUTTI I TEST CLEAN        

        protected async Task CleanTableAsync<T>() where T : class
        {
            var entities = _context.Set<T>().ToList();
            if (entities.Count == 0)
                return;

            // ✅ GESTIONE DIPENDENZE PER ENTITY SPECIFICHE
            if (typeof(T) == typeof(UnitaDiMisura))
            {
                await CleanUnitaDiMisuraDependenciesAsync([.. entities.Cast<UnitaDiMisura>()]);
            }

            else if (typeof(T) == typeof(CategoriaIngrediente))
            {
                await CleanCategoriaIngredienteDependenciesAsync([.. entities.Cast<CategoriaIngrediente>()]);
            }

            else if (typeof(T) == typeof(LogAttivita))
            {
                await CleanLogAttivitaDependenciesAsync([.. entities.Cast<LogAttivita>()]);
            }

            else if (typeof(T) == typeof(DimensioneBicchiere))
            {
                await CleanDimensioneBicchiereDependenciesAsync([.. entities.Cast<DimensioneBicchiere>()]);
            }

            else if (typeof(T) == typeof(StatoOrdine))
            {
                await CleanStatoOrdineDependenciesAsync([.. entities.Cast<StatoOrdine>()]);
            }

            else if (typeof(T) == typeof(StatoPagamento))
            {
                await CleanStatoPagamentoDependenciesAsync([.. entities.Cast<StatoPagamento>()]);
            }

            else if (typeof(T) == typeof(ConfigSoglieTempi))
            {
                await CleanConfigSoglieTempiDependenciesAsync([.. entities.Cast<ConfigSoglieTempi>()]);
            }

            else if (typeof(T) == typeof(Ingrediente))
            {
                await CleanIngredienteDependenciesAsync([.. entities.Cast<Ingrediente>()]);
            }

            else if (typeof(T) == typeof(Articolo))
            {
                await CleanArticoloDependenciesAsync([.. entities.Cast<Articolo>()]);
            }

            else if (typeof(T) == typeof(Personalizzazione))
            {
                await CleanPersonalizzazioneDependenciesAsync([.. entities.Cast<Personalizzazione>()]);
            }

            else if (typeof(T) == typeof(PersonalizzazioneIngrediente))
            {
                await CleanPersonalizzazioneIngredienteDependenciesAsync([.. entities.Cast<PersonalizzazioneIngrediente>()]);
            }
            
            else if (typeof(T) == typeof(PersonalizzazioneCustom))
            {
                await CleanPersonalizzazioneCustomDependenciesAsync([.. entities.Cast<PersonalizzazioneCustom>()]);
            }

            // Aggiungi altri entity con dipendenze qui se necessario
            // else if (typeof(T) == typeof(AltraEntity)) { ... }

            _context.Set<T>().RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        protected async Task ResetDatabaseAsync()
        {
            // Pulisce tutte le tabelle in ordine inverso di dipendenza
            await CleanTableAsync<IngredientiPersonalizzazione>();
            await CleanTableAsync<PersonalizzazioneCustom>();
            await CleanTableAsync<DimensioneQuantitaIngredienti>();
            await CleanTableAsync<PersonalizzazioneIngrediente>();
            await CleanTableAsync<Personalizzazione>();
            await CleanTableAsync<Articolo>();
            await CleanTableAsync<DimensioneBicchiere>();
            await CleanTableAsync<Ingrediente>();
            await CleanTableAsync<StatoPagamento>();
            await CleanTableAsync<ConfigSoglieTempi>();
            await CleanTableAsync<StatoOrdine>();
            await CleanTableAsync<LogAttivita>();
            await CleanTableAsync<TaxRates>();
            await CleanTableAsync<CategoriaIngrediente>();            
            await CleanTableAsync<UnitaDiMisura>();
            await CleanTableAsync<Tavolo>();

            // Riempi con i dati essenziali
            SeedEssentialTables();
        }

        protected async Task CleanUnitaDiMisuraDependenciesAsync(List<UnitaDiMisura> unitaList)
        {
            if (unitaList.Count == 0) return;

            var unitaIds = unitaList.Select(u => u.UnitaMisuraId).ToList();

            // ✅ PRIMA: Trova tutte le DimensioneBicchiere che usano queste unità
            var dimensioniBicchiere = await _context.DimensioneBicchiere
                .Where(db => unitaIds.Contains(db.UnitaMisuraId))
                .ToListAsync();

            if (dimensioniBicchiere.Count != 0)
            {
                var dimensioneBicchiereIds = dimensioniBicchiere.Select(db => db.DimensioneBicchiereId).ToList();

                // ✅ SECONDA: Trova tutte le PersonalizzazioneCustom che usano queste dimensioni bicchiere
                var personalizzazioniCustom = await _context.PersonalizzazioneCustom
                    .Where(pc => dimensioneBicchiereIds.Contains(pc.DimensioneBicchiereId))
                    .ToListAsync();

                // ✅ TERZA: Rimuovi prima le PersonalizzazioneCustom
                if (personalizzazioniCustom.Count != 0)
                {
                    _context.PersonalizzazioneCustom.RemoveRange(personalizzazioniCustom);
                    await _context.SaveChangesAsync();
                }

                // ✅ QUARTA: Rimuovi le DimensioneBicchiere
                _context.DimensioneBicchiere.RemoveRange(dimensioniBicchiere);
                await _context.SaveChangesAsync();
            }

            // ✅ QUINTA: Le UnitaDiMisura verranno rimosse dal metodo chiamante
            // Non c'è bisogno di rimuoverle qui
        }

        protected async Task<LogAttivita> CreateTestLogWithUtenteAsync(string tipoAttivita = "Test", string tipoUtente = "Admin", string nome = "Test", string cognome = "Test")
        {
            // 1. Crea utente
            var utente = new Utenti
            {
                TipoUtente = tipoUtente,
                Nome = nome,
                Cognome = cognome,
                Email = $"{nome.ToLower()}.{cognome.ToLower()}@test.com",
                Attivo = true,
                DataCreazione = DateTime.UtcNow
            };

            _context.Utenti.Add(utente);
            await _context.SaveChangesAsync();

            // 2. Crea log con riferimento all'utente
            var log = new LogAttivita
            {
                TipoAttivita = tipoAttivita,
                Descrizione = $"Attività di test per {nome} {cognome}",
                DataEsecuzione = DateTime.UtcNow,
                UtenteId = utente.UtenteId
            };

            _context.LogAttivita.Add(log);
            await _context.SaveChangesAsync();

            // 3. Collega manualmente (per navigation property)
            log.Utente = utente;

            return log;
        }

        protected T CreateTestEntity<T>(Action<T>? configure = null) where T : class, new()
        {
            var entity = new T();
            configure?.Invoke(entity);
            _context.Set<T>().Add(entity);
            return entity;
        }

        protected async Task<T> CreateAndSaveTestEntityAsync<T>(Action<T>? configure = null) where T : class, new()
        {
            var entity = CreateTestEntity(configure);
            await _context.SaveChangesAsync();
            return entity;
        }

        protected async Task<Tavolo> CreateTestTavoloAsync(int numero = 99, bool disponibile = true, string zona = "Test")
        {
            var tavolo = new Tavolo
            {
                Numero = numero,
                Disponibile = disponibile,
                Zona = zona
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();
            return tavolo;
        }

        protected async Task<TaxRates> CreateTestTaxRateAsync(decimal aliquota = 22.00m, string descrizione = "IVA Test", DateTime? dataCreazione = null, DateTime? dataAggiornamento = null)
        {
            var taxRate = new TaxRates
            {
                Aliquota = aliquota,
                Descrizione = descrizione,
                DataCreazione = dataCreazione ?? DateTime.UtcNow,
                DataAggiornamento = dataAggiornamento ?? DateTime.UtcNow
            };

            _context.TaxRates.Add(taxRate);
            await _context.SaveChangesAsync();
            return taxRate;
        }

        // Dopo CreateTestTaxRateAsync, prima di Dispose()

        protected async Task<UnitaDiMisura> CreateTestUnitaDiMisuraAsync(
            string sigla = "TEST",
            string descrizione = "Unità di Test")
        {
            var unita = new UnitaDiMisura
            {
                Sigla = sigla,
                Descrizione = descrizione
            };

            _context.UnitaDiMisura.Add(unita);
            await _context.SaveChangesAsync();
            return unita;
        }

        #region CategoriaIngrediente Helpers

        protected async Task<CategoriaIngrediente> CreateTestCategoriaIngredienteAsync(string categoria = "TestCategoria", int categoriaId = 0)
        {
            var entity = new CategoriaIngrediente
            {
                Categoria = categoria
            };

            if (categoriaId > 0)
            {
                // Per test che richiedono ID specifico
                entity.CategoriaId = categoriaId;
            }

            _context.CategoriaIngrediente.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        protected async Task<List<CategoriaIngrediente>> CreateMultipleCategorieAsync(int count = 5)
        {
            var categorie = new List<CategoriaIngrediente>();

            for (int i = 1; i <= count; i++)
            {
                categorie.Add(new CategoriaIngrediente
                {
                    Categoria = $"Categoria Test {i}"
                });
            }

            _context.CategoriaIngrediente.AddRange(categorie);
            await _context.SaveChangesAsync();
            return categorie;
        }

        // Metodo per pulire le dipendenze di CategoriaIngrediente (Ingredienti collegati)
        protected async Task CleanCategoriaIngredienteDependenciesAsync(List<CategoriaIngrediente> categorie)
        {
            var categoriaIds = categorie.Select(c => c.CategoriaId).ToList();

            // Elimina Ingredienti collegati
            var ingredienti = await _context.Ingrediente
                .Where(i => categoriaIds.Contains(i.CategoriaId))
                .ToListAsync();

            if (ingredienti.Count > 0)
            {
                _context.Ingrediente.RemoveRange(ingredienti);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region LogAttivita Helpers

        protected async Task<LogAttivita> CreateTestLogAttivitaAsync(
            string tipoAttivita = "Test",
            string descrizione = "Attività di test",
            DateTime? dataEsecuzione = null,
            int? utenteId = null,
            string? dettagli = null)
        {
            var log = new LogAttivita
            {
                TipoAttivita = tipoAttivita,
                Descrizione = descrizione,
                DataEsecuzione = dataEsecuzione ?? DateTime.UtcNow,
                UtenteId = utenteId,
                Dettagli = dettagli
            };

            _context.LogAttivita.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }

        protected async Task<Utenti> CreateTestUtenteAsync(
            string tipoUtente = "Test",
            string nome = "Test",
            string cognome = "Utente",
            string email = "test@example.com")
        {
            var utente = new Utenti
            {
                TipoUtente = tipoUtente,
                Nome = nome,
                Cognome = cognome,
                Email = email,
                Attivo = true,
                DataCreazione = DateTime.UtcNow
            };

            _context.Utenti.Add(utente);
            await _context.SaveChangesAsync();
            return utente;
        }

        protected async Task<List<LogAttivita>> CreateMultipleLogAttivitaAsync(int count = 5, DateTime? baseDate = null)
        {
            var logs = new List<LogAttivita>();
            var baseDateTime = baseDate ?? DateTime.UtcNow.AddDays(-count);

            for (int i = 1; i <= count; i++)
            {
                logs.Add(new LogAttivita
                {
                    TipoAttivita = $"Tipo{i % 3 + 1}", // Cicla tra 3 tipi
                    Descrizione = $"Attività test {i}",
                    DataEsecuzione = baseDateTime.AddHours(i),
                    UtenteId = (i % 3) + 1, // Cicla tra utenti 1,2,3
                    Dettagli = i % 2 == 0 ? $"Dettagli aggiuntivi {i}" : null
                });
            }

            _context.LogAttivita.AddRange(logs);
            await _context.SaveChangesAsync();
            return logs;
        }

        protected async Task CleanLogAttivitaDependenciesAsync(List<LogAttivita> logs)
        {
            // I log non hanno dipendenze dirette da pulire
            // (non ci sono tabelle che hanno FK a LogAttivita)
            // Questo metodo è per consistenza con gli altri
            await Task.CompletedTask;
        }

        #endregion

        #region DimensioneBicchiere Helpers

    //    protected async Task<DimensioneBicchiere> CreateTestDimensioneBicchiereAsync(
    //string sigla = "TEST",
    //string descrizione = "Test Dimensione",
    //decimal capienza = 500.00m,
    //decimal prezzoBase = 3.50m,
    //decimal moltiplicatore = 1.00m)
    //    {
    //        // ✅ VERIFICA E CREA UNITA DI MISURA SE NECESSARIO
    //        var unita = await _context.UnitaDiMisura.FirstOrDefaultAsync();

    //        if (unita == null)
    //        {
    //            // Crea e salva prima l'unità di misura
    //            unita = new UnitaDiMisura
    //            {
    //                Sigla = "ML",
    //                Descrizione = "Millilitri"
    //            };
    //            _context.UnitaDiMisura.Add(unita);
    //            await _context.SaveChangesAsync(); // ✅ SALVA PRIMA DI USARE L'ID
    //        }

    //        var dimensione = new DimensioneBicchiere
    //        {
    //            Sigla = sigla,
    //            Descrizione = descrizione,
    //            Capienza = capienza,
    //            UnitaMisuraId = unita.UnitaMisuraId, // ✅ Ora l'ID esiste nel DB
    //            PrezzoBase = prezzoBase,
    //            Moltiplicatore = moltiplicatore
    //        };

    //        _context.DimensioneBicchiere.Add(dimensione);
    //        await _context.SaveChangesAsync();

    //        return dimensione;
    //    }

        protected async Task<List<DimensioneBicchiere>> CreateMultipleDimensioniAsync(int count = 3)
        {
            var dimensioni = new List<DimensioneBicchiere>();

            for (int i = 1; i <= count; i++)
            {
                dimensioni.Add(new DimensioneBicchiere
                {
                    Sigla = $"T{i}",
                    Descrizione = $"Test Dimensione {i}",
                    Capienza = 300.00m + (i * 100.00m),
                    UnitaMisuraId = 1, // ML
                    PrezzoBase = 2.50m + (i * 0.50m),
                    Moltiplicatore = 1.00m + (i * 0.15m)
                });
            }

            _context.DimensioneBicchiere.AddRange(dimensioni);
            await _context.SaveChangesAsync();
            return dimensioni;
        }
                
        // Metodo per pulire le dipendenze di DimensioneBicchiere
        protected async Task CleanDimensioneBicchiereDependenciesAsync(List<DimensioneBicchiere> dimensioni)
        {
            var dimensioneIds = dimensioni.Select(d => d.DimensioneBicchiereId).ToList();

            // 1. BevandaStandard collegati
            var bevandeStandard = await _context.BevandaStandard
                .Where(b => dimensioneIds.Contains(b.DimensioneBicchiereId))
                .ToListAsync();
            _context.BevandaStandard.RemoveRange(bevandeStandard);

            // 2. DimensioneQuantitaIngredienti collegati
            var quantitaIngredienti = await _context.DimensioneQuantitaIngredienti
                .Where(d => dimensioneIds.Contains(d.DimensioneBicchiereId))
                .ToListAsync();
            _context.DimensioneQuantitaIngredienti.RemoveRange(quantitaIngredienti);

            // 3. PersonalizzazioneCustom collegati
            var personalizzazioni = await _context.PersonalizzazioneCustom
                .Where(p => dimensioneIds.Contains(p.DimensioneBicchiereId))
                .ToListAsync();
            _context.PersonalizzazioneCustom.RemoveRange(personalizzazioni);

            // 4. PreferitiCliente collegati (correzione per int?)
            var preferiti = await _context.PreferitiCliente
                .Where(p => p.DimensioneBicchiereId.HasValue &&
                           dimensioneIds.Contains(p.DimensioneBicchiereId.Value)) // ✅ Converti int? a int
                .ToListAsync();
            _context.PreferitiCliente.RemoveRange(preferiti);

            if (bevandeStandard.Count > 0 || quantitaIngredienti.Count > 0 ||
                personalizzazioni.Count > 0 || preferiti.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        protected async Task SetupDimensioneBicchiereTestDataAsync()
        {
            // Pulisce e ricrea dati per test isolati
            await CleanTableAsync<DimensioneBicchiere>();

            // ✅ GARANTISCI che esista almeno un'unità di misura PRIMA di creare dimensioni
            if (!_context.UnitaDiMisura.Any())
            {
                var unita = new UnitaDiMisura
                {
                    Sigla = "ML",
                    Descrizione = "Millilitri"
                };
                _context.UnitaDiMisura.Add(unita);
                await _context.SaveChangesAsync(); // ✅ SALVA PRIMA
            }

            // Crea dati di test usando i metodi helper CON TUTTI I PARAMETRI
            await CreateTestDimensioneBicchiereAsync(
                sigla: "S",
                descrizione: "Small",
                capienza: 250.00m,
                unitaMisuraId: 1,  // ← OBBLIGATORIO: int, non decimal!
                prezzoBase: 2.50m,
                moltiplicatore: 0.85m);

            await CreateTestDimensioneBicchiereAsync(
                sigla: "M",
                descrizione: "Medium",
                capienza: 500.00m,
                unitaMisuraId: 1,  // ← int!
                prezzoBase: 3.50m,
                moltiplicatore: 1.00m);

            await CreateTestDimensioneBicchiereAsync(
                sigla: "L",
                descrizione: "Large",
                capienza: 750.00m,
                unitaMisuraId: 1,  // ← int!
                prezzoBase: 4.50m,
                moltiplicatore: 1.30m);
        }

        #endregion

        #region StatoOrdine Helpers

        protected async Task<StatoOrdine> CreateTestStatoOrdineAsync(
            string statoOrdine = "Test Stato",
            bool terminale = false,
            int? statoOrdineId = null)
        {
            var stato = new StatoOrdine
            {
                StatoOrdine1 = statoOrdine,
                Terminale = terminale
            };

            if (statoOrdineId.HasValue && statoOrdineId > 0)
            {
                stato.StatoOrdineId = statoOrdineId.Value;
            }

            _context.StatoOrdine.Add(stato);
            await _context.SaveChangesAsync();
            return stato;
        }

        protected async Task<List<StatoOrdine>> CreateMultipleStatiOrdineAsync(int count = 3, bool terminali = false)
        {
            var stati = new List<StatoOrdine>();

            for (int i = 1; i <= count; i++)
            {
                stati.Add(new StatoOrdine
                {
                    StatoOrdine1 = $"Stato Test {i}",
                    Terminale = terminali
                });
            }

            _context.StatoOrdine.AddRange(stati);
            await _context.SaveChangesAsync();
            return stati;
        }

        // Metodo per pulire le dipendenze di StatoOrdine
        protected async Task CleanStatoOrdineDependenciesAsync(List<StatoOrdine> stati)
        {
            var statoIds = stati.Select(s => s.StatoOrdineId).ToList();

            // 1. Elimina ConfigSoglieTempi collegati
            var configSoglie = await _context.ConfigSoglieTempi
                .Where(c => statoIds.Contains(c.StatoOrdineId))
                .ToListAsync();
            _context.ConfigSoglieTempi.RemoveRange(configSoglie);

            // 2. Elimina Ordine collegati
            var ordini = await _context.Ordine
                .Where(o => statoIds.Contains(o.StatoOrdineId))
                .ToListAsync();
            _context.Ordine.RemoveRange(ordini);

            // 3. Elimina StatoStoricoOrdine collegati
            var storico = await _context.StatoStoricoOrdine
                .Where(s => statoIds.Contains(s.StatoOrdineId))
                .ToListAsync();
            _context.StatoStoricoOrdine.RemoveRange(storico);

            if (configSoglie.Count > 0 || ordini.Count > 0 || storico.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        protected async Task SetupStatoOrdineTestDataAsync()
        {
            // Pulisce e ricrea dati per test isolati
            await CleanTableAsync<StatoOrdine>();

            // Crea gli 8 stati standard (come nel DB di produzione)
            await CreateTestStatoOrdineAsync("bozza", false, 1);
            await CreateTestStatoOrdineAsync("in carrello", false, 2);
            await CreateTestStatoOrdineAsync("in coda", false, 3);
            await CreateTestStatoOrdineAsync("in preparazione", false, 4);
            await CreateTestStatoOrdineAsync("pronta consegna", false, 5);
            await CreateTestStatoOrdineAsync("consegnato", true, 6);
            await CreateTestStatoOrdineAsync("sospeso", false, 7);
            await CreateTestStatoOrdineAsync("annullato", true, 8);
            
        }

        #endregion

        #region StatoPagamento Helpers

        protected async Task<StatoPagamento> CreateTestStatoPagamentoAsync(
            string statoPagamento = "Test Stato Pagamento",
            int? statoPagamentoId = null)
        {
            var stato = new StatoPagamento
            {
                StatoPagamento1 = statoPagamento
            };

            if (statoPagamentoId.HasValue && statoPagamentoId > 0)
            {
                stato.StatoPagamentoId = statoPagamentoId.Value;
            }

            _context.StatoPagamento.Add(stato);
            await _context.SaveChangesAsync();
            return stato;
        }

        protected async Task<List<StatoPagamento>> CreateMultipleStatiPagamentoAsync(int count = 3)
        {
            var stati = new List<StatoPagamento>();

            for (int i = 1; i <= count; i++)
            {
                stati.Add(new StatoPagamento
                {
                    StatoPagamento1 = $"Stato Pagamento Test {i}"
                });
            }

            _context.StatoPagamento.AddRange(stati);
            await _context.SaveChangesAsync();
            return stati;
        }

        // Metodo per pulire le dipendenze di StatoPagamento
        protected async Task CleanStatoPagamentoDependenciesAsync(List<StatoPagamento> stati)
        {
            var statoIds = stati.Select(s => s.StatoPagamentoId).ToList();

            // 1. Elimina Ordine collegati (unica dipendenza basata sul modello)
            var ordini = await _context.Ordine
                .Where(o => statoIds.Contains(o.StatoPagamentoId))
                .ToListAsync();
            _context.Ordine.RemoveRange(ordini);

            // Aggiungi altre dipendenze qui se emergono in futuro
            // Esempio: se ci fosse StatoStoricoPagamento, etc.

            if (ordini.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        protected async Task SetupStatoPagamentoTestDataAsync()
        {
            // Pulisce e ricrea dati per test isolati
            await CleanTableAsync<StatoPagamento>();

            // Crea i 5 stati standard (come nel DB di produzione)
            // USO I VALORI DEL DATABASE SQL - Conferma se sono corretti:
            await CreateTestStatoPagamentoAsync("non richiesto", 1);
            await CreateTestStatoPagamentoAsync("pendente", 2);
            await CreateTestStatoPagamentoAsync("completato", 3);
            await CreateTestStatoPagamentoAsync("fallito", 4);
            await CreateTestStatoPagamentoAsync("rimborsato", 5);
            
        }

        #endregion

        #region ConfigSoglieTempi Helpers

        protected async Task<ConfigSoglieTempi> CreateTestConfigSoglieTempiAsync(
            int statoOrdineId,
            int sogliaAttenzione = 30,
            int sogliaCritico = 60,
            string utenteAggiornamento = "testUser",
            DateTime? dataAggiornamento = null,
            int? sogliaId = null)
        {
            // Assicurati che lo StatoOrdine esista
            var statoOrdine = await _context.StatoOrdine
                .FirstOrDefaultAsync(s => s.StatoOrdineId == statoOrdineId);

            if (statoOrdine == null)
            {
                // Se lo stato non esiste, crealo usando l'ID fornito
                var nomeStato = statoOrdineId switch
                {
                    1 => "bozza",
                    2 => "in carrello",
                    3 => "in coda",
                    4 => "in preparazione",
                    5 => "pronta consegna",
                    6 => "consegnato",
                    7 => "sospeso",
                    8 => "annullato",
                    _ => $"Stato Test {statoOrdineId}"
                };

                var terminale = statoOrdineId switch
                {
                    6 => true,                    
                    8 => true,
                    _ => false
                };

                statoOrdine = new StatoOrdine
                {
                    StatoOrdineId = statoOrdineId,
                    StatoOrdine1 = nomeStato,
                    Terminale = terminale
                };
                _context.StatoOrdine.Add(statoOrdine);
                await _context.SaveChangesAsync();
            }

            var config = new ConfigSoglieTempi
            {
                StatoOrdineId = statoOrdineId,
                SogliaAttenzione = sogliaAttenzione,
                SogliaCritico = sogliaCritico,
                UtenteAggiornamento = utenteAggiornamento,
                DataAggiornamento = dataAggiornamento ?? DateTime.UtcNow
            };

            if (sogliaId.HasValue && sogliaId > 0)
            {
                config.SogliaId = sogliaId.Value;
            }

            _context.ConfigSoglieTempi.Add(config);
            await _context.SaveChangesAsync();
            
            config.StatoOrdine = statoOrdine;

            return config;
        }

        protected async Task<List<ConfigSoglieTempi>> CreateMultipleConfigSoglieTempiAsync(
            List<int> statoOrdineIds,
            int sogliaAttenzioneBase = 30,
            int sogliaCriticoBase = 60,
            int? sogliaIdStart = null)
        {
            var configs = new List<ConfigSoglieTempi>();

            // Verifica/crea tutti gli stati ordine
            foreach (var statoId in statoOrdineIds)
            {
                var stato = await _context.StatoOrdine
                    .FirstOrDefaultAsync(s => s.StatoOrdineId == statoId);

                if (stato == null)
                {
                    // Crea lo stato se non esiste
                    stato = new StatoOrdine
                    {
                        StatoOrdineId = statoId,
                        StatoOrdine1 = $"Stato Test {statoId}",
                        Terminale = false
                    };
                    _context.StatoOrdine.Add(stato);
                }
            }
            await _context.SaveChangesAsync();

            // Ora crea le configurazioni
            var statiOrdine = await _context.StatoOrdine
                .Where(s => statoOrdineIds.Contains(s.StatoOrdineId))
                .ToListAsync();

            for (int i = 0; i < statoOrdineIds.Count; i++)
            {
                var statoId = statoOrdineIds[i];
                var stato = statiOrdine.First(s => s.StatoOrdineId == statoId);

                var config = new ConfigSoglieTempi
                {
                    StatoOrdineId = statoId,
                    SogliaAttenzione = sogliaAttenzioneBase + (i * 10),
                    SogliaCritico = sogliaCriticoBase + (i * 20),
                    UtenteAggiornamento = $"testUser{i}",
                    DataAggiornamento = DateTime.UtcNow.AddMinutes(-i),
                    StatoOrdine = stato // Assegnazione diretta per i test
                };

                if (sogliaIdStart.HasValue)
                {
                    config.SogliaId = sogliaIdStart.Value + i;
                }

                configs.Add(config);
            }

            _context.ConfigSoglieTempi.AddRange(configs);
            await _context.SaveChangesAsync();
            return configs;
        }

        // Metodo per creare ConfigSoglieTempi con nuovo StatoOrdine (non usare ID predefiniti 1-8)
        protected async Task<ConfigSoglieTempi> CreateTestConfigSoglieTempiWithNewStatoOrdineAsync(
            string nomeStato = "Test Stato",
            bool terminale = false,
            int sogliaAttenzione = 30,
            int sogliaCritico = 60,
            string utenteAggiornamento = "testUser")
        {
            // Trova il prossimo ID disponibile
            var maxId = await _context.StatoOrdine.MaxAsync(s => (int?)s.StatoOrdineId) ?? 0;
            var newId = maxId + 1;

            // Crea un nuovo stato (non sovrascrive quelli predefiniti)
            var stato = new StatoOrdine
            {
                StatoOrdineId = newId,
                StatoOrdine1 = nomeStato,
                Terminale = terminale
            };

            _context.StatoOrdine.Add(stato);
            await _context.SaveChangesAsync();

            // Crea la configurazione
            return await CreateTestConfigSoglieTempiAsync(
                newId,
                sogliaAttenzione,
                sogliaCritico,
                utenteAggiornamento);
        }

        // Metodo per pulire le dipendenze di ConfigSoglieTempi
        protected async Task CleanConfigSoglieTempiDependenciesAsync(List<ConfigSoglieTempi> configs)
        {
            // ConfigSoglieTempi non ha dipendenze dirette verso altre tabelle
            // Rimuove solo le configurazioni stesse
            if (configs.Count != 0)
            {
                _context.ConfigSoglieTempi.RemoveRange(configs);
                await _context.SaveChangesAsync();
            }
        }

        // Setup dati di test per ConfigSoglieTempi usando gli stati predefiniti (ID 1-8)
        protected async Task SetupConfigSoglieTempiTestDataAsync()
        {
            // Pulisce la tabella
            await CleanTableAsync<ConfigSoglieTempi>();

            // Assicurati che ci siano gli 8 stati predefiniti
            if (!await _context.StatoOrdine.AnyAsync())
            {
                // Usa il codice fornito per creare gli 8 stati predefiniti
                var stati = new[]
                {
                    new StatoOrdine { StatoOrdineId = 1, StatoOrdine1 = "bozza", Terminale = false },
                    new StatoOrdine { StatoOrdineId = 2, StatoOrdine1 = "in carrello", Terminale = false },
                    new StatoOrdine { StatoOrdineId = 3, StatoOrdine1 = "in coda", Terminale = false },
                    new StatoOrdine { StatoOrdineId = 4, StatoOrdine1 = "in preparazione", Terminale = false },
                    new StatoOrdine { StatoOrdineId = 5, StatoOrdine1 = "pronta consegna", Terminale = false },
                    new StatoOrdine { StatoOrdineId = 6, StatoOrdine1 = "consegnato", Terminale = true },
                    new StatoOrdine { StatoOrdineId = 7, StatoOrdine1 = "sospeso", Terminale = false },
                    new StatoOrdine { StatoOrdineId = 8, StatoOrdine1 = "annullato", Terminale = true }
                };

                _context.StatoOrdine.AddRange(stati);
                await _context.SaveChangesAsync();
            }

            // ✅ Crea configurazioni solo per stati NON terminali
            // Stati non terminali nel vostro seed: 1, 2, 3, 4, 5, 7
            var statiNonTerminali = new[] { 1, 2, 3, 4 }; // Creiamo 4 configs come esempio

            for (int i = 0; i < statiNonTerminali.Length; i++)
            {
                var statoId = statiNonTerminali[i];
                await CreateTestConfigSoglieTempiAsync(
                    statoOrdineId: statoId,
                    sogliaAttenzione: 30 + (i * 15),   // 30, 45, 60, 75
                    sogliaCritico: 60 + (i * 30),      // 60, 90, 120, 150
                    utenteAggiornamento: "admin",
                    dataAggiornamento: DateTime.UtcNow.AddHours(-i)
                );
            }
        }

        // Metodo per validare un DTO ConfigSoglieTempi (utile per test di validazione)
        protected void AssertConfigSoglieTempiDTO(
            ConfigSoglieTempiDTO dto,
            int expectedSogliaId,
            int expectedStatoOrdineId,
            int expectedSogliaAttenzione,
            int expectedSogliaCritico,
            string? expectedUtenteAggiornamento = null,
            bool checkDate = false)
        {
            Assert.NotNull(dto);
            Assert.Equal(expectedSogliaId, dto.SogliaId);
            Assert.Equal(expectedStatoOrdineId, dto.StatoOrdineId);
            Assert.Equal(expectedSogliaAttenzione, dto.SogliaAttenzione);
            Assert.Equal(expectedSogliaCritico, dto.SogliaCritico);

            if (expectedUtenteAggiornamento != null)
            {
                Assert.Equal(expectedUtenteAggiornamento, dto.UtenteAggiornamento);
            }

            if (checkDate && dto.DataAggiornamento.HasValue)
            {
                // ⚠️ Usa TimeSpan per confronti sicuri senza warning
                var timeDifference = DateTime.UtcNow - dto.DataAggiornamento.Value;
                Assert.True(timeDifference.TotalSeconds >= -1,
                    $"DataAggiornamento ({dto.DataAggiornamento}) non può essere nel futuro (ora: {DateTime.UtcNow})");
            }
        }

        // Metodo per validare un'entità ConfigSoglieTempi
        protected void AssertConfigSoglieTempiEntity(
            ConfigSoglieTempi entity,
            int expectedStatoOrdineId,
            int expectedSogliaAttenzione,
            int expectedSogliaCritico,
            string? expectedUtenteAggiornamento = null)
        {
            Assert.NotNull(entity);
            Assert.Equal(expectedStatoOrdineId, entity.StatoOrdineId);
            Assert.Equal(expectedSogliaAttenzione, entity.SogliaAttenzione);
            Assert.Equal(expectedSogliaCritico, entity.SogliaCritico);

            if (expectedUtenteAggiornamento != null)
            {
                Assert.Equal(expectedUtenteAggiornamento, entity.UtenteAggiornamento);
            }

            Assert.NotNull(entity.DataAggiornamento);

            // ⚠️ Confronto sicuro con tolleranza di 1 secondo
            var maxAllowed = DateTime.UtcNow.AddSeconds(1);
            Assert.True(entity.DataAggiornamento <= maxAllowed,
                $"DataAggiornamento ({entity.DataAggiornamento}) non può essere nel futuro (max: {maxAllowed})");
        }

        #endregion

        #region Ingrediente Helpers

        // Metodo per creare un ingrediente di test
        protected async Task<Ingrediente> CreateTestIngredienteAsync(
            string nome = "Test Ingrediente",
            int categoriaId = 1,
            decimal prezzoAggiunto = 1.50m,
            bool disponibile = true,
            DateTime? dataInserimento = null,
            DateTime? dataAggiornamento = null,
            int? ingredienteId = null)
        {
            // Assicurati che la categoria esista
            var categoria = await _context.CategoriaIngrediente
                .FirstOrDefaultAsync(c => c.CategoriaId == categoriaId);

            if (categoria == null)
            {
                // Crea una categoria di test se non esiste
                categoria = new CategoriaIngrediente
                {
                    CategoriaId = categoriaId,
                    Categoria = categoriaId switch
                    {
                        1 => "tea",
                        2 => "latte",
                        3 => "dolcificante",
                        4 => "topping",
                        5 => "aroma",
                        6 => "speciale",
                        _ => $"Categoria Test {categoriaId}"
                    }
                };
                _context.CategoriaIngrediente.Add(categoria);
                await _context.SaveChangesAsync();
            }

            var ingrediente = new Ingrediente
            {
                Ingrediente1 = nome,
                CategoriaId = categoriaId,
                PrezzoAggiunto = prezzoAggiunto,
                Disponibile = disponibile,
                DataInserimento = dataInserimento ?? DateTime.UtcNow,
                DataAggiornamento = dataAggiornamento ?? DateTime.UtcNow
            };

            if (ingredienteId.HasValue && ingredienteId > 0)
            {
                ingrediente.IngredienteId = ingredienteId.Value;
            }

            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            // Imposta la navigazione per i test
            ingrediente.Categoria = categoria;

            return ingrediente;
        }

        // Metodo per creare più ingredienti
        protected async Task<List<Ingrediente>> CreateMultipleIngredientiAsync(
            List<(string nome, int categoriaId, decimal prezzo, bool disponibile)> ingredientiData,
            int? startId = null)
        {
            var ingredienti = new List<Ingrediente>();
            var idCounter = startId ?? 1;

            // Prima assicurati che le categorie esistano
            var categorieIds = ingredientiData.Select(d => d.categoriaId).Distinct().ToList();
            var categorieEsistenti = await _context.CategoriaIngrediente
                .Where(c => categorieIds.Contains(c.CategoriaId))
                .ToListAsync();

            foreach (var categoriaId in categorieIds)
            {
                if (!categorieEsistenti.Any(c => c.CategoriaId == categoriaId))
                {
                    var nuovaCategoria = new CategoriaIngrediente
                    {
                        CategoriaId = categoriaId,
                        Categoria = $"Categoria Test {categoriaId}"
                    };
                    _context.CategoriaIngrediente.Add(nuovaCategoria);
                    categorieEsistenti.Add(nuovaCategoria);
                }
            }
            await _context.SaveChangesAsync();

            // Ora crea gli ingredienti
            foreach (var (nome, categoriaId, prezzo, disponibile) in ingredientiData)
            {
                var categoria = categorieEsistenti.First(c => c.CategoriaId == categoriaId);

                var ingrediente = new Ingrediente
                {
                    IngredienteId = idCounter++,
                    Ingrediente1 = nome,
                    CategoriaId = categoriaId,
                    PrezzoAggiunto = prezzo,
                    Disponibile = disponibile,
                    DataInserimento = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                };

                ingredienti.Add(ingrediente);
            }

            _context.Ingrediente.AddRange(ingredienti);
            await _context.SaveChangesAsync();

            // Collega le categorie per i test
            foreach (var ingrediente in ingredienti)
            {
                ingrediente.Categoria = categorieEsistenti.First(c => c.CategoriaId == ingrediente.CategoriaId);
            }

            return ingredienti;
        }

        // Metodo per pulire le dipendenze di Ingrediente
        protected async Task CleanIngredienteDependenciesAsync(List<Ingrediente> ingredienti)
        {
            if (ingredienti.Count == 0)  // ✅ Count > 0 invece di Any()
                return;

            var ingredienteIds = ingredienti.Select(i => i.IngredienteId).ToList();

            // Pulisci IngredientiPersonalizzazione
            var ingredientiPers = await _context.IngredientiPersonalizzazione
                .Where(ip => ingredienteIds.Contains(ip.IngredienteId))
                .ToListAsync();

            if (ingredientiPers.Count > 0)  // ✅ Count > 0 invece di Any()
            {
                _context.IngredientiPersonalizzazione.RemoveRange(ingredientiPers);
            }

            // Pulisci PersonalizzazioneIngrediente
            var personalizzazioni = await _context.PersonalizzazioneIngrediente
                .Where(pi => ingredienteIds.Contains(pi.IngredienteId))
                .ToListAsync();

            if (personalizzazioni.Count > 0)  // ✅ Count > 0 invece di Any()
            {
                _context.PersonalizzazioneIngrediente.RemoveRange(personalizzazioni);
            }

            await _context.SaveChangesAsync();
        }

        // Setup dati di test per Ingrediente
        protected async Task SetupIngredienteTestDataAsync()
        {
            // Pulisce la tabella
            await CleanTableAsync<Ingrediente>();

            // Crea 5 ingredienti di test con categorie diverse
            var ingredientiData = new List<(string, int, decimal, bool)>
            {
                ("Tea Nero Premium", 1, 0.50m, true),
                ("Latte di Cocco", 2, 0.80m, true),
                ("Sciroppo di Caramello", 3, 1.50m, true),
                ("Perle di Tapioca", 4, 1.20m, false), // Non disponibile
                ("Aroma di Vaniglia", 5, 0.30m, true)
            };

            await CreateMultipleIngredientiAsync(ingredientiData, 1);
        }

        // Metodo per validare un DTO Ingrediente
        protected void AssertIngredienteDTO(
            IngredienteDTO dto,
            int expectedIngredienteId,
            string expectedNome,
            int expectedCategoriaId,
            decimal expectedPrezzoAggiunto,
            bool expectedDisponibile,
            bool checkDates = false)
        {
            Assert.NotNull(dto);
            Assert.Equal(expectedIngredienteId, dto.IngredienteId);
            Assert.Equal(expectedNome, dto.Nome);
            Assert.Equal(expectedCategoriaId, dto.CategoriaId);
            Assert.Equal(expectedPrezzoAggiunto, dto.PrezzoAggiunto);
            Assert.Equal(expectedDisponibile, dto.Disponibile);

            if (checkDates)
            {
                // ⚠️ Usa TimeSpan per confronti sicuri senza warning
                Assert.NotEqual(DateTime.MinValue, dto.DataInserimento);
                Assert.NotEqual(DateTime.MinValue, dto.DataAggiornamento);

                var inserimentoDiff = DateTime.UtcNow - dto.DataInserimento;
                Assert.True(inserimentoDiff.TotalSeconds >= -1,
                    $"DataInserimento non può essere nel futuro: {dto.DataInserimento}");

                var aggiornamentoDiff = DateTime.UtcNow - dto.DataAggiornamento;
                Assert.True(aggiornamentoDiff.TotalSeconds >= -1,
                    $"DataAggiornamento non può essere nel futuro: {dto.DataAggiornamento}");
            }
        }

        // Metodo per validare un'entità Ingrediente
        protected void AssertIngredienteEntity(
            Ingrediente entity,
            string expectedNome,
            int expectedCategoriaId,
            decimal expectedPrezzoAggiunto,
            bool expectedDisponibile)
        {
            Assert.NotNull(entity);
            Assert.Equal(expectedNome, entity.Ingrediente1);
            Assert.Equal(expectedCategoriaId, entity.CategoriaId);
            Assert.Equal(expectedPrezzoAggiunto, entity.PrezzoAggiunto);
            Assert.Equal(expectedDisponibile, entity.Disponibile);
            Assert.NotEqual(DateTime.MinValue, entity.DataInserimento);
            Assert.NotEqual(DateTime.MinValue, entity.DataAggiornamento);
        }

        // Helper per creare DTO di test
        protected IngredienteDTO CreateTestIngredienteDTO(
            string nome = "Test Ingrediente DTO",
            int categoriaId = 1,
            decimal prezzoAggiunto = 1.00m,
            bool disponibile = true)
        {
            return new IngredienteDTO
            {
                IngredienteId = 0, // Per create
                Nome = nome,
                CategoriaId = categoriaId,
                PrezzoAggiunto = prezzoAggiunto,
                Disponibile = disponibile,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
        }

        // Metodo per garantire che un Ingrediente esista
        protected async Task EnsureIngredienteExistsAsync(int ingredienteId)
        {
            var existing = await _context.Ingrediente
                .AnyAsync(i => i.IngredienteId == ingredienteId);

            if (!existing)
            {
                // Crea un ingrediente con l'ID specificato
                var ingrediente = new Ingrediente
                {
                    IngredienteId = ingredienteId,
                    Ingrediente1 = $"Ingrediente Test {ingredienteId}",
                    CategoriaId = 1, // default
                    PrezzoAggiunto = 1.00m,
                    Disponibile = true,
                    DataInserimento = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                };
                _context.Ingrediente.Add(ingrediente);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Articolo Helpers

        protected async Task<Articolo> CreateTestArticoloAsync(string tipo = "BC", int? articoloId = null, DateTime? dataCreazione = null)
        {
            var now = DateTime.UtcNow;

            var articolo = new Articolo
            {
                Tipo = tipo,
                DataCreazione = dataCreazione ?? now,
                DataAggiornamento = dataCreazione ?? now
            };

            if (articoloId.HasValue && articoloId.Value > 0)
            {
                // Per test che richiedono ID specifico
                articolo.ArticoloId = articoloId.Value;
            }

            _context.Articolo.Add(articolo);
            await _context.SaveChangesAsync();
            return articolo;
        }
        
        protected async Task<List<Articolo>> CreateAllTipiArticoliAsync()
        {
            var now = DateTime.UtcNow;
            var articoli = new List<Articolo>
            {
                new() { Tipo = "BC", DataCreazione = now, DataAggiornamento = now },
                new() { Tipo = "BS", DataCreazione = now.AddMinutes(1), DataAggiornamento = now.AddMinutes(1) },
                new() { Tipo = "D", DataCreazione = now.AddMinutes(2), DataAggiornamento = now.AddMinutes(2) }
            };

            _context.Articolo.AddRange(articoli);
            await _context.SaveChangesAsync();
            return articoli;
        }
        
        protected async Task<List<Articolo>> CreateMultipleArticoliAsync(string tipo, int count = 5)
        {
            var articoli = new List<Articolo>();
            var now = DateTime.UtcNow;

            for (int i = 0; i < count; i++)
            {
                articoli.Add(new Articolo
                {
                    Tipo = tipo,
                    DataCreazione = now.AddMinutes(i),
                    DataAggiornamento = now.AddMinutes(i)
                });
            }

            _context.Articolo.AddRange(articoli);
            await _context.SaveChangesAsync();
            return articoli;
        }
        
        protected async Task CleanArticoloDependenciesAsync(List<Articolo> articoli)
        {
            var articoloIds = articoli.Select(a => a.ArticoloId).ToList();

            // OrderItem
            var orderItems = await _context.OrderItem
                .Where(oi => articoloIds.Contains(oi.ArticoloId))
                .ToListAsync();

            if (orderItems.Count > 0)
            {
                _context.OrderItem.RemoveRange(orderItems);
            }

            // BevandaCustom
            var bevandeCustom = await _context.BevandaCustom
                .Where(bc => articoloIds.Contains(bc.ArticoloId))
                .ToListAsync();

            if (bevandeCustom.Count > 0)
            {
                _context.BevandaCustom.RemoveRange(bevandeCustom);
            }

            // BevandaStandard
            var bevandeStandard = await _context.BevandaStandard
                .Where(bs => articoloIds.Contains(bs.ArticoloId))
                .ToListAsync();

            if (bevandeStandard.Count > 0)
            {
                _context.BevandaStandard.RemoveRange(bevandeStandard);
            }

            // Dolce
            var dolci = await _context.Dolce
                .Where(d => articoloIds.Contains(d.ArticoloId))
                .ToListAsync();

            if (dolci.Count > 0)
            {
                _context.Dolce.RemoveRange(dolci);
            }

            await _context.SaveChangesAsync();
        }
        
        protected async Task<bool> ArticoloHasDependenciesAsync(int articoloId)
        {
            bool hasOrderItem = await _context.OrderItem
                .AnyAsync(oi => oi.ArticoloId == articoloId);

            bool hasBevandaCustom = await _context.BevandaCustom
                .AnyAsync(bc => bc.ArticoloId == articoloId);

            bool hasBevandaStandard = await _context.BevandaStandard
                .AnyAsync(bs => bs.ArticoloId == articoloId);

            bool hasDolce = await _context.Dolce
                .AnyAsync(d => d.ArticoloId == articoloId);

            return hasOrderItem || hasBevandaCustom || hasBevandaStandard || hasDolce;
        }
        
        protected void AssertDateTimeWithTolerance(DateTime expected, DateTime actual, int toleranceSeconds = 2)
        {
            var timeDifference = Math.Abs((expected - actual).TotalSeconds);
            Assert.True(timeDifference <= toleranceSeconds,
                $"Date differiscono di {timeDifference} secondi (tolleranza: {toleranceSeconds}s). " +
                $"Expected: {expected:yyyy-MM-dd HH:mm:ss}, Actual: {actual:yyyy-MM-dd HH:mm:ss}");
        }
        
        protected void AssertArticoliEqual(Articolo expected, Articolo actual, bool ignoreDates = true)
        {
            Assert.Equal(expected.ArticoloId, actual.ArticoloId);
            Assert.Equal(expected.Tipo, actual.Tipo);

            if (!ignoreDates)
            {
                AssertDateTimeWithTolerance(expected.DataCreazione, actual.DataCreazione);
                AssertDateTimeWithTolerance(expected.DataAggiornamento, actual.DataAggiornamento);
            }
        }

        #endregion

        #region Personalizzazione Helpers
        
        protected async Task<Personalizzazione> CreateTestPersonalizzazioneAsync(
            string nome = "TEST",
            string descrizione = "Descrizione di test",
            int? personalizzazioneId = null,
            DateTime? dataCreazione = null)
        {
            var now = DateTime.UtcNow;

            var personalizzazione = new Personalizzazione
            {
                Nome = nome.ToUpper(), // Il repository converte in maiuscolo
                Descrizione = descrizione,
                DtCreazione = dataCreazione ?? now
            };

            if (personalizzazioneId.HasValue && personalizzazioneId.Value > 0)
            {
                // Per test che richiedono ID specifico
                personalizzazione.PersonalizzazioneId = personalizzazioneId.Value;
            }

            _context.Personalizzazione.Add(personalizzazione);
            await _context.SaveChangesAsync();
            return personalizzazione;
        }
        
        protected async Task<List<Personalizzazione>> CreateMultiplePersonalizzazioniAsync(int count = 5)
        {
            var personalizzazioni = new List<Personalizzazione>();
            var now = DateTime.UtcNow;
            var baseNome = "PERSONALIZZAZIONE";

            for (int i = 1; i <= count; i++)
            {
                personalizzazioni.Add(new Personalizzazione
                {
                    Nome = $"{baseNome} {i}",
                    Descrizione = $"Descrizione personalizzazione {i}",
                    DtCreazione = now.AddMinutes(i)
                });
            }

            _context.Personalizzazione.AddRange(personalizzazioni);
            await _context.SaveChangesAsync();
            return personalizzazioni;
        }
        
        protected async Task CleanPersonalizzazioneDependenciesAsync(List<Personalizzazione> personalizzazioni)
        {
            var personalizzazioneIds = personalizzazioni.Select(p => p.PersonalizzazioneId).ToList();

            // BevandaStandard
            var bevandeStandard = await _context.BevandaStandard
                .Where(bs => personalizzazioneIds.Contains(bs.PersonalizzazioneId))
                .ToListAsync();

            if (bevandeStandard.Count > 0)
            {
                _context.BevandaStandard.RemoveRange(bevandeStandard);
            }

            // PersonalizzazioneIngrediente
            var personalizzazioneIngredienti = await _context.PersonalizzazioneIngrediente
                .Where(pi => personalizzazioneIds.Contains(pi.PersonalizzazioneId))
                .ToListAsync();

            if (personalizzazioneIngredienti.Count > 0)
            {
                _context.PersonalizzazioneIngrediente.RemoveRange(personalizzazioneIngredienti);
            }

            await _context.SaveChangesAsync();
        }
        
        protected async Task<bool> PersonalizzazioneHasDependenciesAsync(int personalizzazioneId)
        {
            bool hasBevandaStandard = await _context.BevandaStandard
                .AnyAsync(bs => bs.PersonalizzazioneId == personalizzazioneId);

            bool hasPersonalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                .AnyAsync(pi => pi.PersonalizzazioneId == personalizzazioneId);

            return hasBevandaStandard || hasPersonalizzazioneIngrediente;
        }
        
        protected void AssertPersonalizzazioniEqual(Personalizzazione expected, Personalizzazione actual, bool ignoreId = false, bool ignoreDate = true)
        {
            if (!ignoreId)
            {
                Assert.Equal(expected.PersonalizzazioneId, actual.PersonalizzazioneId);
            }

            Assert.Equal(expected.Nome, actual.Nome);
            Assert.Equal(expected.Descrizione, actual.Descrizione);

            if (!ignoreDate)
            {
                AssertDateTimeWithTolerance(expected.DtCreazione, actual.DtCreazione);
            }
        }
        
        protected void AssertPersonalizzazioneDTOEqual(PersonalizzazioneDTO expected, PersonalizzazioneDTO actual, bool ignoreId = false, bool ignoreDate = true)
        {
            if (!ignoreId)
            {
                Assert.Equal(expected.PersonalizzazioneId, actual.PersonalizzazioneId);
            }

            Assert.Equal(expected.Nome, actual.Nome);
            Assert.Equal(expected.Descrizione, actual.Descrizione);

            if (!ignoreDate)
            {
                AssertDateTimeWithTolerance(expected.DtCreazione, actual.DtCreazione);
            }
        }
        
        protected async Task<Personalizzazione> CreateDuplicatePersonalizzazioneAsync(string nome = "DUPLICATO")
        {
            var now = DateTime.UtcNow;

            // Prima crea una personalizzazione con il nome
            var prima = new Personalizzazione
            {
                Nome = nome.ToUpper(),
                Descrizione = "Prima personalizzazione",
                DtCreazione = now
            };

            _context.Personalizzazione.Add(prima);
            await _context.SaveChangesAsync();

            return prima;
        }
        
        protected async Task<List<Personalizzazione>> CreateCaseVariationsAsync(string baseNome = "Test")
        {
            var variations = new List<Personalizzazione>
            {
                new() { Nome = baseNome.ToUpper(), Descrizione = "Tutto maiuscolo", DtCreazione = DateTime.UtcNow },
                new() { Nome = baseNome.ToLower(), Descrizione = "Tutto minuscolo", DtCreazione = DateTime.UtcNow.AddMinutes(1) },
                new() { Nome = char.ToUpper(baseNome[0]) + baseNome.Substring(1).ToLower(), Descrizione = "Prima maiuscola", DtCreazione = DateTime.UtcNow.AddMinutes(2) }
            };

            _context.Personalizzazione.AddRange(variations);
            await _context.SaveChangesAsync();
            return variations;
        }

        #endregion

        #region PersonalizzazioneIngrediente Helpers

        protected async Task<PersonalizzazioneIngrediente> CreateTestPersonalizzazioneIngredienteAsync(
            int personalizzazioneId = 1,
            int ingredienteId = 1,
            decimal quantita = 100.00m,
            int unitaMisuraId = 1,
            int? personalizzazioneIngredienteId = null)
        {
            var personalizzazioneIngrediente = new PersonalizzazioneIngrediente
            {
                PersonalizzazioneId = personalizzazioneId,
                IngredienteId = ingredienteId,
                Quantita = quantita,
                UnitaMisuraId = unitaMisuraId
            };

            if (personalizzazioneIngredienteId.HasValue && personalizzazioneIngredienteId.Value > 0)
            {
                personalizzazioneIngrediente.PersonalizzazioneIngredienteId = personalizzazioneIngredienteId.Value;
            }

            _context.PersonalizzazioneIngrediente.Add(personalizzazioneIngrediente);
            await _context.SaveChangesAsync();
            return personalizzazioneIngrediente;
        }

        protected async Task<List<PersonalizzazioneIngrediente>> CreateMultiplePersonalizzazioneIngredientiAsync(int count = 3)
        {
            var personalizzazioni = await CreateMultiplePersonalizzazioniAsync(count);
            var personalizzazioneIngredienti = new List<PersonalizzazioneIngrediente>();
            var now = DateTime.UtcNow;

            // Creiamo ingredienti di test se non ci sono abbastanza
            var ingredientiEsistenti = await _context.Ingrediente.CountAsync();
            if (ingredientiEsistenti < count)
            {
                for (int i = ingredientiEsistenti + 1; i <= count; i++)
                {
                    _context.Ingrediente.Add(new Ingrediente
                    {
                        IngredienteId = 100 + i, // ID alti per non sovrascrivere quelli di seed
                        Ingrediente1 = $"Ingrediente Test {i}",
                        CategoriaId = 1,
                        PrezzoAggiunto = 1.00m,
                        Disponibile = true,
                        DataInserimento = now,
                        DataAggiornamento = now
                    });
                }
                await _context.SaveChangesAsync();
            }

            var ingredienti = await _context.Ingrediente
                .Take(count)
                .ToListAsync();

            for (int i = 0; i < count; i++)
            {
                personalizzazioneIngredienti.Add(new PersonalizzazioneIngrediente
                {
                    PersonalizzazioneId = personalizzazioni[i].PersonalizzazioneId,
                    IngredienteId = ingredienti[i].IngredienteId,
                    Quantita = (i + 1) * 50.00m, // 50, 100, 150, ...
                    UnitaMisuraId = 1 // ML
                });
            }

            _context.PersonalizzazioneIngrediente.AddRange(personalizzazioneIngredienti);
            await _context.SaveChangesAsync();
            return personalizzazioneIngredienti;
        }

        protected async Task CleanPersonalizzazioneIngredienteDependenciesAsync(List<PersonalizzazioneIngrediente> personalizzazioneIngredienti)
        {
            var personalizzazioneIngredienteIds = personalizzazioneIngredienti.Select(pi => pi.PersonalizzazioneIngredienteId).ToList();

            // DimensioneQuantitaIngredienti
            var dimensioniQuantita = await _context.DimensioneQuantitaIngredienti
                .Where(dqi => personalizzazioneIngredienteIds.Contains(dqi.PersonalizzazioneIngredienteId))
                .ToListAsync();

            if (dimensioniQuantita.Count > 0)
            {
                _context.DimensioneQuantitaIngredienti.RemoveRange(dimensioniQuantita);
                await _context.SaveChangesAsync();
            }
        }

        protected async Task<bool> PersonalizzazioneIngredienteHasDependenciesAsync(int personalizzazioneIngredienteId)
        {
            return await _context.DimensioneQuantitaIngredienti
                .AnyAsync(dqi => dqi.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);
        }

        protected void AssertPersonalizzazioneIngredienteEqual(PersonalizzazioneIngrediente expected, PersonalizzazioneIngrediente actual, bool ignoreId = false)
        {
            if (!ignoreId)
            {
                Assert.Equal(expected.PersonalizzazioneIngredienteId, actual.PersonalizzazioneIngredienteId);
            }

            Assert.Equal(expected.PersonalizzazioneId, actual.PersonalizzazioneId);
            Assert.Equal(expected.IngredienteId, actual.IngredienteId);
            Assert.Equal(expected.Quantita, actual.Quantita);
            Assert.Equal(expected.UnitaMisuraId, actual.UnitaMisuraId);
        }

        protected void AssertPersonalizzazioneIngredienteDTOEqual(PersonalizzazioneIngredienteDTO expected, PersonalizzazioneIngredienteDTO actual, bool ignoreId = false)
        {
            if (!ignoreId)
            {
                Assert.Equal(expected.PersonalizzazioneIngredienteId, actual.PersonalizzazioneIngredienteId);
            }

            Assert.Equal(expected.PersonalizzazioneId, actual.PersonalizzazioneId);
            Assert.Equal(expected.IngredienteId, actual.IngredienteId);
            Assert.Equal(expected.Quantita, actual.Quantita);
            Assert.Equal(expected.UnitaMisuraId, actual.UnitaMisuraId);
        }

        protected async Task<PersonalizzazioneIngrediente> CreateDuplicatePersonalizzazioneIngredienteAsync(int personalizzazioneId, int ingredienteId)
        {
            var existing = new PersonalizzazioneIngrediente
            {
                PersonalizzazioneId = personalizzazioneId,
                IngredienteId = ingredienteId,
                Quantita = 100.00m,
                UnitaMisuraId = 1
            };

            _context.PersonalizzazioneIngrediente.Add(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        protected PersonalizzazioneIngredienteDTO CreateTestPersonalizzazioneIngredienteDTO(
    int personalizzazioneId = 1,
    int ingredienteId = 1,
    decimal quantita = 100.00m,
    int unitaMisuraId = 1)
        {
            return new PersonalizzazioneIngredienteDTO
            {
                PersonalizzazioneId = personalizzazioneId,
                IngredienteId = ingredienteId,
                Quantita = quantita,
                UnitaMisuraId = unitaMisuraId
            };
        }

        protected async Task<Ingrediente> CreateTestIngredienteAsync(
            string nome = "Test Ingrediente",
            int categoriaId = 1,
            decimal prezzoAggiunto = 1.00m,
            bool disponibile = true,
            int? ingredienteId = null)
        {
            var now = DateTime.UtcNow;
            var ingrediente = new Ingrediente
            {
                Ingrediente1 = nome,
                CategoriaId = categoriaId,
                PrezzoAggiunto = prezzoAggiunto,
                Disponibile = disponibile,
                DataInserimento = now,
                DataAggiornamento = now
            };

            if (ingredienteId.HasValue && ingredienteId.Value > 0)
            {
                ingrediente.IngredienteId = ingredienteId.Value;
            }

            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();
            return ingrediente;
        }

        #endregion

        #region DimensioneQuantitaIngredienti Helpers

        protected async Task<DimensioneQuantitaIngredienti> CreateTestDimensioneQuantitaIngredientiAsync(
            int personalizzazioneIngredienteId = 1,
            int dimensioneBicchiereId = 1,
            decimal moltiplicatore = 1.5m,
            int? dimensioneId = null)
        {
            // Creiamo le entità dipendenti se non esistono
            await EnsurePersonalizzazioneIngredienteExistsAsync(personalizzazioneIngredienteId);
            await EnsureDimensioneBicchiereExistsAsync(dimensioneBicchiereId);

            var dimensioneQuantita = new DimensioneQuantitaIngredienti
            {
                PersonalizzazioneIngredienteId = personalizzazioneIngredienteId,
                DimensioneBicchiereId = dimensioneBicchiereId,
                Moltiplicatore = moltiplicatore
            };

            if (dimensioneId.HasValue && dimensioneId.Value > 0)
            {
                dimensioneQuantita.DimensioneId = dimensioneId.Value;
            }

            _context.DimensioneQuantitaIngredienti.Add(dimensioneQuantita);
            await _context.SaveChangesAsync();
            return dimensioneQuantita;
        }

        protected async Task<List<DimensioneQuantitaIngredienti>> CreateMultipleDimensioneQuantitaIngredientiAsync(int count = 3)
        {
            var dimensioniQuantita = new List<DimensioneQuantitaIngredienti>();

            // Creiamo personalizzazioni ingredienti di test
            var personalizzazioneIngredienti = await CreateMultiplePersonalizzazioneIngredientiAsync(count);

            // Usiamo le dimensioni bicchiere esistenti (dal seed)
            var dimensioniBicchiere = await _context.DimensioneBicchiere
                .Take(count)
                .ToListAsync();

            // Se non ci sono abbastanza dimensioni, creiamo quelle mancanti
            for (int i = dimensioniBicchiere.Count; i < count; i++)
            {
                var nuovaDimensione = await CreateTestDimensioneBicchiereAsync(
                    sigla: $"T{i}", // T per "Test"
                    descrizione: $"Bicchiere Test {i}",
                    dimensioneBicchiereId: 100 + i // ID alti per non sovrascrivere il seed
                );
                dimensioniBicchiere.Add(nuovaDimensione);
            }

            for (int i = 0; i < count; i++)
            {
                var dimensioneQuantita = new DimensioneQuantitaIngredienti
                {
                    PersonalizzazioneIngredienteId = personalizzazioneIngredienti[i].PersonalizzazioneIngredienteId,
                    DimensioneBicchiereId = dimensioniBicchiere[i].DimensioneBicchiereId,
                    Moltiplicatore = (i + 1) * 0.5m // 0.5, 1.0, 1.5, ...
                };

                dimensioniQuantita.Add(dimensioneQuantita);
            }

            _context.DimensioneQuantitaIngredienti.AddRange(dimensioniQuantita);
            await _context.SaveChangesAsync();
            return dimensioniQuantita;
        }

        // ✅ MANTIENI SOLO QUESTO (quello con 7 parametri)
        protected async Task<DimensioneBicchiere> CreateTestDimensioneBicchiereAsync(
            string sigla = "M",
            string descrizione = "medium",
            decimal capienza = 500m,
            int unitaMisuraId = 2,
            decimal prezzoBase = 5.00m,
            decimal moltiplicatore = 2.0m,
            int? dimensioneBicchiereId = null)
        {
            var dimensioneBicchiere = new DimensioneBicchiere
            {
                Sigla = sigla,
                Descrizione = descrizione,
                Capienza = capienza,
                UnitaMisuraId = unitaMisuraId,  // <-- IMPORTANTE: questo mancava nell'altra versione
                PrezzoBase = prezzoBase,
                Moltiplicatore = moltiplicatore,
            };

            if (dimensioneBicchiereId.HasValue && dimensioneBicchiereId.Value > 0)
            {
                dimensioneBicchiere.DimensioneBicchiereId = dimensioneBicchiereId.Value;
            }

            _context.DimensioneBicchiere.Add(dimensioneBicchiere);
            await _context.SaveChangesAsync();
            return dimensioneBicchiere;
        }

        protected async Task<DimensioneBicchiere> GetSeedDimensioneBicchiereAsync(int seedId = 1)
        {
            // Ritorna una dimensione bicchiere dal seed (1: Medium, 2: Large)
            var dimensione = await _context.DimensioneBicchiere
                .FirstOrDefaultAsync(db => db.DimensioneBicchiereId == seedId);

            return dimensione ?? throw new InvalidOperationException(
                    $"DimensioneBicchiere con ID {seedId} non trovata nel seed. " +
                    "Assicurati che il seed sia stato eseguito prima dei test.");
        }

        protected async Task EnsurePersonalizzazioneIngredienteExistsAsync(int personalizzazioneIngredienteId)
        {
            var existing = await _context.PersonalizzazioneIngrediente
                .AnyAsync(pi => pi.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);

            if (!existing)
            {
                await CreateTestPersonalizzazioneIngredienteAsync(personalizzazioneIngredienteId: personalizzazioneIngredienteId);
            }
        }

        protected async Task EnsureDimensioneBicchiereExistsAsync(int dimensioneBicchiereId)
        {
            var existing = await _context.DimensioneBicchiere
                .AnyAsync(db => db.DimensioneBicchiereId == dimensioneBicchiereId);

            if (!existing)
            {
                // Se l'ID è 1 o 2, usiamo i valori del seed
                if (dimensioneBicchiereId == 1 || dimensioneBicchiereId == 2)
                {
                    await GetSeedDimensioneBicchiereAsync(dimensioneBicchiereId);
                }
                else
                {
                    await CreateTestDimensioneBicchiereAsync(
                        dimensioneBicchiereId: dimensioneBicchiereId,
                        sigla: $"T{dimensioneBicchiereId}",
                        descrizione: $"Bicchiere Test {dimensioneBicchiereId}"
                    );
                }
            }
        }

        protected void AssertDimensioneQuantitaIngredientiEqual(DimensioneQuantitaIngredienti expected, DimensioneQuantitaIngredienti actual, bool ignoreId = false)
        {
            if (!ignoreId)
            {
                Assert.Equal(expected.DimensioneId, actual.DimensioneId);
            }

            Assert.Equal(expected.PersonalizzazioneIngredienteId, actual.PersonalizzazioneIngredienteId);
            Assert.Equal(expected.DimensioneBicchiereId, actual.DimensioneBicchiereId);
            Assert.Equal(expected.Moltiplicatore, actual.Moltiplicatore);
        }

        protected void AssertDimensioneQuantitaIngredientiDTOEqual(DimensioneQuantitaIngredientiDTO expected, DimensioneQuantitaIngredientiDTO actual, bool ignoreId = false)
        {
            if (!ignoreId)
            {
                Assert.Equal(expected.DimensioneId, actual.DimensioneId);
            }

            Assert.Equal(expected.PersonalizzazioneIngredienteId, actual.PersonalizzazioneIngredienteId);
            Assert.Equal(expected.DimensioneBicchiereId, actual.DimensioneBicchiereId);
            Assert.Equal(expected.Moltiplicatore, actual.Moltiplicatore);
        }

        protected async Task<DimensioneQuantitaIngredienti> CreateDuplicateDimensioneQuantitaIngredientiAsync(
            int personalizzazioneIngredienteId = 1,
            int dimensioneBicchiereId = 1)
        {
            await EnsurePersonalizzazioneIngredienteExistsAsync(personalizzazioneIngredienteId);
            await EnsureDimensioneBicchiereExistsAsync(dimensioneBicchiereId);

            var existing = new DimensioneQuantitaIngredienti
            {
                PersonalizzazioneIngredienteId = personalizzazioneIngredienteId,
                DimensioneBicchiereId = dimensioneBicchiereId,
                Moltiplicatore = 1.0m
            };

            _context.DimensioneQuantitaIngredienti.Add(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        protected DimensioneQuantitaIngredientiDTO CreateTestDimensioneQuantitaIngredientiDTO(
            int personalizzazioneIngredienteId = 1,
            int dimensioneBicchiereId = 1,
            decimal moltiplicatore = 1.5m)
        {
            return new DimensioneQuantitaIngredientiDTO
            {
                PersonalizzazioneIngredienteId = personalizzazioneIngredienteId,
                DimensioneBicchiereId = dimensioneBicchiereId,
                Moltiplicatore = moltiplicatore
            };
        }

        protected async Task CleanAllTestDimensioneQuantitaIngredientiAsync()
        {
            // Rimuove solo i record con ID alti (creati per test)
            var allTestDimensioni = await _context.DimensioneQuantitaIngredienti
                .Where(d => d.DimensioneId >= 1000 ||
                           (d.PersonalizzazioneIngredienteId >= 1000 && d.PersonalizzazioneIngredienteId < 2000) ||
                           (d.DimensioneBicchiereId >= 1000 && d.DimensioneBicchiereId < 2000))
                .ToListAsync();

            if (allTestDimensioni.Count > 0)
            {
                _context.DimensioneQuantitaIngredienti.RemoveRange(allTestDimensioni);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region PersonalizzazioneCustom Helpers

        protected async Task<PersonalizzazioneCustom> CreateTestPersonalizzazioneCustomAsync(
    string nome = "Test Personalizzazione",
    byte gradoDolcezza = 2,
    int dimensioneBicchiereId = 1,
    DateTime? dataCreazione = null,
    DateTime? dataAggiornamento = null,
    int? persCustomId = null)
        {
            // ✅ Assicurati che la dimensione bicchiere esista
            await EnsureDimensioneBicchiereExistsAsync(dimensioneBicchiereId);

            var now = DateTime.UtcNow;

            // Se non viene specificato un ID, generane uno alto (>= 1000) per evitare conflitti col seed
            if (!persCustomId.HasValue)
            {
                // Trova il massimo ID esistente
                var maxId = await _context.PersonalizzazioneCustom
                    .Select(p => (int?)p.PersCustomId)
                    .DefaultIfEmpty()
                    .MaxAsync();

                // ✅ Corretto: usa .GetValueOrDefault() invece di .Value per evitare warning
                persCustomId = (maxId.GetValueOrDefault()) < 1000 ? 1000 : maxId.GetValueOrDefault() + 1;
            }

            var personalizzazione = new PersonalizzazioneCustom
            {
                PersCustomId = persCustomId.Value,
                Nome = nome,
                GradoDolcezza = gradoDolcezza,
                DimensioneBicchiereId = dimensioneBicchiereId,
                DataCreazione = dataCreazione ?? now,
                DataAggiornamento = dataAggiornamento ?? now
            };

            _context.PersonalizzazioneCustom.Add(personalizzazione);
            await _context.SaveChangesAsync();
            return personalizzazione;
        }

        protected async Task<List<PersonalizzazioneCustom>> CreateMultiplePersonalizzazioneCustomAsync(int count = 3)
        {
            var personalizzazioni = new List<PersonalizzazioneCustom>();

            // ✅ Usiamo le dimensioni bicchiere esistenti (dal seed)
            var dimensioniBicchiere = await _context.DimensioneBicchiere
                .Take(count)
                .ToListAsync();

            // ✅ Se non ci sono abbastanza dimensioni, creiamo quelle mancanti
            for (int i = dimensioniBicchiere.Count; i < count; i++)
            {
                var nuovaDimensione = await CreateTestDimensioneBicchiereAsync(
                    sigla: $"T{i}",
                    descrizione: $"Bicchiere Test {i}",
                    dimensioneBicchiereId: 100 + i
                );
                dimensioniBicchiere.Add(nuovaDimensione);
            }

            var now = DateTime.UtcNow;
            for (int i = 0; i < count; i++)
            {
                var personalizzazione = new PersonalizzazioneCustom
                {
                    Nome = $"Personalizzazione Test {i + 1}",
                    GradoDolcezza = (byte)((i % 3) + 1), // 1, 2, 3 ciclico
                    DimensioneBicchiereId = dimensioniBicchiere[i].DimensioneBicchiereId,
                    DataCreazione = now.AddMinutes(i),
                    DataAggiornamento = now.AddMinutes(i)
                };

                personalizzazioni.Add(personalizzazione);
            }

            _context.PersonalizzazioneCustom.AddRange(personalizzazioni);
            await _context.SaveChangesAsync();
            return personalizzazioni;
        }

        protected async Task<PersonalizzazioneCustom> GetSeedPersonalizzazioneCustomAsync(int seedId = 1)
        {
            // ✅ Ritorna una personalizzazione dal seed (1: Classico, 2: Fruttato)
            var personalizzazione = await _context.PersonalizzazioneCustom
                .FirstOrDefaultAsync(p => p.PersCustomId == seedId);

            return personalizzazione ?? throw new InvalidOperationException(
                    $"PersonalizzazioneCustom con ID {seedId} non trovata nel seed. " +
                    "Assicurati che il seed sia stato eseguito prima dei test.");
        }

        protected async Task EnsurePersonalizzazioneCustomExistsAsync(int persCustomId)
        {
            var existing = await _context.PersonalizzazioneCustom
                .AnyAsync(p => p.PersCustomId == persCustomId);

            if (!existing)
            {
                await CreateTestPersonalizzazioneCustomAsync(persCustomId: persCustomId);
            }
        }

        // ✅ Assert con tolleranza per date (evita warning compilazione)
        protected void AssertPersonalizzazioneCustomEqual(PersonalizzazioneCustom expected, PersonalizzazioneCustom actual, bool ignoreId = false)
        {
            if (!ignoreId)
            {
                Assert.Equal(expected.PersCustomId, actual.PersCustomId);
            }

            Assert.Equal(expected.Nome, actual.Nome);
            Assert.Equal(expected.GradoDolcezza, actual.GradoDolcezza);
            Assert.Equal(expected.DimensioneBicchiereId, actual.DimensioneBicchiereId);

            // ✅ Confronto date con tolleranza (1 secondo) per evitare warning
            Assert.Equal(expected.DataCreazione, actual.DataCreazione, TimeSpan.FromSeconds(1));
            Assert.Equal(expected.DataAggiornamento, actual.DataAggiornamento, TimeSpan.FromSeconds(1));
        }

        // ✅ Assert per DTO con tolleranza date
        protected void AssertPersonalizzazioneCustomDTOEqual(PersonalizzazioneCustomDTO expected, PersonalizzazioneCustomDTO actual, bool ignoreId = false)
        {
            if (!ignoreId)
            {
                Assert.Equal(expected.PersCustomId, actual.PersCustomId);
            }

            Assert.Equal(expected.Nome, actual.Nome);
            Assert.Equal(expected.GradoDolcezza, actual.GradoDolcezza);
            Assert.Equal(expected.DimensioneBicchiereId, actual.DimensioneBicchiereId);
            Assert.Equal(expected.DataCreazione, actual.DataCreazione, TimeSpan.FromSeconds(1));
            Assert.Equal(expected.DataAggiornamento, actual.DataAggiornamento, TimeSpan.FromSeconds(1));
        }

        // ✅ Crea DTO di test senza dipendenze InMemory
        protected PersonalizzazioneCustomDTO CreateTestPersonalizzazioneCustomDTO(
            string nome = "Test Personalizzazione DTO",
            byte gradoDolcezza = 2,
            int dimensioneBicchiereId = 1)
        {
            var now = DateTime.UtcNow;
            return new PersonalizzazioneCustomDTO
            {
                Nome = nome,
                GradoDolcezza = gradoDolcezza,
                DimensioneBicchiereId = dimensioneBicchiereId,
                DataCreazione = now,
                DataAggiornamento = now
            };
        }

        // ✅ Crea DTO con DescrizioneBicchiere (utile per test di query join)
        protected PersonalizzazioneCustomDTO CreateTestPersonalizzazioneCustomDTOWithDescrizione(
            string nome = "Test Personalizzazione DTO",
            byte gradoDolcezza = 2,
            int dimensioneBicchiereId = 1,
            string descrizioneBicchiere = "Medium")
        {
            var now = DateTime.UtcNow;
            return new PersonalizzazioneCustomDTO
            {
                Nome = nome,
                GradoDolcezza = gradoDolcezza,
                DimensioneBicchiereId = dimensioneBicchiereId,
                DescrizioneBicchiere = descrizioneBicchiere,
                DataCreazione = now,
                DataAggiornamento = now
            };
        }

        // ✅ Pulizia completa per test PersonalizzazioneCustom
        protected async Task CleanAllTestPersonalizzazioneCustomAsync()
        {
            // Identifica le personalizzazioni di test (ID alti o con dimensioni bicchiere di test)
            var testPersonalizzazioni = await _context.PersonalizzazioneCustom
                .Where(p => p.PersCustomId >= 1000 ||
                           (p.DimensioneBicchiereId >= 1000 && p.DimensioneBicchiereId < 2000))
                .ToListAsync();

            if (testPersonalizzazioni.Count != 0)
            {
                // Pulisci prima le dipendenze
                await CleanPersonalizzazioneCustomDependenciesAsync(testPersonalizzazioni);

                // Pulisci le personalizzazioni stesse
                _context.PersonalizzazioneCustom.RemoveRange(testPersonalizzazioni);
                await _context.SaveChangesAsync();
            }

            // Pulisci dimensioni bicchiere di test (create appositamente per test)
            var dimensioniBicchiereTest = await _context.DimensioneBicchiere
                .Where(db => db.DimensioneBicchiereId >= 1000 && db.DimensioneBicchiereId < 2000)
                .ToListAsync();

            if (dimensioniBicchiereTest.Count != 0)
            {
                _context.DimensioneBicchiere.RemoveRange(dimensioniBicchiereTest);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Verifica se una PersonalizzazioneCustom ha dipendenze nel database di test
        protected async Task<bool> HasDependenciesForPersonalizzazioneCustomAsync(int persCustomId)
        {
            bool hasBevandaCustom = await _context.BevandaCustom
                .AnyAsync(bc => bc.PersCustomId == persCustomId);

            bool hasIngredientiPersonalizzazione = await _context.IngredientiPersonalizzazione
                .AnyAsync(ip => ip.PersCustomId == persCustomId);

            return hasBevandaCustom || hasIngredientiPersonalizzazione;
        }

        // ✅ Helper per creare dipendenze (BevandaCustom e IngredientiPersonalizzazione) per test di HasDependencies
        protected async Task CreateDependenciesForPersonalizzazioneCustomAsync(int persCustomId)
        {
            // ✅ Crea un ingrediente temporaneo per il test (ID alto per non confliggere)
            var ingredienteTest = new Ingrediente
            {
                IngredienteId = persCustomId + 1000, // ID unico per test
                Ingrediente1 = $"Ingrediente Test {persCustomId}",
                CategoriaId = 1, // tea dal seed
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };

            _context.Ingrediente.Add(ingredienteTest);
            await _context.SaveChangesAsync();

            // ✅ Usa Articolo dal seed (ID 1) o creane uno se non esiste (ma nel seed c'è)
            var articolo = await _context.Articolo.FindAsync(1);
            if (articolo == null)
            {
                articolo = new Articolo
                {
                    ArticoloId = persCustomId + 2000, // ID alto per test
                    Tipo = "BC",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                };
                _context.Articolo.Add(articolo);
                await _context.SaveChangesAsync();
            }

            // ✅ Crea una BevandaCustom collegata
            var bevandaCustom = new BevandaCustom
            {
                ArticoloId = articolo.ArticoloId,
                PersCustomId = persCustomId,
                Prezzo = 5.00m,
                DataCreazione = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };

            // ✅ Crea un IngredientiPersonalizzazione collegato
            var ingredientePersonalizzazione = new IngredientiPersonalizzazione
            {
                PersCustomId = persCustomId,
                IngredienteId = ingredienteTest.IngredienteId,
                DataCreazione = DateTime.UtcNow
            };

            _context.BevandaCustom.Add(bevandaCustom);
            _context.IngredientiPersonalizzazione.Add(ingredientePersonalizzazione);
            await _context.SaveChangesAsync();
        }

        // ✅ Pulizia delle dipendenze di PersonalizzazioneCustom
        protected async Task CleanPersonalizzazioneCustomDependenciesAsync(List<PersonalizzazioneCustom> personalizzazioni)
        {
            if (personalizzazioni.Count == 0) return;

            var persCustomIds = personalizzazioni.Select(p => p.PersCustomId).ToList();

            // ✅ BevandaCustom - rimuove quelle collegate alle personalizzazioni
            var bevandeCustom = await _context.BevandaCustom
                .Where(bc => persCustomIds.Contains(bc.PersCustomId))
                .ToListAsync();

            if (bevandeCustom.Count != 0)
            {
                _context.BevandaCustom.RemoveRange(bevandeCustom);
                await _context.SaveChangesAsync();
            }

            // ✅ IngredientiPersonalizzazione - rimuove quelle collegate
            var ingredientiPersonalizzazione = await _context.IngredientiPersonalizzazione
                .Where(ip => persCustomIds.Contains(ip.PersCustomId))
                .ToListAsync();

            if (ingredientiPersonalizzazione.Count != 0)
            {
                _context.IngredientiPersonalizzazione.RemoveRange(ingredientiPersonalizzazione);
                await _context.SaveChangesAsync();
            }

            // ✅ NON puliamo Articoli e Ingredienti del seed, solo quelli di test (ID alti)
            var articoliTestIds = personalizzazioni.Select(p => 1000 + p.PersCustomId).ToList();
            var ingredientiTestIds = personalizzazioni.Select(p => 2000 + p.PersCustomId).ToList();

            // Rimuovi articoli di test
            var articoliTest = await _context.Articolo
                .Where(a => articoliTestIds.Contains(a.ArticoloId))
                .ToListAsync();

            if (articoliTest.Count != 0)
            {
                _context.Articolo.RemoveRange(articoliTest);
                await _context.SaveChangesAsync();
            }

            // Rimuovi ingredienti di test
            var ingredientiTest = await _context.Ingrediente
                .Where(i => ingredientiTestIds.Contains(i.IngredienteId))
                .ToListAsync();

            if (ingredientiTest.Count != 0)
            {
                _context.Ingrediente.RemoveRange(ingredientiTest);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region IngredientiPersonalizzazione Helpers        

        protected async Task<IngredientiPersonalizzazione> CreateTestIngredientiPersonalizzazioneAsync(
            int persCustomId = 1,
            int ingredienteId = 1,
            DateTime? dataCreazione = null,
            int? ingredientePersId = null)
        {
            // ✅ Usa i metodi esistenti per garantire che le entità parent esistano
            await EnsurePersonalizzazioneCustomExistsAsync(persCustomId);
            await EnsureIngredienteExistsAsync(ingredienteId);

            // Se non viene specificato un ID, generane uno alto (>= 1000) per evitare conflitti col seed
            if (!ingredientePersId.HasValue)
            {
                var maxId = await _context.IngredientiPersonalizzazione
                    .Select(ip => (int?)ip.IngredientePersId)
                    .DefaultIfEmpty()
                    .MaxAsync();

                ingredientePersId = (maxId.GetValueOrDefault()) < 1000 ? 1000 : maxId.GetValueOrDefault() + 1;
            }

            var ingredientePers = new IngredientiPersonalizzazione
            {
                IngredientePersId = ingredientePersId.Value,
                PersCustomId = persCustomId,
                IngredienteId = ingredienteId,
                DataCreazione = dataCreazione ?? DateTime.UtcNow
            };

            _context.IngredientiPersonalizzazione.Add(ingredientePers);
            await _context.SaveChangesAsync();
            return ingredientePers;
        }

        protected async Task<List<IngredientiPersonalizzazione>> CreateMultipleIngredientiPersonalizzazioneAsync(int count = 3)
        {
            var ingredientiPersonalizzazioni = new List<IngredientiPersonalizzazione>();

            // ✅ Usa personalizzazioni esistenti dal seed
            var personalizzazioni = await _context.PersonalizzazioneCustom
                .Take(count)
                .ToListAsync();

            // Se non ci sono abbastanza personalizzazioni, usa il seed
            for (int i = personalizzazioni.Count; i < count; i++)
            {
                // Usa il metodo esistente CreateTestPersonalizzazioneCustomAsync
                var nuovaPersonalizzazione = await CreateTestPersonalizzazioneCustomAsync(
                    nome: $"Test Personalizzazione {i}",
                    persCustomId: 1000 + i
                );
                personalizzazioni.Add(nuovaPersonalizzazione);
            }

            // ✅ Usa ingredienti esistenti dal seed
            var ingredienti = await _context.Ingrediente
                .Take(count)
                .ToListAsync();

            // Se non ci sono abbastanza ingredienti, usa il seed o creane di nuovi
            for (int i = ingredienti.Count; i < count; i++)
            {
                // Usa il metodo esistente CreateTestIngredienteAsync (senza parametri o con quelli di default)
                var nuovoIngrediente = new Ingrediente
                {
                    IngredienteId = 1000 + i,
                    Ingrediente1 = $"Ingrediente Test {i}",
                    CategoriaId = 1, // Usa una categoria esistente dal seed
                    PrezzoAggiunto = 1.00m,
                    Disponibile = true,
                    DataInserimento = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                };
                _context.Ingrediente.Add(nuovoIngrediente);
                await _context.SaveChangesAsync();
                ingredienti.Add(nuovoIngrediente);
            }

            var now = DateTime.UtcNow;
            for (int i = 0; i < count; i++)
            {
                var ingredientePers = new IngredientiPersonalizzazione
                {
                    PersCustomId = personalizzazioni[i].PersCustomId,
                    IngredienteId = ingredienti[i].IngredienteId,
                    DataCreazione = now.AddMinutes(i)
                };

                ingredientiPersonalizzazioni.Add(ingredientePers);
            }

            _context.IngredientiPersonalizzazione.AddRange(ingredientiPersonalizzazioni);
            await _context.SaveChangesAsync();
            return ingredientiPersonalizzazioni;
        }

        protected async Task<IngredientiPersonalizzazione> GetSeedIngredientiPersonalizzazioneAsync()
        {
            // ✅ Ritorna un IngredientiPersonalizzazione dal seed (se esiste)
            var ingredientePers = await _context.IngredientiPersonalizzazione
                .FirstOrDefaultAsync();

            return ingredientePers ?? throw new InvalidOperationException(
                    "Nessuna IngredientiPersonalizzazione trovata nel seed. " +
                    "Assicurati che il seed sia stato eseguito prima dei test.");
        }

        protected async Task EnsureIngredientiPersonalizzazioneExistsAsync(int ingredientePersId)
        {
            var existing = await _context.IngredientiPersonalizzazione
                .AnyAsync(ip => ip.IngredientePersId == ingredientePersId);

            if (!existing)
            {
                await CreateTestIngredientiPersonalizzazioneAsync(ingredientePersId: ingredientePersId);
            }
        }

        // ✅ Assert con tolleranza per date
        protected void AssertIngredientiPersonalizzazioneEqual(IngredientiPersonalizzazione expected, IngredientiPersonalizzazione actual, bool ignoreId = false)
        {
            if (!ignoreId)
            {
                Assert.Equal(expected.IngredientePersId, actual.IngredientePersId);
            }

            Assert.Equal(expected.PersCustomId, actual.PersCustomId);
            Assert.Equal(expected.IngredienteId, actual.IngredienteId);
            Assert.Equal(expected.DataCreazione, actual.DataCreazione, TimeSpan.FromSeconds(1));
        }

        // ✅ Assert per DTO con tolleranza date
        protected void AssertIngredientiPersonalizzazioneDTOEqual(IngredientiPersonalizzazioneDTO expected, IngredientiPersonalizzazioneDTO actual, bool ignoreId = false)
        {
            if (!ignoreId)
            {
                Assert.Equal(expected.IngredientePersId, actual.IngredientePersId);
            }

            Assert.Equal(expected.PersCustomId, actual.PersCustomId);
            Assert.Equal(expected.IngredienteId, actual.IngredienteId);
            Assert.Equal(expected.DataCreazione, actual.DataCreazione, TimeSpan.FromSeconds(1));

            if (expected.NomePersonalizzazione != null)
                Assert.Equal(expected.NomePersonalizzazione, actual.NomePersonalizzazione);

            if (expected.NomeIngrediente != null)
                Assert.Equal(expected.NomeIngrediente, actual.NomeIngrediente);
        }

        // ✅ Crea DTO di test senza dipendenze InMemory
        protected IngredientiPersonalizzazioneDTO CreateTestIngredientiPersonalizzazioneDTO(
            int persCustomId = 1,
            int ingredienteId = 1)
        {
            var now = DateTime.UtcNow;
            return new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = persCustomId,
                IngredienteId = ingredienteId,
                DataCreazione = now
            };
        }

        // ✅ Crea DTO con nomi personalizzazione e ingrediente
        protected IngredientiPersonalizzazioneDTO CreateTestIngredientiPersonalizzazioneDTOWithNames(
            int persCustomId = 1,
            string nomePersonalizzazione = "Test Personalizzazione",
            int ingredienteId = 1,
            string nomeIngrediente = "Test Ingrediente")
        {
            var now = DateTime.UtcNow;
            return new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = persCustomId,
                NomePersonalizzazione = nomePersonalizzazione,
                IngredienteId = ingredienteId,
                NomeIngrediente = nomeIngrediente,
                DataCreazione = now
            };
        }

        #endregion

        public void Dispose()
        {
            _context?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}