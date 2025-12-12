using Database;
using Database.Models;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Interface;
using Repository.Service;
using Stripe;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryTest
{
    public class BaseTestCompleto : IDisposable
    {
        protected readonly BubbleTeaContext _context;
        protected readonly IIngredienteRepository _ingredienteRepository;
        protected readonly IConfiguration _configuration;
        private readonly DateTime _now;

        public BaseTestCompleto()
        {
            // ✅ CARICA CONFIGURAZIONE CON ENTRAMBI I FILE
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.test.json", optional: false)
                .AddJsonFile("appsettings.test.local.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // ✅ CREA OPZIONI PER INMEMORY
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            // ✅ CREA IL CONTEXT
            _context = new BubbleTeaContext(options);

            // ✅ CREA IL REPOSITORY DIRETTAMENTE
            _ingredienteRepository = new IngredienteRepository(_context);

            _now = DateTime.UtcNow;

            // ✅ INIZIALIZZA IL DATABASE
            InitializeTestDatabase();
        }

        private void InitializeTestDatabase()
        {
            // ✅ USA SOLO EnsureCreated
            _context.Database.EnsureCreated();

            var now = _now;
            var scadenzaFutura = now.AddHours(2);

            try
            {


                // ✅ SEQUENZA CORRETTA CON TUTTE LE DIPENDENZE

                // 1. TAVOLO (base)
                if (!_context.Tavolo.Any())
                {
                    _context.Tavolo.AddRange(
                        new Tavolo
                        {
                            TavoloId = 1, 
                            Numero = 1,
                            Zona = "Interno",
                            Disponibile = true
                        },
                        new Tavolo
                        {
                            TavoloId = 2,
                            Numero = 2,
                            Zona = "Interno",
                            Disponibile = true
                        },
                        new Tavolo
                        {
                            TavoloId = 3, 
                            Numero = 3,
                            Zona = "Terrazza",
                            Disponibile = false
                        },
                        new Tavolo
                        {
                            TavoloId = 4, 
                            Numero = 4,
                            Zona = "Terrazza",
                            Disponibile = true
                        },
                        new Tavolo
                        {
                            TavoloId = 5, 
                            Numero = 5,
                            Zona = "Bar",
                            Disponibile = true
                        }
                    );
                    _context.SaveChanges();
                }

                // 2. UNITÀ DI MISURA (base)
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
                    _context.SaveChanges();
                }

                // 3. CATEGORIE INGREDIENTI (base)
                if (!_context.CategoriaIngrediente.Any())
                {
                    _context.CategoriaIngrediente.AddRange(
                        new CategoriaIngrediente
                        {
                            CategoriaId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                            Categoria = "tea"
                        },
                        new CategoriaIngrediente
                        {
                            CategoriaId = 2, // ✅ AGGIUNTO ID PER DIPENDENZE
                            Categoria = "latte"
                        },
                        new CategoriaIngrediente
                        {
                            CategoriaId = 3, // ✅ AGGIUNTO ID PER DIPENDENZE
                            Categoria = "dolcificante"
                        },
                        new CategoriaIngrediente
                        {
                            CategoriaId = 4, // ✅ AGGIUNTO ID PER DIPENDENZE
                            Categoria = "topping"
                        },
                        new CategoriaIngrediente
                        {
                            CategoriaId = 5, // ✅ AGGIUNTO ID PER DIPENDENZE
                            Categoria = "aroma"
                        },
                        new CategoriaIngrediente
                        {
                            CategoriaId = 6, // ✅ AGGIUNTO ID PER DIPENDENZE
                            Categoria = "speciale"
                        }
                    );
                    _context.SaveChanges();
                }

                // 4. TAX RATES (base)
                if (!_context.TaxRates.Any())
                {
                    _context.TaxRates.AddRange(
                        new TaxRates
                        {
                            TaxRateId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                            Aliquota = 22.00m,
                            Descrizione = "IVA Standard",
                            DataCreazione = now,
                            DataAggiornamento = now
                        },
                        new TaxRates
                        {
                            TaxRateId = 2, // ✅ AGGIUNTO ID PER DIPENDENZE
                            Aliquota = 10.00m,
                            Descrizione = "IVA Ridotta",
                            DataCreazione = now,
                            DataAggiornamento = now
                        }
                    );
                    _context.SaveChanges();
                }

                // 5. LOG ATTIVITÀ (base)
                if (!_context.LogAttivita.Any())
                {
                    _context.LogAttivita.AddRange(
                        new LogAttivita
                        {
                            LogId = 1, // ✅ AGGIUNTO ID
                            TipoAttivita = "Sistema",
                            Descrizione = "Avvio applicazione",
                            DataEsecuzione = now.AddHours(-4),
                            Dettagli = "Sistema avviato correttamente",
                            UtenteId = null
                        },
                        new LogAttivita
                        {
                            LogId = 2, // ✅ AGGIUNTO ID
                            TipoAttivita = "Database",
                            Descrizione = "Pulizia cache",
                            DataEsecuzione = now.AddHours(-3),
                            Dettagli = "Cache pulita automaticamente",
                            UtenteId = null
                        },
                        new LogAttivita
                        {
                            LogId = 3, // ✅ AGGIUNTO ID
                            TipoAttivita = "Ordine",
                            Descrizione = "Nuovo ordine creato",
                            DataEsecuzione = now.AddHours(-2),
                            Dettagli = "Ordine #1 creato dal cliente",
                            UtenteId = 1
                        },
                        new LogAttivita
                        {
                            LogId = 4, // ✅ AGGIUNTO ID
                            TipoAttivita = "Ordine",
                            Descrizione = "Stato ordine aggiornato",
                            DataEsecuzione = now.AddHours(-1),
                            Dettagli = "Ordine #1 passato in preparazione",
                            UtenteId = 2
                        },
                        new LogAttivita
                        {
                            LogId = 5, // ✅ AGGIUNTO ID
                            TipoAttivita = "Sistema",
                            Descrizione = "Backup automatico",
                            DataEsecuzione = now.AddMinutes(-30),
                            Dettagli = "Backup database completato",
                            UtenteId = null
                        }
                    );
                    _context.SaveChanges();
                }

                // 6. STATI ORDINE (base)
                if (!_context.StatoOrdine.Any())
                {
                    _context.StatoOrdine.AddRange(
                        new StatoOrdine
                        {
                            StatoOrdineId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                            StatoOrdine1 = "bozza",
                            Terminale = false
                        },
                        new StatoOrdine
                        {
                            StatoOrdineId = 2, // ✅ AGGIUNTO ID PER DIPENDENZE
                            StatoOrdine1 = "in_carrello",
                            Terminale = false
                        },
                        new StatoOrdine
                        {
                            StatoOrdineId = 3, // ✅ AGGIUNTO ID PER DIPENDENZE
                            StatoOrdine1 = "In Attesa",
                            Terminale = false
                        },
                        new StatoOrdine
                        {
                            StatoOrdineId = 4, // ✅ AGGIUNTO ID PER DIPENDENZE
                            StatoOrdine1 = "In Preparazione",
                            Terminale = false
                        },
                        new StatoOrdine
                        {
                            StatoOrdineId = 5, // ✅ AGGIUNTO ID PER DIPENDENZE
                            StatoOrdine1 = "Pronto",
                            Terminale = false
                        },
                        new StatoOrdine
                        {
                            StatoOrdineId = 6, // ✅ AGGIUNTO ID PER DIPENDENZE
                            StatoOrdine1 = "Completato",
                            Terminale = true
                        },
                        new StatoOrdine
                        {
                            StatoOrdineId = 7, // ✅ AGGIUNTO ID PER DIPENDENZE
                            StatoOrdine1 = "Annullato",
                            Terminale = true
                        }
                    );
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Errore inizializzazione database test: {ex.Message}", ex);
            }

            // 7. CONFIG SOGLIE TEMPI (dipende da StatoOrdine)
            if (!_context.ConfigSoglieTempi.Any())
            {
                _context.ConfigSoglieTempi.AddRange(
                    new ConfigSoglieTempi
                    {
                        SogliaId = 1, // ✅ AGGIUNTO ID
                        StatoOrdineId = 1,
                        SogliaAttenzione = 5,
                        SogliaCritico = 10,
                        DataAggiornamento = now,
                        UtenteAggiornamento = "system"
                    },
                    new ConfigSoglieTempi
                    {
                        SogliaId = 2, // ✅ AGGIUNTO ID
                        StatoOrdineId = 2,
                        SogliaAttenzione = 10,
                        SogliaCritico = 20,
                        DataAggiornamento = now,
                        UtenteAggiornamento = "system"
                    },
                    new ConfigSoglieTempi
                    {
                        SogliaId = 3, // ✅ AGGIUNTO ID
                        StatoOrdineId = 3,
                        SogliaAttenzione = 5,
                        SogliaCritico = 15,
                        DataAggiornamento = now,
                        UtenteAggiornamento = "system"
                    }
                );
                _context.SaveChanges();
            }

            // 8. STATI PAGAMENTO (base)
            if (!_context.StatoPagamento.Any())
            {
                _context.StatoPagamento.AddRange(
                    new StatoPagamento
                    {
                        StatoPagamentoId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                        StatoPagamento1 = "non_richiesto"
                    },
                    new StatoPagamento
                    {
                        StatoPagamentoId = 2, // ✅ AGGIUNTO ID PER DIPENDENZE
                        StatoPagamento1 = "Pending"
                    },
                    new StatoPagamento
                    {
                        StatoPagamentoId = 3, // ✅ AGGIUNTO ID PER DIPENDENZE
                        StatoPagamento1 = "Pagato"
                    },
                    new StatoPagamento
                    {
                        StatoPagamentoId = 4, // ✅ AGGIUNTO ID PER DIPENDENZE
                        StatoPagamento1 = "Fallito"
                    },
                    new StatoPagamento
                    {
                        StatoPagamentoId = 5, // ✅ AGGIUNTO ID PER DIPENDENZE
                        StatoPagamento1 = "Rimborsato"
                    }
                );
                _context.SaveChanges();
            }

            // 9. INGREDIENTE (dipende da CategoriaIngrediente)
            if (!_context.Ingrediente.Any())
            {
                _context.Ingrediente.AddRange(
                    new Ingrediente
                    {
                        IngredienteId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Ingrediente1 = "Tea nero premium",
                        CategoriaId = 1,
                        PrezzoAggiunto = 0.50m,
                        Disponibile = true,
                        DataInserimento = now,
                        DataAggiornamento = now
                    },
                    new Ingrediente
                    {
                        IngredienteId = 2, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Ingrediente1 = "Tea verde special",
                        CategoriaId = 1,
                        PrezzoAggiunto = 0.45m,
                        Disponibile = true,
                        DataInserimento = now,
                        DataAggiornamento = now
                    },
                    new Ingrediente
                    {
                        IngredienteId = 3, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Ingrediente1 = "Sciroppo di caramello",
                        CategoriaId = 3,
                        PrezzoAggiunto = 1.50m,
                        Disponibile = true,
                        DataInserimento = now,
                        DataAggiornamento = now
                    },
                    new Ingrediente
                    {
                        IngredienteId = 4, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Ingrediente1 = "Perle di tapioca",
                        CategoriaId = 4,
                        PrezzoAggiunto = 1.20m,
                        Disponibile = true,
                        DataInserimento = now,
                        DataAggiornamento = now
                    },
                    new Ingrediente
                    {
                        IngredienteId = 5, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Ingrediente1 = "Latte di cocco",
                        CategoriaId = 2,
                        PrezzoAggiunto = 0.80m,
                        Disponibile = true,
                        DataInserimento = now,
                        DataAggiornamento = now
                    }
                );
                _context.SaveChanges();
            }

            // 10. DIMENSIONE BICCHIERE (dipende da UnitaDiMisura)
            if (!_context.DimensioneBicchiere.Any())
            {
                _context.DimensioneBicchiere.AddRange(
                    new DimensioneBicchiere
                    {
                        DimensioneBicchiereId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Sigla = "M",
                        Descrizione = "Medium",
                        Capienza = 500.00m,
                        UnitaMisuraId = 1,
                        PrezzoBase = 3.50m,
                        Moltiplicatore = 1.00m
                    },
                    new DimensioneBicchiere
                    {
                        DimensioneBicchiereId = 2, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Sigla = "L",
                        Descrizione = "Large",
                        Capienza = 700.00m,
                        UnitaMisuraId = 1,
                        PrezzoBase = 5.00m,
                        Moltiplicatore = 1.30m
                    }
                );
                _context.SaveChanges();
            }

            // 11. ARTICOLI (base) - CON ENTITÀ CORRELATE
            if (!_context.Articolo.Any())
            {
                // ✅ PRIMA CREA LE PERSONALIZZAZIONI NECESSARIE
                if (!_context.Personalizzazione.Any())
                {
                    _context.Personalizzazione.Add(new Personalizzazione
                    {
                        PersonalizzazioneId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Nome = "Classic Milk Tea",
                        DtCreazione = now,
                        Descrizione = "Il classico bubble tea con latte e perle di tapioca"
                    });
                    _context.SaveChanges();
                }

                // ✅ CREA PERSONALIZZAZIONE CUSTOM NECESSARIA
                if (!_context.PersonalizzazioneCustom.Any())
                {
                    _context.PersonalizzazioneCustom.Add(new PersonalizzazioneCustom
                    {
                        PersCustomId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Nome = "Test Custom",
                        GradoDolcezza = 3,
                        DimensioneBicchiereId = 1,
                        DataCreazione = now,
                        DataAggiornamento = now
                    });
                    _context.SaveChanges();
                }

                _context.Articolo.AddRange(
                    new Articolo
                    {
                        ArticoloId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Tipo = "BS",
                        DataCreazione = now,
                        DataAggiornamento = now,
                        BevandaStandard = new BevandaStandard()
                        {
                            Disponibile = true,
                            SempreDisponibile = true,
                            Prezzo = 4.50m,
                            DataCreazione = now,
                            DataAggiornamento = now,
                            Priorita = 1,
                            PersonalizzazioneId = 1,
                            DimensioneBicchiereId = 1
                        }
                    },
                    new Articolo
                    {
                        ArticoloId = 2, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Tipo = "BS",
                        DataCreazione = now,
                        DataAggiornamento = now,
                        BevandaStandard = new BevandaStandard()
                        {
                            Disponibile = true,
                            SempreDisponibile = false,
                            Prezzo = 5.00m,
                            DataCreazione = now,
                            DataAggiornamento = now,
                            Priorita = 2,
                            PersonalizzazioneId = 1,
                            DimensioneBicchiereId = 1
                        }
                    },
                    new Articolo
                    {
                        ArticoloId = 3, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Tipo = "BC",
                        DataCreazione = now,
                        DataAggiornamento = now,
                        BevandaCustom = new BevandaCustom()
                        {
                            PersCustomId = 1,
                            Prezzo = 6.00m,
                            DataCreazione = now,
                            DataAggiornamento = now
                        }
                    },
                    new Articolo
                    {
                        ArticoloId = 4, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Tipo = "D",
                        DataCreazione = now,
                        DataAggiornamento = now,
                        Dolce = new Dolce()
                        {
                            Nome = "Tiramisù",
                            Prezzo = 4.50m,
                            Disponibile = true,
                            DataCreazione = now,
                            DataAggiornamento = now,
                            Priorita = 1
                        }
                    },
                    new Articolo
                    {
                        ArticoloId = 5, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Tipo = "D",
                        DataCreazione = now,
                        DataAggiornamento = now,
                        Dolce = new Dolce()
                        {
                            Nome = "Cheesecake",
                            Prezzo = 5.00m,
                            Disponibile = false,
                            DataCreazione = now,
                            DataAggiornamento = now,
                            Priorita = 2
                        }
                    }
                );
                _context.SaveChanges();
            }

            // 12. PERSONALIZZAZIONE (base)
            if (!_context.Personalizzazione.Any())
            {
                _context.Personalizzazione.AddRange(
                    new Personalizzazione
                    {
                        PersonalizzazioneId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Nome = "Classic Milk Tea",
                        Descrizione = "Tè nero classico con latte",
                        DtCreazione = now
                    },
                    new Personalizzazione
                    {
                        PersonalizzazioneId = 2, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Nome = "Fruit Fusion",
                        Descrizione = "Mix frutta tropicale",
                        DtCreazione = now
                    },
                    new Personalizzazione
                    {
                        PersonalizzazioneId = 3, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Nome = "Caramel Dream",
                        Descrizione = "Base caramello e vaniglia",
                        DtCreazione = now
                    }
                );
                _context.SaveChanges();
            }

            // 13. PERSONALIZZAZIONE INGREDIENTE (dipende da Personalizzazione, Ingrediente, UnitaDiMisura)
            if (!_context.PersonalizzazioneIngrediente.Any())
            {
                _context.PersonalizzazioneIngrediente.AddRange(
                    // Classic Milk Tea ingredients
                    new PersonalizzazioneIngrediente
                    {
                        PersonalizzazioneIngredienteId = 1, // ✅ AGGIUNTO ID
                        PersonalizzazioneId = 1,
                        IngredienteId = 1,
                        Quantita = 10.0m,
                        UnitaMisuraId = 2
                    },
                    new PersonalizzazioneIngrediente
                    {
                        PersonalizzazioneIngredienteId = 2, // ✅ AGGIUNTO ID
                        PersonalizzazioneId = 1,
                        IngredienteId = 3,
                        Quantita = 20.0m,
                        UnitaMisuraId = 2
                    },
                    // Fruit Fusion ingredients
                    new PersonalizzazioneIngrediente
                    {
                        PersonalizzazioneIngredienteId = 3, // ✅ AGGIUNTO ID
                        PersonalizzazioneId = 2,
                        IngredienteId = 2,
                        Quantita = 8.0m,
                        UnitaMisuraId = 2
                    },
                    new PersonalizzazioneIngrediente
                    {
                        PersonalizzazioneIngredienteId = 4, // ✅ AGGIUNTO ID
                        PersonalizzazioneId = 2,
                        IngredienteId = 5,
                        Quantita = 150.0m,
                        UnitaMisuraId = 2
                    }
                );
                _context.SaveChanges();
            }

            // 14. DIMENSIONE QUANTITÀ INGREDIENTI (dipende da PersonalizzazioneIngrediente, DimensioneBicchiere)
            if (!_context.DimensioneQuantitaIngredienti.Any())
            {
                _context.DimensioneQuantitaIngredienti.AddRange(
                    new DimensioneQuantitaIngredienti
                    {
                        DimensioneId = 1, // ✅ AGGIUNTO ID
                        PersonalizzazioneIngredienteId = 1,
                        DimensioneBicchiereId = 1,
                        Moltiplicatore = 1.00m
                    },
                    new DimensioneQuantitaIngredienti
                    {
                        DimensioneId = 2, // ✅ AGGIUNTO ID
                        PersonalizzazioneIngredienteId = 1,
                        DimensioneBicchiereId = 2,
                        Moltiplicatore = 1.30m
                    },
                    new DimensioneQuantitaIngredienti
                    {
                        DimensioneId = 3, // ✅ AGGIUNTO ID
                        PersonalizzazioneIngredienteId = 2,
                        DimensioneBicchiereId = 1,
                        Moltiplicatore = 1.00m
                    },
                    new DimensioneQuantitaIngredienti
                    {
                        DimensioneId = 4, // ✅ AGGIUNTO ID
                        PersonalizzazioneIngredienteId = 3,
                        DimensioneBicchiereId = 1,
                        Moltiplicatore = 1.00m
                    }
                );
                _context.SaveChanges();
            }

            // 15. PERSONALIZZAZIONE CUSTOM (dipende da DimensioneBicchiere)
            if (!_context.PersonalizzazioneCustom.Any())
            {
                _context.PersonalizzazioneCustom.AddRange(
                    new PersonalizzazioneCustom
                    {
                        PersCustomId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Nome = "My Custom Tea",
                        GradoDolcezza = 2,
                        DimensioneBicchiereId = 1,
                        DataCreazione = now,
                        DataAggiornamento = now
                    },
                    new PersonalizzazioneCustom
                    {
                        PersCustomId = 2, // ✅ AGGIUNTO ID PER DIPENDENZE
                        Nome = "Extra Sweet Mix",
                        GradoDolcezza = 3,
                        DimensioneBicchiereId = 2,
                        DataCreazione = now,
                        DataAggiornamento = now
                    }
                );
                _context.SaveChanges();
            }

            // 16. INGREDIENTI PERSONALIZZAZIONE (dipende da PersonalizzazioneCustom, Ingrediente)
            if (!_context.IngredientiPersonalizzazione.Any())
            {
                _context.IngredientiPersonalizzazione.AddRange(
                    // My Custom Tea ingredients
                    new IngredientiPersonalizzazione
                    {
                        IngredientePersId = 1, // ✅ AGGIUNTO ID
                        PersCustomId = 1,
                        IngredienteId = 1,
                        DataCreazione = now
                    },
                    new IngredientiPersonalizzazione
                    {
                        IngredientePersId = 2, // ✅ AGGIUNTO ID
                        PersCustomId = 1,
                        IngredienteId = 4,
                        DataCreazione = now
                    },
                    // Extra Sweet Mix ingredients
                    new IngredientiPersonalizzazione
                    {
                        IngredientePersId = 3, // ✅ AGGIUNTO ID
                        PersCustomId = 2,
                        IngredienteId = 3,
                        DataCreazione = now
                    },
                    new IngredientiPersonalizzazione
                    {
                        IngredientePersId = 4, // ✅ AGGIUNTO ID
                        PersCustomId = 2,
                        IngredienteId = 4,
                        DataCreazione = now
                    }
                );
                _context.SaveChanges();
            }

            // 17. BEVANDA STANDARD (dipende da Articolo, Personalizzazione, DimensioneBicchiere)
            if (!_context.BevandaStandard.Any())
            {
                _context.BevandaStandard.AddRange(
                    new BevandaStandard
                    {
                        ArticoloId = 1,
                        PersonalizzazioneId = 1,
                        DimensioneBicchiereId = 1,
                        Prezzo = 4.50m,
                        Disponibile = true,
                        SempreDisponibile = true,
                        Priorita = 1,
                        DataCreazione = now,
                        DataAggiornamento = now
                    },
                    new BevandaStandard
                    {
                        ArticoloId = 2,
                        PersonalizzazioneId = 2,
                        DimensioneBicchiereId = 1,
                        Prezzo = 5.50m,
                        Disponibile = true,
                        SempreDisponibile = false,
                        Priorita = 2,
                        DataCreazione = now,
                        DataAggiornamento = now
                    },
                    new BevandaStandard
                    {
                        ArticoloId = 6, // ✅ AGGIUNTO ARTICOLO EXTRA
                        PersonalizzazioneId = 3,
                        DimensioneBicchiereId = 2,
                        Prezzo = 6.00m,
                        Disponibile = true,
                        SempreDisponibile = true,
                        Priorita = 3,
                        DataCreazione = now,
                        DataAggiornamento = now
                    }
                );
                _context.SaveChanges();
            }

            // 18. BEVANDA CUSTOM (dipende da Articolo, PersonalizzazioneCustom)
            if (!_context.BevandaCustom.Any())
            {
                _context.BevandaCustom.AddRange(
                    new BevandaCustom
                    {
                        ArticoloId = 3,
                        PersCustomId = 1,
                        Prezzo = 5.50m,
                        DataCreazione = now,
                        DataAggiornamento = now
                    },
                    new BevandaCustom
                    {
                        ArticoloId = 7, // ✅ AGGIUNTO ARTICOLO EXTRA
                        PersCustomId = 2,
                        Prezzo = 6.50m,
                        DataCreazione = now,
                        DataAggiornamento = now
                    }
                );
                _context.SaveChanges();
            }

            // 19. DOLCE (dipende da Articolo)
            if (!_context.Dolce.Any())
            {
                _context.Dolce.AddRange(
                    new Dolce
                    {
                        ArticoloId = 4,
                        Nome = "Tiramisu",
                        Prezzo = 4.50m,
                        Descrizione = "Dolce al cucchiaio",
                        ImmagineUrl = "www.immagine_2.it",
                        Disponibile = true,
                        Priorita = 1,
                        DataCreazione = now,
                        DataAggiornamento = now
                    },
                    new Dolce
                    {
                        ArticoloId = 5,
                        Nome = "Cheesecake",
                        Prezzo = 5.00m,
                        Descrizione = "Torta al formaggio",
                        ImmagineUrl = "www.immagine_3.it",
                        Disponibile = false,
                        Priorita = 2,
                        DataCreazione = now,
                        DataAggiornamento = now
                    }
                );
                _context.SaveChanges();
            }

            // 20. CLIENTE (dipende da Tavolo)
            if (!_context.Cliente.Any())
            {
                _context.Cliente.AddRange(
                    new Cliente
                    {
                        ClienteId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                        TavoloId = 1,
                        DataCreazione = now,
                        DataAggiornamento = now
                    },
                    new Cliente
                    {
                        ClienteId = 2, // ✅ AGGIUNTO ID PER DIPENDENZE
                        TavoloId = 2,
                        DataCreazione = now,
                        DataAggiornamento = now
                    }
                );
                _context.SaveChanges();
            }

            // 21. SESSIONI QR (dipende da Cliente, Tavolo)
            if (!_context.SessioniQr.Any())
            {
                var sessioneId1 = Guid.NewGuid();
                var sessioneId2 = Guid.NewGuid();

                _context.SessioniQr.AddRange(
                    new SessioniQr
                    {
                        SessioneId = sessioneId1,
                        ClienteId = 1,
                        QrCode = $"QR_{sessioneId1.ToString("N").Substring(0, 20)}",
                        DataCreazione = now,
                        DataScadenza = now.AddHours(2),
                        Utilizzato = true,
                        DataUtilizzo = now.AddMinutes(5),
                        TavoloId = 1,
                        CodiceSessione = $"SESS_{now:yyyyMMddHHmmss}",
                        Stato = "Completata"
                    },
                    new SessioniQr
                    {
                        SessioneId = sessioneId2,
                        ClienteId = null,
                        QrCode = $"QR_{sessioneId2.ToString("N").Substring(0, 20)}",
                        DataCreazione = now,
                        DataScadenza = now.AddHours(1),
                        Utilizzato = false,
                        DataUtilizzo = null,
                        TavoloId = 1,
                        CodiceSessione = $"SESS_{now.AddMinutes(1):yyyyMMddHHmmss}",
                        Stato = "Attiva"
                    },
                    new SessioniQr
                    {
                        SessioneId = Guid.NewGuid(),
                        ClienteId = 2,
                        QrCode = $"QR_{Guid.NewGuid().ToString("N").Substring(0, 20)}",
                        DataCreazione = now.AddHours(-1),
                        DataScadenza = now.AddHours(1),
                        Utilizzato = false,
                        DataUtilizzo = null,
                        TavoloId = 2,
                        CodiceSessione = $"SESS_{now.AddHours(-1):yyyyMMddHHmmss}",
                        Stato = "Attiva"
                    }
                );
                _context.SaveChanges();
            }

            // 22. PREFERITI CLIENTE (dipende da Cliente, BevandaStandard, DimensioneBicchiere)
            if (!_context.PreferitiCliente.Any())
            {
                _context.PreferitiCliente.AddRange(
                    new PreferitiCliente
                    {
                        PreferitoId = 1, // ✅ AGGIUNTO ID
                        ClienteId = 1,
                        BevandaId = 1,
                        DataAggiunta = now.AddDays(-7),
                        TipoArticolo = "BS",
                        NomePersonalizzato = "Il mio Bubble Tea Classico",
                        GradoDolcezza = 2,
                        DimensioneBicchiereId = 1,
                        IngredientiJson = "{\"ingredienti\": [\"tè nero\", \"latte\", \"tapioca\", \"zucchero di canna\"]}",
                        NotePersonali = "Preferito con poco ghiaccio"
                    },
                    new PreferitiCliente
                    {
                        PreferitoId = 2, // ✅ AGGIUNTO ID
                        ClienteId = 1,
                        BevandaId = 2,
                        DataAggiunta = now.AddDays(-3),
                        TipoArticolo = "BS",
                        NomePersonalizzato = "Bubble Tea Fruttato Estivo",
                        GradoDolcezza = 3,
                        DimensioneBicchiereId = 2,
                        IngredientiJson = "{\"ingredienti\": [\"tè verde\", \"mango\", \"frutto della passione\", \"tapioca arcobaleno\"]}",
                        NotePersonali = "Perfetto per l'estate, con extra frutta"
                    },
                    new PreferitiCliente
                    {
                        PreferitoId = 3, // ✅ AGGIUNTO ID
                        ClienteId = 1,
                        BevandaId = 3,
                        DataAggiunta = now.AddDays(-1),
                        TipoArticolo = "BS",
                        NomePersonalizzato = "Bubble Tea Light",
                        GradoDolcezza = 1,
                        DimensioneBicchiereId = 1,
                        IngredientiJson = "{\"ingredienti\": [\"tè nero\", \"latte di mandorla\", \"tapioca\", \"stevia\"]}",
                        NotePersonali = "Versione light senza zucchero aggiunto"
                    },
                    new PreferitiCliente
                    {
                        PreferitoId = 4, // ✅ AGGIUNTO ID
                        ClienteId = 2,
                        BevandaId = 1,
                        DataAggiunta = now.AddDays(-5),
                        TipoArticolo = "BS",
                        NomePersonalizzato = "Il Mio Preferito",
                        GradoDolcezza = 3,
                        DimensioneBicchiereId = 2,
                        IngredientiJson = "{\"ingredienti\": [\"tè nero\", \"latte\", \"tapioca\", \"zucchero di canna\"]}",
                        NotePersonali = "Sempre perfetto!"
                    }
                );
                _context.SaveChanges();
            }

            // 23. UTENTI (dipende da Cliente)
            if (!_context.Utenti.Any())
            {
                _context.Utenti.AddRange(
                    new Utenti
                    {
                        UtenteId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
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
                        UtenteId = 2, // ✅ AGGIUNTO ID PER DIPENDENZE
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
                        UtenteId = 3, // ✅ AGGIUNTO ID PER DIPENDENZE
                        TipoUtente = "guest",
                        SessioneGuest = Guid.NewGuid(),
                        DataCreazione = now,
                        DataAggiornamento = now,
                        Attivo = true
                    }
                );
                _context.SaveChanges();
            }

            // 24. LOG ACCESSI (dipende da Utenti, Cliente)
            if (!_context.LogAccessi.Any())
            {
                _context.LogAccessi.AddRange(
                    new LogAccessi
                    {
                        LogId = 1, // ✅ AGGIUNTO ID
                        UtenteId = 1,
                        ClienteId = null,
                        TipoAccesso = "Login",
                        Esito = "Successo",
                        IpAddress = "192.168.1.100",
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                        DataCreazione = now.AddHours(-3),
                        Dettagli = "Accesso amministratore al sistema"
                    },
                    new LogAccessi
                    {
                        LogId = 2, // ✅ AGGIUNTO ID
                        UtenteId = 2,
                        ClienteId = 1,
                        TipoAccesso = "Accesso API",
                        Esito = "Fallito",
                        IpAddress = "192.168.1.200",
                        UserAgent = "PostmanRuntime/7.32.0",
                        DataCreazione = now.AddHours(-1),
                        Dettagli = "Tentativo di accesso con token scaduto"
                    },
                    new LogAccessi
                    {
                        LogId = 3, // ✅ AGGIUNTO ID
                        UtenteId = 2,
                        ClienteId = 1,
                        TipoAccesso = "Login",
                        Esito = "Successo",
                        IpAddress = "192.168.1.150",
                        UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X)",
                        DataCreazione = now.AddMinutes(-30),
                        Dettagli = "Accesso cliente da dispositivo mobile"
                    }
                );
                _context.SaveChanges();
            }

            // 25. ORDINE (dipende da Cliente, StatoOrdine, StatoPagamento)
            if (!_context.Ordine.Any())
            {
                // ✅ RECUPERA SESSIONE QR ATTIVA PER TEST
                var sessioneAttiva = _context.SessioniQr.FirstOrDefault(sq => sq.Stato == "Attiva");

                _context.Ordine.AddRange(
                    new Ordine
                    {
                        OrdineId = 1, // ✅ AGGIUNTO ID PER DIPENDENZE
                        ClienteId = 1,
                        DataCreazione = now.AddHours(-3),
                        DataAggiornamento = now.AddHours(-3),
                        StatoOrdineId = 3, // "In Attesa"
                        StatoPagamentoId = 2, // "Pending"
                        Totale = 12.50m,
                        Priorita = 1,
                        SessioneId = sessioneAttiva?.SessioneId
                    },
                    new Ordine
                    {
                        OrdineId = 2, // ✅ AGGIUNTO ID PER DIPENDENZE
                        ClienteId = 1,
                        DataCreazione = now.AddHours(-1),
                        DataAggiornamento = now.AddHours(-1),
                        StatoOrdineId = 1, // "bozza"
                        StatoPagamentoId = 1, // "non_richiesto"
                        Totale = 8.75m,
                        Priorita = 2,
                        SessioneId = null
                    },
                    new Ordine
                    {
                        OrdineId = 3, // ✅ AGGIUNTO ID PER DIPENDENZE
                        ClienteId = 2,
                        DataCreazione = now.AddMinutes(-30),
                        DataAggiornamento = now.AddMinutes(-15),
                        StatoOrdineId = 2, // "in_carrello"
                        StatoPagamentoId = 1, // "non_richiesto"
                        Totale = 15.25m,
                        Priorita = 1,
                        SessioneId = sessioneAttiva?.SessioneId
                    }
                );
                _context.SaveChanges();
            }

            // 26. NOTIFICHE OPERATIVE (base)
            if (!_context.NotificheOperative.Any())
            {
                _context.NotificheOperative.AddRange(
                    new NotificheOperative
                    {
                        NotificaId = 1, // ✅ AGGIUNTO ID
                        DataCreazione = now.AddHours(-2),
                        OrdiniCoinvolti = "1,2",
                        Messaggio = "Ordini in attesa da più di 1 ora",
                        Stato = "Attiva",
                        DataGestione = null,
                        UtenteGestione = null,
                        Priorita = 2,
                        TipoNotifica = "RitardoOrdine"
                    },
                    new NotificheOperative
                    {
                        NotificaId = 2, // ✅ AGGIUNTO ID
                        DataCreazione = now.AddHours(-1),
                        OrdiniCoinvolti = "1",
                        Messaggio = "Ingrediente 'Perle di tapioca' in esaurimento",
                        Stato = "Risolta",
                        DataGestione = now.AddMinutes(-30),
                        UtenteGestione = "gestore",
                        Priorita = 1,
                        TipoNotifica = "ScortaIngrediente"
                    },
                    new NotificheOperative
                    {
                        NotificaId = 3, // ✅ AGGIUNTO ID
                        DataCreazione = now.AddMinutes(-15),
                        OrdiniCoinvolti = "",
                        Messaggio = "Sistema di pagamento temporaneamente non disponibile",
                        Stato = "Attiva",
                        DataGestione = null,
                        UtenteGestione = null,
                        Priorita = 3,
                        TipoNotifica = "SistemaPagamento"
                    },
                    new NotificheOperative
                    {
                        NotificaId = 4, // ✅ AGGIUNTO ID
                        DataCreazione = now.AddMinutes(-5),
                        OrdiniCoinvolti = "2",
                        Messaggio = "Ordine #2 pronto per la consegna",
                        Stato = "Attiva",
                        DataGestione = null,
                        UtenteGestione = null,
                        Priorita = 2,
                        TipoNotifica = "OrdinePronto"
                    }
                );
                _context.SaveChanges();
            }

            // 27. STATO STORICO ORDINE (dipende da Ordine, StatoOrdine)
            if (!_context.StatoStoricoOrdine.Any())
            {
                _context.StatoStoricoOrdine.AddRange(
                    new StatoStoricoOrdine
                    {
                        StatoStoricoOrdineId = 1, // ✅ AGGIUNTO ID
                        OrdineId = 1,
                        StatoOrdineId = 3,
                        Inizio = now.AddHours(-2),
                        Fine = now.AddHours(-1)
                    },
                    new StatoStoricoOrdine
                    {
                        StatoStoricoOrdineId = 2, // ✅ AGGIUNTO ID
                        OrdineId = 2,
                        StatoOrdineId = 1,
                        Inizio = now.AddHours(-1),
                        Fine = now.AddMinutes(-30)
                    },
                    new StatoStoricoOrdine
                    {
                        StatoStoricoOrdineId = 3, // ✅ AGGIUNTO ID
                        OrdineId = 3,
                        StatoOrdineId = 3,
                        Inizio = now.AddMinutes(-30)
                    },
                    new StatoStoricoOrdine
                    {
                        StatoStoricoOrdineId = 4, // ✅ AGGIUNTO ID
                        OrdineId = 1,
                        StatoOrdineId = 4,
                        Inizio = now.AddHours(-1)
                    }
                );
                _context.SaveChanges();
            }

            // 28. ORDER ITEM (dipende da Ordine, Articolo, TaxRates)
            if (!_context.OrderItem.Any())
            {
                _context.OrderItem.AddRange(
                    // Ordine 1 - Classic Milk Tea
                    new OrderItem
                    {
                        OrderItemId = 1, // ✅ AGGIUNTO ID
                        OrdineId = 1,
                        ArticoloId = 1,
                        Quantita = 2,
                        PrezzoUnitario = 4.50m,
                        ScontoApplicato = 0.00m,
                        Imponibile = 9.00m,
                        TotaleIvato = 10.98m, // 9.00 + 22% IVA
                        TaxRateId = 1,
                        TipoArticolo = "BS",
                        DataCreazione = now.AddHours(-2),
                        DataAggiornamento = now.AddHours(-2)
                    },
                    // Ordine 1 - Tiramisù
                    new OrderItem
                    {
                        OrderItemId = 2, // ✅ AGGIUNTO ID
                        OrdineId = 1,
                        ArticoloId = 4, // ✅ CORRETTO: Articolo del dolce
                        Quantita = 1,
                        PrezzoUnitario = 4.50m,
                        ScontoApplicato = 0.00m,
                        Imponibile = 4.50m,
                        TotaleIvato = 5.49m, // 4.50 + 22% IVA
                        TaxRateId = 1,
                        TipoArticolo = "D",
                        DataCreazione = now.AddHours(-2),
                        DataAggiornamento = now.AddHours(-2)
                    },
                    // Ordine 2 - Fruit Fusion
                    new OrderItem
                    {
                        OrderItemId = 3, // ✅ AGGIUNTO ID
                        OrdineId = 2,
                        ArticoloId = 2,
                        Quantita = 1,
                        PrezzoUnitario = 5.50m,
                        ScontoApplicato = 0.50m, // Sconto applicato
                        Imponibile = 5.00m,
                        TotaleIvato = 6.10m, // 5.00 + 22% IVA
                        TaxRateId = 1,
                        TipoArticolo = "BS",
                        DataCreazione = now.AddHours(-1),
                        DataAggiornamento = now.AddHours(-1)
                    },
                    // Ordine 3 - Bevanda Custom
                    new OrderItem
                    {
                        OrderItemId = 4, // ✅ AGGIUNTO ID
                        OrdineId = 3,
                        ArticoloId = 3,
                        Quantita = 1,
                        PrezzoUnitario = 5.50m,
                        ScontoApplicato = 0.00m,
                        Imponibile = 5.50m,
                        TotaleIvato = 6.71m, // 5.50 + 22% IVA
                        TaxRateId = 1,
                        TipoArticolo = "BC",
                        DataCreazione = now.AddMinutes(-30),
                        DataAggiornamento = now.AddMinutes(-30)
                    }
                );
                _context.SaveChanges();
            }

            // 29. STATISTICHE CACHE (base)
            if (!_context.StatisticheCache.Any())
            {
                _context.StatisticheCache.AddRange(
                    new StatisticheCache
                    {
                        Id = 1, // ✅ AGGIUNTO ID
                        TipoStatistica = "VenditeGiornaliere",
                        Periodo = now.ToString("yyyy-MM-dd"),
                        Metriche = "{\"totaleOrdini\": 15, \"fatturato\": 187.50, \"mediaOrdine\": 12.50, \"bevandePiuVendute\": [\"Classic Milk Tea\", \"Fruit Fusion\"]}",
                        DataAggiornamento = now.AddHours(-1)
                    },
                    new StatisticheCache
                    {
                        Id = 2, // ✅ AGGIUNTO ID
                        TipoStatistica = "VenditeMensili",
                        Periodo = now.ToString("yyyy-MM"),
                        Metriche = "{\"totaleOrdini\": 325, \"fatturato\": 4125.75, \"crescitaMesePrecedente\": 12.5, \"clientiAttivi\": 45}",
                        DataAggiornamento = now.AddDays(-1)
                    },
                    new StatisticheCache
                    {
                        Id = 3, // ✅ AGGIUNTO ID
                        TipoStatistica = "PerformanceTempi",
                        Periodo = "UltimaSettimana",
                        Metriche = "{\"tempoMedioPreparazione\": 8.5, \"tempoMedioAttesa\": 3.2, \"efficienza\": 92.5, \"ordiniRitardati\": 2}",
                        DataAggiornamento = now.AddHours(-2)
                    },
                    new StatisticheCache
                    {
                        Id = 4, // ✅ AGGIUNTO ID
                        TipoStatistica = "PreferenzeClienti",
                        Periodo = "UltimoMese",
                        Metriche = "{\"ingredientiPopolari\": [\"Perle di tapioca\", \"Sciroppo di caramello\"], \"categoriePreferite\": [\"Classici\", \"Specialità\"], \"dimensionePreferita\": \"Large\"}",
                        DataAggiornamento = now.AddDays(-2)
                    }
                );

                _context.SaveChanges();
            }
        }

        // ✅ METODI ESISTENTI (CleanTableAsync, GetStripeSettings, HasRealStripeKeys, Dispose)
        protected async Task CleanTableAsync<T>() where T : class
        {
            var entities = _context.Set<T>().ToList();
            if (entities.Count > 0)
            {
                _context.Set<T>().RemoveRange(entities);
                await _context.SaveChangesAsync();
            }
        }

        protected StripeSettingsDTO GetStripeSettings()
        {
            var stripeSection = _configuration.GetSection("Stripe");
            return new StripeSettingsDTO
            {
                SecretKey = stripeSection["SecretKey"] ?? "REPLACE_WITH_STRIPE_SECRET_KEY",
                PublishableKey = stripeSection["PublishableKey"] ?? "REPLACE_WITH_STRIPE_PUBLISHABLE_KEY",
                WebhookSecret = stripeSection["WebhookSecret"] ?? "REPLACE_WITH_STRIPE_WEBHOOK_SECRET"
            };
        }

        protected bool HasRealStripeKeys()
        {
            var settings = GetStripeSettings();
            return !settings.SecretKey.Contains("REPLACE_WITH_STRIPE") &&
                   !settings.SecretKey.Contains("sk_test_mock");
        }

        public void Dispose()
        {
            _context?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}