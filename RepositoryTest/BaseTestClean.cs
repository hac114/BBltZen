// RepositoryTest/BaseTestClean.cs
using BBltZen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                        Terminale = true
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

            // 7. Ingrediente
            if (!_context.Ingrediente.Any())
            {
                _context.Ingrediente.AddRange(
                    new Ingrediente
                    {
                        IngredienteId = 1,
                        Ingrediente1 = "Tea nero premium",
                        CategoriaId = 1,
                        PrezzoAggiunto = 0.50m,
                        Disponibile = true,
                        DataInserimento = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    },
                    new Ingrediente
                    {
                        IngredienteId = 2,
                        Ingrediente1 = "Tea verde special",
                        CategoriaId = 1,
                        PrezzoAggiunto = 0.45m,
                        Disponibile = true,
                        DataInserimento = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    },
                    new Ingrediente
                    {
                        IngredienteId = 3,
                        Ingrediente1 = "Sciroppo di caramello",
                        CategoriaId = 3,
                        PrezzoAggiunto = 1.50m,
                        Disponibile = true,
                        DataInserimento = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    },
                    new Ingrediente
                    {
                        IngredienteId = 4,
                        Ingrediente1 = "Perle di tapioca",
                        CategoriaId = 4,
                        PrezzoAggiunto = 1.20m,
                        Disponibile = true,
                        DataInserimento = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    },
                    new Ingrediente
                    {
                        IngredienteId = 5,
                        Ingrediente1 = "Latte di cocco",
                        CategoriaId = 2,
                        PrezzoAggiunto = 0.80m,
                        Disponibile = true,
                        DataInserimento = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                );
            }

            // 8. DimensioneBicchiere
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


            // 9. Utenti
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

        private async Task CleanUnitaDiMisuraDependenciesAsync(List<UnitaDiMisura> unitaList)
        {
            var unitaIds = unitaList.Select(u => u.UnitaMisuraId).ToList();

            // 1. Elimina DimensioneBicchiere collegati
            var dimensioni = await _context.DimensioneBicchiere
                .Where(d => unitaIds.Contains(d.UnitaMisuraId))
                .ToListAsync();
            _context.DimensioneBicchiere.RemoveRange(dimensioni);

            // 2. Elimina PersonalizzazioneIngrediente collegati
            var personalizzazioni = await _context.PersonalizzazioneIngrediente
                .Where(p => unitaIds.Contains(p.UnitaMisuraId))
                .ToListAsync();
            _context.PersonalizzazioneIngrediente.RemoveRange(personalizzazioni);

            if (dimensioni.Count > 0 || personalizzazioni.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

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

            // Aggiungi altri entity con dipendenze qui se necessario
            // else if (typeof(T) == typeof(AltraEntity)) { ... }

            _context.Set<T>().RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        protected async Task ResetDatabaseAsync()
        {
            // Pulisce tutte le tabelle in ordine inverso di dipendenza
            await CleanTableAsync<DimensioneBicchiere>();
            await CleanTableAsync<Ingrediente>();
            await CleanTableAsync<StatoPagamento>();
            await CleanTableAsync<StatoOrdine>();
            await CleanTableAsync<TaxRates>();
            await CleanTableAsync<CategoriaIngrediente>();            
            await CleanTableAsync<UnitaDiMisura>();
            await CleanTableAsync<Tavolo>();

            // Riempi con i dati essenziali
            SeedEssentialTables();
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

        protected async Task<DimensioneBicchiere> CreateTestDimensioneBicchiereAsync(
    string sigla = "TEST",
    string descrizione = "Test Dimensione",
    decimal capienza = 500.00m,
    decimal prezzoBase = 3.50m,
    decimal moltiplicatore = 1.00m)
        {
            // ✅ VERIFICA E CREA UNITA DI MISURA SE NECESSARIO
            var unita = await _context.UnitaDiMisura.FirstOrDefaultAsync();

            if (unita == null)
            {
                // Crea e salva prima l'unità di misura
                unita = new UnitaDiMisura
                {
                    Sigla = "ML",
                    Descrizione = "Millilitri"
                };
                _context.UnitaDiMisura.Add(unita);
                await _context.SaveChangesAsync(); // ✅ SALVA PRIMA DI USARE L'ID
            }

            var dimensione = new DimensioneBicchiere
            {
                Sigla = sigla,
                Descrizione = descrizione,
                Capienza = capienza,
                UnitaMisuraId = unita.UnitaMisuraId, // ✅ Ora l'ID esiste nel DB
                PrezzoBase = prezzoBase,
                Moltiplicatore = moltiplicatore
            };

            _context.DimensioneBicchiere.Add(dimensione);
            await _context.SaveChangesAsync();

            return dimensione;
        }

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

            // Crea dati di test usando i metodi helper
            await CreateTestDimensioneBicchiereAsync("S", "Small", 250.00m, 2.50m, 0.85m);
            await CreateTestDimensioneBicchiereAsync("M", "Medium", 500.00m, 3.50m, 1.00m);
            await CreateTestDimensioneBicchiereAsync("L", "Large", 750.00m, 4.50m, 1.30m);
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

        public void Dispose()
        {
            _context?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}