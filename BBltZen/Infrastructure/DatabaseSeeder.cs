using Database;
using BBltZen;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBltZen.Infrastructure
{
    public class DatabaseSeeder(BubbleTeaContext context)
    {
        private readonly BubbleTeaContext _context = context;

        public async Task SeedAsync(bool forceReset = false)
        {
            if (forceReset)
            {
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.EnsureCreatedAsync();
            }

            try
            {
                // ✅ ORDINE CORRETTO per foreign keys (SENZA TRANSAZIONE)
                await SeedTavoliAsync();
                await SeedUnitaMisuraAsync();
                await SeedCategorieIngredientiAsync();
                await SeedTaxRatesAsync();
                await SeedLogAttivitaAsync();
                await SeedStatiOrdineAsync();
                await SeedConfigSoglieTempiAsync();
                await SeedStatiPagamentoAsync();
                await SeedIngredientiAsync();
                await SeedDimensioniBicchieriAsync();
                await SeedArticoliAsync();
                await SeedPersonalizzazioniAsync();
                await SeedPersonalizzazioneIngredientiAsync();
                await SeedDimensioneQuantitaIngredientiAsync();
                await SeedPersonalizzazioniCustomAsync();
                await SeedIngredientiPersonalizzazioneAsync();
                await SeedBevandeStandardAsync();
                await SeedBevandeCustomAsync();
                await SeedDolciAsync();
                await SeedClientiAsync();
                await SeedSessioniQrAsync();
                await SeedPreferitiClienteAsync();
                await SeedUtentiAsync();
                await SeedLogAccessiAsync();
                await SeedOrdiniAsync();
                await SeedNotificheOperativeAsync();
                await SeedStatoStoricoOrdiniAsync();
                await SeedOrderItemsAsync();
                await SeedStatisticheCacheAsync();

                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Database seeded successfully!");
            }
            catch (Exception ex)
            {
                // In caso di errore con InMemory, ricomincia da zero
                if (forceReset)
                {
                    await _context.Database.EnsureDeletedAsync();
                    await _context.Database.EnsureCreatedAsync();
                }

                throw new Exception($"Seeding failed: {ex.Message}", ex);
            }
        }

        private async Task SeedTavoliAsync()
        {
            if (await _context.Tavolo.AnyAsync()) return;

            var tavoli = new[]
            {
                new Tavolo
                {
                    Numero = 1,
                    Zona = "Interno",
                    Disponibile = true
                },
                new Tavolo
                {
                    Numero = 2,
                    Zona = "Interno",
                    Disponibile = true
                },
                new Tavolo
                {
                    Numero = 3,
                    Zona = "Terrazza",
                    Disponibile = false
                },
                new Tavolo
                {
                    Numero = 4,
                    Zona = "Terrazza",
                    Disponibile = true
                },
                new Tavolo
                {
                    Numero = 5,
                    Zona = "Bar",
                    Disponibile = true
                }
            };

            await _context.Tavolo.AddRangeAsync(tavoli);
        }

        private async Task SeedUnitaMisuraAsync()
        {
            if (await _context.UnitaDiMisura.AnyAsync()) return;

            var unita = new[]
            {
                new UnitaDiMisura
                {
                    UnitaMisuraId = 1, // AGGIUNGI
                    Sigla = "ML",
                    Descrizione = "Millilitri"
                },
                new UnitaDiMisura
                {
                    UnitaMisuraId = 2, // AGGIUNGI
                    Sigla = "GR",
                    Descrizione = "Grammi"
                },
                new UnitaDiMisura
                {
                    UnitaMisuraId = 3, // AGGIUNGI
                    Sigla = "PZ",
                    Descrizione = "Pezzi"
                }
            };

            await _context.UnitaDiMisura.AddRangeAsync(unita);
        }

        private async Task SeedCategorieIngredientiAsync()
        {
            if (await _context.CategoriaIngrediente.AnyAsync()) return;

            var categorie = new[]
            {
                new CategoriaIngrediente
                {
                    Categoria = "tea"
                },
                new CategoriaIngrediente
                {
                    Categoria = "latte"
                },
                new CategoriaIngrediente
                {
                    Categoria = "dolcificante"
                },
                new CategoriaIngrediente
                {
                    Categoria = "topping"
                },
                new CategoriaIngrediente
                {
                    Categoria = "aroma"
                },
                new CategoriaIngrediente
                {
                    Categoria = "speciale" 
                }
            };

            await _context.CategoriaIngrediente.AddRangeAsync(categorie);
        }

        private async Task SeedTaxRatesAsync()
        {
            if (await _context.TaxRates.AnyAsync()) return;

            var taxRates = new[]
            {
                new TaxRates
                {
                    Aliquota = 22.00m,
                    Descrizione = "IVA Standard",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                },
                new TaxRates
                {
                    Aliquota = 10.00m,
                    Descrizione = "IVA Ridotta",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                }
            };

            await _context.TaxRates.AddRangeAsync(taxRates);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ TaxRates seeded - {taxRates.Length} aliquote create");
        }

        private async Task SeedLogAttivitaAsync()
        {
            if (await _context.LogAttivita.AnyAsync()) return;

            try
            {
                var utenti = await _context.Utenti.ToListAsync();
                var utente = utenti.FirstOrDefault();

                var logAttivita = new[]
                {
                    new LogAttivita
                    {
                        TipoAttivita = "Sistema",
                        Descrizione = "Avvio applicazione",
                        DataEsecuzione = DateTime.UtcNow.AddHours(-4),
                        Dettagli = "Sistema avviato correttamente",
                        UtenteId = null
                    },
                    new LogAttivita
                    {
                        TipoAttivita = "Database",
                        Descrizione = "Pulizia cache",
                        DataEsecuzione = DateTime.UtcNow.AddHours(-3),
                        Dettagli = "Cache pulita automaticamente",
                        UtenteId = null
                    },
                    new LogAttivita
                    {
                        TipoAttivita = "Ordine",
                        Descrizione = "Nuovo ordine creato",
                        DataEsecuzione = DateTime.UtcNow.AddHours(-2),
                        Dettagli = "Ordine #1 creato dal cliente",
                        UtenteId = 1
                    },
                    new LogAttivita
                    {
                        TipoAttivita = "Ordine",
                        Descrizione = "Stato ordine aggiornato",
                        DataEsecuzione = DateTime.UtcNow.AddHours(-1),
                        Dettagli = "Ordine #1 passato in preparazione",
                        UtenteId = 2
                    },
                    new LogAttivita
                    {
                        TipoAttivita = "Sistema",
                        Descrizione = "Backup automatico",
                        DataEsecuzione = DateTime.UtcNow.AddMinutes(-30),
                        Dettagli = "Backup database completato",
                        UtenteId = null
                    }
                };

                await _context.LogAttivita.AddRangeAsync(logAttivita);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ LogAttivita seeded - {logAttivita.Length} record creati");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedLogAttivitaAsync: {ex.Message}");
            }
        }

        private async Task SeedStatiOrdineAsync()
        {
            if (await _context.StatoOrdine.AnyAsync()) return;

            var statiOrdine = new[]
            {
                // ✅ NUOVI STATI AGGIUNTI
                new StatoOrdine
                {
                    StatoOrdine1 = "bozza",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdine1 = "in carrello",
                    Terminale = false
                },
                // ✅ STATI ESISTENTI
                new StatoOrdine
                {
                    StatoOrdine1 = "in coda",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdine1 = "in preparazione",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdine1 = "pronta consegna",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdine1 = "consegnato",
                    Terminale = true
                },
                new StatoOrdine
                {
                    StatoOrdine1 = "sospeso",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdine1 = "annullato",
                    Terminale = true
                }
            };

            await _context.StatoOrdine.AddRangeAsync(statiOrdine);
            await _context.SaveChangesAsync(); // ✅ AGGIUNTO SaveChangesAsync

            Console.WriteLine($"✅ StatoOrdine seeded successfully - {statiOrdine.Length} stati creati");
        }

        private async Task SeedConfigSoglieTempiAsync()
        {
            if (await _context.ConfigSoglieTempi.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutti gli stati
                var statiOrdine = await _context.StatoOrdine.ToListAsync();

                // ✅ CORREZIONE: Usa i nomi esatti degli stati dal seed
                var statoInCoda = statiOrdine.FirstOrDefault(s => s.StatoOrdine1 == "in coda");
                var statoInPreparazione = statiOrdine.FirstOrDefault(s => s.StatoOrdine1 == "in preparazione");
                var statoProntaConsegna = statiOrdine.FirstOrDefault(s => s.StatoOrdine1 == "pronta consegna");

                // ✅ Verifica che gli stati esistano
                if (statoInCoda == null || statoInPreparazione == null || statoProntaConsegna == null)
                {
                    Console.WriteLine("⚠️  Stati ordine non trovati per ConfigSoglieTempi");
                    return;
                }

                var soglieTempi = new[]
                {
            new ConfigSoglieTempi
            {
                StatoOrdineId = statoInCoda.StatoOrdineId,
                SogliaAttenzione = 5,    // minuti
                SogliaCritico = 10,      // minuti
                DataAggiornamento = DateTime.UtcNow,
                UtenteAggiornamento = "system"
            },
            new ConfigSoglieTempi
            {
                StatoOrdineId = statoInPreparazione.StatoOrdineId,
                SogliaAttenzione = 10,   // minuti
                SogliaCritico = 20,      // minuti
                DataAggiornamento = DateTime.UtcNow,
                UtenteAggiornamento = "system"
            },
            new ConfigSoglieTempi
            {
                StatoOrdineId = statoProntaConsegna.StatoOrdineId,
                SogliaAttenzione = 5,    // minuti
                SogliaCritico = 15,      // minuti
                DataAggiornamento = DateTime.UtcNow,
                UtenteAggiornamento = "system"
            }
        };

                await _context.ConfigSoglieTempi.AddRangeAsync(soglieTempi);

                // ✅ IMPORTANTE: Salva i cambiamenti per le ConfigSoglieTempi
                await _context.SaveChangesAsync();

                Console.WriteLine("✅ ConfigSoglieTempi seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedConfigSoglieTempiAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedStatiPagamentoAsync()
        {
            if (await _context.StatoPagamento.AnyAsync()) return;

            var statiPagamento = new[]
            {
                // ✅ NUOVO STATO AGGIUNTO
                new StatoPagamento
                {
                    StatoPagamento1 = "non richiesto"
                },
                // ✅ STATI ESISTENTI
                new StatoPagamento
                {
                    StatoPagamento1 = "pendente"
                },
                new StatoPagamento
                {
                    StatoPagamento1 = "completato"
                },
                new StatoPagamento
                {
                    StatoPagamento1 = "fallito"
                },
                new StatoPagamento
                {
                    StatoPagamento1 = "rimborsato"
                }
            };

            await _context.StatoPagamento.AddRangeAsync(statiPagamento);
            await _context.SaveChangesAsync(); // ✅ AGGIUNTO SaveChangesAsync

            Console.WriteLine($"✅ StatoPagamento seeded successfully - {statiPagamento.Length} stati creati");
        }

        private async Task SeedIngredientiAsync()
        {
            if (await _context.Ingrediente.AnyAsync()) return;

            // Aspetta che le categorie siano salvate per ottenere gli ID corretti
            await _context.SaveChangesAsync();

            var teaCat = await _context.CategoriaIngrediente.FirstAsync(c => c.Categoria == "tea");
            var latteCat = await _context.CategoriaIngrediente.FirstAsync(c => c.Categoria == "latte");
            var dolcificanteCat = await _context.CategoriaIngrediente.FirstAsync(c => c.Categoria == "dolcificante");
            var toppingCat = await _context.CategoriaIngrediente.FirstAsync(c => c.Categoria == "topping");

            var ingredienti = new[]
            {
                new Ingrediente
                {
                    Ingrediente1 = "Tea nero premium",
                    CategoriaId = teaCat.CategoriaId,
                    PrezzoAggiunto = 0.50m,
                    Disponibile = true,
                    DataInserimento = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                },
                new Ingrediente
                {
                    Ingrediente1 = "Tea verde special",
                    CategoriaId = teaCat.CategoriaId,
                    PrezzoAggiunto = 0.45m,
                    Disponibile = true,
                    DataInserimento = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                },
                new Ingrediente
                {
                    Ingrediente1 = "Sciroppo di caramello",
                    CategoriaId = dolcificanteCat.CategoriaId,
                    PrezzoAggiunto = 1.50m,
                    Disponibile = true,
                    DataInserimento = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                },
                new Ingrediente
                {
                    Ingrediente1 = "Perle di tapioca",
                    CategoriaId = toppingCat.CategoriaId,
                    PrezzoAggiunto = 1.20m,
                    Disponibile = true,
                    DataInserimento = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                },
                new Ingrediente
                {
                    Ingrediente1 = "Latte di cocco",
                    CategoriaId = latteCat.CategoriaId,
                    PrezzoAggiunto = 0.80m,
                    Disponibile = true,
                    DataInserimento = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                }
            };

            await _context.Ingrediente.AddRangeAsync(ingredienti);
        }

        private async Task SeedDimensioniBicchieriAsync()
        {
            if (await _context.DimensioneBicchiere.AnyAsync()) return;

            var mlUnit = await _context.UnitaDiMisura.FirstAsync(u => u.Sigla == "ML");

            var dimensioni = new[]
            {
                new DimensioneBicchiere
                {
                    Sigla = "M",
                    Descrizione = "Medium",
                    Capienza = 500.00m,
                    UnitaMisuraId = mlUnit.UnitaMisuraId,
                    PrezzoBase = 3.50m,
                    Moltiplicatore = 1.00m
                },
                new DimensioneBicchiere
                {
                    Sigla = "L",
                    Descrizione = "Large",
                    Capienza = 700.00m,
                    UnitaMisuraId = mlUnit.UnitaMisuraId,
                    PrezzoBase = 5.00m,
                    Moltiplicatore = 1.30m
                }
            };

            await _context.DimensioneBicchiere.AddRangeAsync(dimensioni);
        }

        private async Task SeedArticoliAsync()
        {
            if (await _context.Articolo.AnyAsync()) return;

            var now = DateTime.UtcNow;

            var articoli = new[]
            {
                new Articolo
                {
                    Tipo = "BS",
                    DataCreazione = now,
                    DataAggiornamento = now,
                    BevandaStandard = new BevandaStandard()
                    {
                        Disponibile = true,
                        SempreDisponibile = true, // ✅ ORDINABILE
                        Prezzo = 4.50m,
                        DataCreazione = now, // ✅ DATA OBBLIGATORIA
                        DataAggiornamento = now, // ✅ DATA OBBLIGATORIA
                        Priorita = 1, // ✅ AGGIUNTO PRIORITÀ (1-10)
                        PersonalizzazioneId = 1, // ✅ DEVE ESSERE SETTATO
                        DimensioneBicchiereId = 1 // ✅ DEVE ESSERE SETTATO
                    }
                },
                new Articolo
                {
                    Tipo = "BS",
                    DataCreazione = now,
                    DataAggiornamento = now,
                    BevandaStandard = new BevandaStandard()
                    {
                        Disponibile = true,
                        SempreDisponibile = false, // ✅ NON ORDINABILE
                        Prezzo = 5.00m,
                        DataCreazione = now, // ✅ DATA OBBLIGATORIA
                        DataAggiornamento = now, // ✅ DATA OBBLIGATORIA
                        Priorita = 2, // ✅ AGGIUNTO PRIORITÀ (1-10)
                        PersonalizzazioneId = 1, // ✅ DEVE ESSERE SETTATO
                        DimensioneBicchiereId = 1 // ✅ DEVE ESSERE SETTATO
                    }
                },
                new Articolo
                {
                    Tipo = "BC",
                    DataCreazione = now,
                    DataAggiornamento = now,
                    BevandaCustom = new BevandaCustom() // ✅ CORRETTO: SINGOLO OGGETTO, NON LISTA
                    {
                        PersCustomId = 1, // ✅ AGGIUNTO: FK OBBLIGATORIA
                        Prezzo = 6.00m,
                        DataCreazione = now, // ✅ DATA OBBLIGATORIA
                        DataAggiornamento = now // ✅ DATA OBBLIGATORIA
                    }
                },
                new Articolo
                {
                    Tipo = "BC",
                    DataCreazione = now,
                    DataAggiornamento = now,
                    BevandaCustom = new BevandaCustom() // ✅ CORRETTO: SINGOLO OGGETTO, NON LISTA
                    {
                        PersCustomId = 2, // ✅ AGGIUNTO: FK OBBLIGATORIA
                        Prezzo = 6.00m,
                        DataCreazione = now, // ✅ DATA OBBLIGATORIA
                        DataAggiornamento = now // ✅ DATA OBBLIGATORIA
                    }
                },
                // ✅ DOLCE - ORDINABILE
                new Articolo
                {
                    Tipo = "D",
                    DataCreazione = now,
                    DataAggiornamento = now,
                    Dolce = new Dolce()
                    {
                        Nome = "Tiramisù",
                        Prezzo = 4.50m,
                        Disponibile = true, // ✅ ORDINABILE
                        DataCreazione = now, // ✅ DATA OBBLIGATORIA
                        DataAggiornamento = now, // ✅ DATA OBBLIGATORIA
                        Priorita = 1 // ✅ AGGIUNTO PRIORITÀ (1-10)
                    }
                },
                // ✅ DOLCE - NON ORDINABILE
                new Articolo
                {
                    Tipo = "D",
                    DataCreazione = now,
                    DataAggiornamento = now,
                    Dolce = new Dolce()
                    {
                        Nome = "Cheesecake",
                        Prezzo = 5.00m,
                        Disponibile = false, // ✅ NON ORDINABILE
                        DataCreazione = now, // ✅ DATA OBBLIGATORIA
                        DataAggiornamento = now, // ✅ DATA OBBLIGATORIA
                        Priorita = 2 // ✅ AGGIUNTO PRIORITÀ (1-10)
                    }
                }
            };

            await _context.Articolo.AddRangeAsync(articoli);
            await _context.SaveChangesAsync(); // ✅ AGGIUNTO SaveChangesAsync

            Console.WriteLine($"✅ Articoli seeded successfully - {articoli.Length} articoli creati");
        }

        private async Task SeedPersonalizzazioniAsync()
        {
            if (await _context.Personalizzazione.AnyAsync()) return;

            var personalizzazioni = new[]
            {
                new Personalizzazione
                {
                    Nome = "Classic Milk Tea",
                    Descrizione = "Tè nero classico con latte",
                    DtCreazione = DateTime.UtcNow
                },
                new Personalizzazione
                {
                    Nome = "Fruit Fusion",
                    Descrizione = "Mix frutta tropicale",
                    DtCreazione = DateTime.UtcNow
                },
                new Personalizzazione
                {
                    Nome = "Caramel Dream",
                    Descrizione = "Base caramello e vaniglia",
                    DtCreazione = DateTime.UtcNow
                }
            };

            await _context.Personalizzazione.AddRangeAsync(personalizzazioni);
        }

        private async Task SeedPersonalizzazioneIngredientiAsync()
        {
            if (await _context.PersonalizzazioneIngrediente.AnyAsync()) return;

            // Classic Milk Tea: Tea nero + Sciroppo di caramello
            // Fruit Fusion: Tea verde + Latte di cocco  
            // Caramel Dream: Latte di cocco

            var piList = new[]
            {
                // Classic Milk Tea
                new PersonalizzazioneIngrediente { PersonalizzazioneId = 1, IngredienteId = 1, Quantita = 250m, UnitaMisuraId = 1 },
                new PersonalizzazioneIngrediente { PersonalizzazioneId = 1, IngredienteId = 3, Quantita = 50m, UnitaMisuraId = 1 },
        
                // Fruit Fusion
                new PersonalizzazioneIngrediente { PersonalizzazioneId = 2, IngredienteId = 2, Quantita = 300m, UnitaMisuraId = 1 },
                new PersonalizzazioneIngrediente { PersonalizzazioneId = 2, IngredienteId = 5, Quantita = 100m, UnitaMisuraId = 1 },
        
                // Caramel Dream
                new PersonalizzazioneIngrediente { PersonalizzazioneId = 3, IngredienteId = 5, Quantita = 200m, UnitaMisuraId = 1 }
            };

            await _context.PersonalizzazioneIngrediente.AddRangeAsync(piList);
            Console.WriteLine($"✅ PersonalizzazioneIngredienti seeded: {piList.Length} records");
        }

        private async Task SeedDimensioneQuantitaIngredientiAsync()
        {
            if (await _context.DimensioneQuantitaIngredienti.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Assicurati che il contesto sia salvato prima delle query
                await _context.SaveChangesAsync();

                // ✅ Carica tutto in memoria
                var personalizzazioneIngredienti = await _context.PersonalizzazioneIngrediente.ToListAsync();
                var dimensioniBicchieri = await _context.DimensioneBicchiere.ToListAsync();

                // ✅ Verifica che i dati esistano
                if (personalizzazioneIngredienti.Count == 0)
                {
                    Console.WriteLine("⚠️  Nessuna PersonalizzazioneIngrediente trovata");
                    return;
                }

                if (dimensioniBicchieri.Count == 0)
                {
                    Console.WriteLine("⚠️  Nessuna DimensioneBicchiere trovata");
                    return;
                }

                // ✅ Trova le dimensioni per sigla
                var dimensioneMedia = dimensioniBicchieri.FirstOrDefault(d => d.Sigla == "M");
                var dimensioneLarge = dimensioniBicchieri.FirstOrDefault(d => d.Sigla == "L");

                if (dimensioneMedia == null || dimensioneLarge == null)
                {
                    Console.WriteLine($"⚠️  Dimensioni bicchieri non trovate. Disponibili: {string.Join(", ", dimensioniBicchieri.Select(d => d.Sigla))}");
                    return;
                }

                var dimensioneQuantitaIngredienti = new List<DimensioneQuantitaIngredienti>();

                foreach (var personalizzazioneIngrediente in personalizzazioneIngredienti)
                {
                    // Per dimensione Media
                    dimensioneQuantitaIngredienti.Add(new DimensioneQuantitaIngredienti
                    {
                        PersonalizzazioneIngredienteId = personalizzazioneIngrediente.PersonalizzazioneIngredienteId,
                        DimensioneBicchiereId = dimensioneMedia.DimensioneBicchiereId,
                        Moltiplicatore = 1.0m
                    });

                    // Per dimensione Large
                    dimensioneQuantitaIngredienti.Add(new DimensioneQuantitaIngredienti
                    {
                        PersonalizzazioneIngredienteId = personalizzazioneIngrediente.PersonalizzazioneIngredienteId,
                        DimensioneBicchiereId = dimensioneLarge.DimensioneBicchiereId,
                        Moltiplicatore = 1.3m
                    });
                }

                await _context.DimensioneQuantitaIngredienti.AddRangeAsync(dimensioneQuantitaIngredienti);

                // ✅ SALVA ESPLICITAMENTE questa tabella
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ DimensioneQuantitaIngredienti seeded: {dimensioneQuantitaIngredienti.Count} records");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore in SeedDimensioneQuantitaIngredientiAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner Exception: {ex.InnerException.Message}");
                }
                // Non propagare l'eccezione per non bloccare tutto il seeding
            }
        }

        private async Task SeedPersonalizzazioniCustomAsync()
        {
            if (await _context.PersonalizzazioneCustom.AnyAsync()) return;

            try
            {
                var dimensioniBicchieri = await _context.DimensioneBicchiere.ToListAsync();

                // ✅ CORREZIONE: Controllo conteggio invece di FirstOrDefault
                if (dimensioniBicchieri.Count < 1)
                {
                    Console.WriteLine("⚠️  DimensioniBicchiere insufficienti per PersonalizzazioniCustom");
                    return;
                }

                var dimensione = dimensioniBicchieri[0]; // ✅ Accesso per indice

                var personalizzazioniCustom = new[]
                {
                    new PersonalizzazioneCustom
                    {
                        Nome = "My Custom Tea",
                        GradoDolcezza = 2,
                        DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    },
                    new PersonalizzazioneCustom
                    {
                        Nome = "Extra Sweet Mix",
                        GradoDolcezza = 3,
                        DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                };

                await _context.PersonalizzazioneCustom.AddRangeAsync(personalizzazioniCustom);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ PersonalizzazioniCustom seeded - {personalizzazioniCustom.Length} create");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedPersonalizzazioniCustomAsync: {ex.Message}");
            }
        }

        private async Task SeedIngredientiPersonalizzazioneAsync()
        {
            if (await _context.IngredientiPersonalizzazione.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var personalizzazioniCustom = await _context.PersonalizzazioneCustom.ToListAsync();
                var ingredienti = await _context.Ingrediente.ToListAsync();

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var customTea = personalizzazioniCustom.FirstOrDefault(p => p.Nome == "My Custom Tea");
                var extraSweet = personalizzazioniCustom.FirstOrDefault(p => p.Nome == "Extra Sweet Mix");

                var teaNero = ingredienti.FirstOrDefault(i => i.Ingrediente1 == "Tea nero premium");
                var caramello = ingredienti.FirstOrDefault(i => i.Ingrediente1 == "Sciroppo di caramello");
                var perleTapioca = ingredienti.FirstOrDefault(i => i.Ingrediente1 == "Perle di tapioca");

                // ✅ Verifica che tutte le entità esistano
                if (customTea == null || extraSweet == null ||
                    teaNero == null || caramello == null || perleTapioca == null)
                {
                    Console.WriteLine("⚠️  Entità mancanti per IngredientiPersonalizzazione");
                    return;
                }

                var ingredientiPersonalizzazione = new[]
                {
                    // My Custom Tea ingredients
                    new IngredientiPersonalizzazione
                    {
                        PersCustomId = customTea.PersCustomId,
                        IngredienteId = teaNero.IngredienteId,
                        DataCreazione = DateTime.UtcNow
                    },
                    new IngredientiPersonalizzazione
                    {
                        PersCustomId = customTea.PersCustomId,
                        IngredienteId = perleTapioca.IngredienteId,
                        DataCreazione = DateTime.UtcNow
                    },

                    // Extra Sweet Mix ingredients
                    new IngredientiPersonalizzazione
                    {
                        PersCustomId = extraSweet.PersCustomId,
                        IngredienteId = caramello.IngredienteId,
                        DataCreazione = DateTime.UtcNow
                    },
                    new IngredientiPersonalizzazione
                    {
                        PersCustomId = extraSweet.PersCustomId,
                        IngredienteId = perleTapioca.IngredienteId,
                        DataCreazione = DateTime.UtcNow
                    }
                };

                await _context.IngredientiPersonalizzazione.AddRangeAsync(ingredientiPersonalizzazione);
                Console.WriteLine("✅ IngredientiPersonalizzazione seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedIngredientiPersonalizzazioneAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedBevandeStandardAsync()
        {
            if (await _context.BevandaStandard.AnyAsync()) return;

            try
            {
                var articoli = await _context.Articolo.ToListAsync();
                var personalizzazioni = await _context.Personalizzazione.ToListAsync();
                var dimensioniBicchieri = await _context.DimensioneBicchiere.ToListAsync();

                // ✅ CORREZIONE: Filtra in memoria invece di Where()
                var articoliBS = articoli.Where(a => a.Tipo == "BS").ToList();

                if (articoliBS.Count < 2 || personalizzazioni.Count < 1 || dimensioniBicchieri.Count < 1)
                {
                    Console.WriteLine("⚠️  Articoli BS, Personalizzazioni o Dimensioni insufficienti");
                    return;
                }

                var articolo1 = articoliBS[0];
                var articolo2 = articoliBS[1];
                var personalizzazione = personalizzazioni[0];
                var dimensione = dimensioniBicchieri[0];

                var bevande = new[]
                {
                    new BevandaStandard
                    {
                        ArticoloId = articolo1.ArticoloId,
                        PersonalizzazioneId = personalizzazione.PersonalizzazioneId,
                        DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                        Prezzo = 4.50m,
                        Disponibile = true,
                        SempreDisponibile = true,
                        Priorita = 1,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    },
                    new BevandaStandard
                    {
                        ArticoloId = articolo2.ArticoloId,
                        PersonalizzazioneId = personalizzazione.PersonalizzazioneId,
                        DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                        Prezzo = 5.50m,
                        Disponibile = true,
                        SempreDisponibile = false,
                        Priorita = 2,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                };

                await _context.BevandaStandard.AddRangeAsync(bevande);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ BevandeStandard seeded - {bevande.Length} bevande create");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedBevandeStandardAsync: {ex.Message}");
            }
        }

        private async Task SeedBevandeCustomAsync()
        {
            if (await _context.BevandaCustom.AnyAsync()) return;

            try
            {
                var articoli = await _context.Articolo.ToListAsync();
                var personalizzazioniCustom = await _context.PersonalizzazioneCustom.ToListAsync();

                // ✅ CORREZIONE: Filtra in memoria invece di Where()
                var articoliBC = articoli.Where(a => a.Tipo == "BC").ToList();

                if (articoliBC.Count < 1 || personalizzazioniCustom.Count < 1)
                {
                    Console.WriteLine("⚠️  Articoli BC o PersonalizzazioniCustom insufficienti");
                    return;
                }

                var articolo = articoliBC[0];
                var personalizzazione = personalizzazioniCustom[0];

                var bevandeCustom = new[]
                {
                    new BevandaCustom
                    {
                        ArticoloId = articolo.ArticoloId,
                        PersCustomId = personalizzazione.PersCustomId,
                        Prezzo = 6.00m,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                };

                await _context.BevandaCustom.AddRangeAsync(bevandeCustom);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ BevandeCustom seeded - {bevandeCustom.Length} bevande create");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedBevandeCustomAsync: {ex.Message}");
            }
        }

        private async Task SeedDolciAsync()
        {
            if (await _context.Dolce.AnyAsync()) return;

            try
            {
                var articoli = await _context.Articolo.ToListAsync();

                // ✅ CORREZIONE: Filtra in memoria invece di Where()
                var articoliD = articoli.Where(a => a.Tipo == "D").ToList();

                if (articoliD.Count < 1)
                {
                    Console.WriteLine("⚠️  Nessun articolo D trovato per Dolci");
                    return;
                }

                var dolci = new[]
                {
                    new Dolce
                    {
                        ArticoloId = articoliD[0].ArticoloId,
                        Nome = "Tiramisù",
                        Prezzo = 5.50m,
                        Disponibile = true,
                        Priorita = 1,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                };

                await _context.Dolce.AddRangeAsync(dolci);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Dolci seeded - {dolci.Length} dolci creati");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedDolciAsync: {ex.Message}");
            }
        }

        private async Task SeedClientiAsync()
        {
            if (await _context.Cliente.AnyAsync()) return;

            try
            {
                var tavoli = await _context.Tavolo.ToListAsync();
                var tavolo = tavoli.FirstOrDefault();

                if (tavolo == null)
                {
                    Console.WriteLine("⚠️  Nessun tavolo trovato per Clienti");
                    return;
                }

                var clienti = new[]
                {
                    new Cliente
                    {
                        TavoloId = tavolo.TavoloId,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                };

                await _context.Cliente.AddRangeAsync(clienti);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Clienti seeded - {clienti.Length} clienti creati");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedClientiAsync: {ex.Message}");
            }
        }

        private async Task SeedSessioniQrAsync()
        {
            if (await _context.SessioniQr.AnyAsync()) return;

            try
            {
                var tavoli = await _context.Tavolo.ToListAsync();
                var clienti = await _context.Cliente.ToListAsync();

                var tavolo = tavoli.FirstOrDefault();
                var cliente = clienti.FirstOrDefault();

                if (tavolo == null)
                {
                    Console.WriteLine("⚠️  Nessun tavolo trovato per SessioniQr");
                    return;
                }

                var sessioniQr = new[]
                {
                    new SessioniQr
                    {
                        SessioneId = Guid.NewGuid(),
                        ClienteId = cliente?.ClienteId,
                        QrCode = $"QR_{Guid.NewGuid()}",
                        DataCreazione = DateTime.UtcNow,
                        DataScadenza = DateTime.UtcNow.AddHours(2),
                        Utilizzato = true,
                        DataUtilizzo = DateTime.UtcNow.AddMinutes(5),
                        TavoloId = tavolo.TavoloId,
                        CodiceSessione = $"SESS_{DateTime.UtcNow:yyyyMMddHHmmss}",
                        Stato = "Completata"
                    },
                    new SessioniQr
                    {
                        SessioneId = Guid.NewGuid(),
                        ClienteId = null,
                        QrCode = $"QR_{Guid.NewGuid()}",
                        DataCreazione = DateTime.UtcNow,
                        DataScadenza = DateTime.UtcNow.AddHours(1),
                        Utilizzato = false,
                        DataUtilizzo = null,
                        TavoloId = tavolo.TavoloId,
                        CodiceSessione = $"SESS_{DateTime.UtcNow.AddMinutes(1):yyyyMMddHHmmss}",
                        Stato = "Attiva"
                    }
                };

                await _context.SessioniQr.AddRangeAsync(sessioniQr);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ SessioniQr seeded - {sessioniQr.Length} sessioni create");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedSessioniQrAsync: {ex.Message}");
            }
        }

        private async Task SeedPreferitiClienteAsync()
        {
            if (await _context.PreferitiCliente.AnyAsync()) return;

            try
            {
                var clienti = await _context.Cliente.ToListAsync();
                var bevandeStandard = await _context.BevandaStandard.ToListAsync();
                var dimensioniBicchiere = await _context.DimensioneBicchiere.ToListAsync();

                // ✅ CORREZIONE: Controllo conteggio invece di FirstOrDefault con condizioni
                if (clienti.Count < 1 || bevandeStandard.Count < 2 || dimensioniBicchiere.Count < 1)
                {
                    Console.WriteLine("⚠️  Clienti, BevandeStandard o Dimensioni insufficienti per PreferitiCliente");
                    return;
                }

                var cliente = clienti[0]; // ✅ Accesso per indice
                var bevanda1 = bevandeStandard[0]; // ✅ Prima bevanda
                var bevanda2 = bevandeStandard[1]; // ✅ Seconda bevanda  
                var dimensioneDefault = dimensioniBicchiere[0]; // ✅ Accesso per indice

                var preferiti = new[]
                {
                    new PreferitiCliente
                    {
                        ClienteId = cliente.ClienteId,
                        BevandaId = bevanda1.ArticoloId,
                        DataAggiunta = DateTime.UtcNow.AddDays(-7),
                        TipoArticolo = "BS",
                        NomePersonalizzato = "Il mio Bubble Tea Classico",
                        GradoDolcezza = 2,
                        DimensioneBicchiereId = dimensioneDefault.DimensioneBicchiereId,
                        IngredientiJson = "{\"ingredienti\": [\"tè nero\", \"latte\", \"tapioca\", \"zucchero di canna\"]}",
                        NotePersonali = "Preferito con poco ghiaccio"
                    },
                    new PreferitiCliente
                    {
                        ClienteId = cliente.ClienteId,
                        BevandaId = bevanda2.ArticoloId,
                        DataAggiunta = DateTime.UtcNow.AddDays(-3),
                        TipoArticolo = "BS",
                        NomePersonalizzato = "Bubble Tea Fruttato Estivo",
                        GradoDolcezza = 3,
                        DimensioneBicchiereId = dimensioneDefault.DimensioneBicchiereId,
                        IngredientiJson = "{\"ingredienti\": [\"tè verde\", \"mango\", \"frutto della passione\", \"tapioca arcobaleno\"]}",
                        NotePersonali = "Perfetto per l'estate, con extra frutta"
                    },
                    new PreferitiCliente
                    {
                        ClienteId = cliente.ClienteId,
                        BevandaId = bevanda1.ArticoloId,
                        DataAggiunta = DateTime.UtcNow.AddDays(-1),
                        TipoArticolo = "BS",
                        NomePersonalizzato = "Bubble Tea Light",
                        GradoDolcezza = 1,
                        DimensioneBicchiereId = dimensioneDefault.DimensioneBicchiereId,
                        IngredientiJson = "{\"ingredienti\": [\"tè nero\", \"latte di mandorla\", \"tapioca\", \"stevia\"]}",
                        NotePersonali = "Versione light senza zucchero aggiunto"
                    }
                };

                await _context.PreferitiCliente.AddRangeAsync(preferiti);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ PreferitiCliente seeded - {preferiti.Length} preferiti creati");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedPreferitiClienteAsync: {ex.Message}");
            }
        }

        private async Task SeedUtentiAsync()
        {
            if (await _context.Utenti.AnyAsync()) return;

            var utenti = new[]
            {
                new Utenti
                {
                    UtenteId = 1,
                    Email = "gestore@bubbleteazen.com",
                    PasswordHash = "$2a$10$N9qo8uLOickgx2ZMRZoMye3s3B9yX7U7Jq.7c6q8q7q6q8q7q6q8q7",
                    TipoUtente = "gestore",
                    Nome = "Mario",
                    Cognome = "Rossi",
                    Telefono = "+39123456789",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow,
                    Attivo = true
                },
                new Utenti
                {
                    UtenteId = 2,
                    Email = "cliente@email.com",
                    PasswordHash = "$2a$10$N9qo8uLOickgx2ZMRZoMye3s3B9yX7U7Jq.7c6q8q7q6q8q7q6q8q7",
                    TipoUtente = "cliente",
                    Nome = "Luigi",
                    Cognome = "Verdi",
                    Telefono = "+39987654321",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow,
                    Attivo = true
                },
                new Utenti
                {   
                    UtenteId = 3,
                    TipoUtente = "guest",
                    SessioneGuest = Guid.NewGuid(),
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow,
                    Attivo = true
                }
            };

            await _context.Utenti.AddRangeAsync(utenti);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ Utenti seeded - {utenti.Length} utenti creati");
        }

        private async Task SeedLogAccessiAsync()
        {
            if (await _context.LogAccessi.AnyAsync()) return;

            try
            {
                var utenti = await _context.Utenti.ToListAsync();
                var clienti = await _context.Cliente.ToListAsync();

                var utente = utenti.FirstOrDefault();
                var cliente = clienti.FirstOrDefault();

                if (utente == null)
                {
                    Console.WriteLine("⚠️  Utente non trovato per LogAccessi");
                    return;
                }

                var logAccessi = new List<LogAccessi>
                {
                    new() {
                        UtenteId = utente.UtenteId,
                        ClienteId = null,
                        TipoAccesso = "Login",
                        Esito = "Successo",
                        IpAddress = "192.168.1.100",
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                        DataCreazione = DateTime.UtcNow.AddHours(-3),
                        Dettagli = "Accesso amministratore al sistema"
                    },
                    new() {
                        UtenteId = utente.UtenteId,
                        ClienteId = null,
                        TipoAccesso = "Accesso API",
                        Esito = "Fallito",
                        IpAddress = "192.168.1.200",
                        UserAgent = "PostmanRuntime/7.32.0",
                        DataCreazione = DateTime.UtcNow.AddHours(-1),
                        Dettagli = "Tentativo di accesso con token scaduto"
                    }
                };

                if (cliente != null)
                {
                    logAccessi.Add(new LogAccessi
                    {
                        UtenteId = null,
                        ClienteId = cliente.ClienteId,
                        TipoAccesso = "Registrazione",
                        Esito = "Successo",
                        IpAddress = "192.168.1.150",
                        UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X)",
                        DataCreazione = DateTime.UtcNow.AddHours(-2),
                        Dettagli = "Nuovo cliente registrato tramite QR code"
                    });
                }

                await _context.LogAccessi.AddRangeAsync(logAccessi);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ LogAccessi seeded - {logAccessi.Count} record creati");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedLogAccessiAsync: {ex.Message}");
            }
        }

        private async Task SeedOrdiniAsync()
        {
            if (await _context.Ordine.AnyAsync()) return;

            try
            {
                var clienti = await _context.Cliente.ToListAsync();
                var statiOrdine = await _context.StatoOrdine.ToListAsync();
                var statiPagamento = await _context.StatoPagamento.ToListAsync();
                var sessioniQr = await _context.SessioniQr.ToListAsync();

                // ✅ CORREZIONE: Controllo conteggio invece di FirstOrDefault con condizioni
                if (clienti.Count < 1 || statiOrdine.Count < 3 || statiPagamento.Count < 2)
                {
                    Console.WriteLine("⚠️  Clienti, StatiOrdine o StatiPagamento insufficienti per Ordini");
                    return;
                }

                var cliente = clienti[0]; // ✅ Accesso per indice

                // ✅ CORREZIONE: Prendi i primi stati invece di cercare per nome
                var stato1 = statiOrdine[0]; // Primo stato (es. "bozza")
                var stato2 = statiOrdine[1]; // Secondo stato (es. "in carrello")
                var stato3 = statiOrdine[2]; // Terzo stato (es. "in coda")

                var statoPagamento1 = statiPagamento[0]; // "non richiesto"
                var statoPagamento2 = statiPagamento[1]; // "pendente"
                var statoPagamento3 = statiPagamento[2]; // "completato"

                var sessioneQr = sessioniQr.Count > 0 ? sessioniQr[0] : null; // ✅ Sessione opzionale

                var ordini = new[]
                {
                    new Ordine
                    {
                        ClienteId = cliente.ClienteId,
                        DataCreazione = DateTime.UtcNow.AddHours(-3),
                        DataAggiornamento = DateTime.UtcNow.AddHours(-3),
                        StatoOrdineId = stato2.StatoOrdineId, // "in carrello"
                        StatoPagamentoId = statoPagamento1.StatoPagamentoId, // "non richiesto"
                        Totale = 12.50m,
                        Priorita = 1,
                        SessioneId = sessioneQr?.SessioneId
                    },
                    new Ordine
                    {
                        ClienteId = cliente.ClienteId,
                        DataCreazione = DateTime.UtcNow.AddHours(-1),
                        DataAggiornamento = DateTime.UtcNow.AddHours(-1),
                        StatoOrdineId = stato3.StatoOrdineId, // "in coda"
                        StatoPagamentoId = statoPagamento2.StatoPagamentoId, // "pendente"
                        Totale = 8.75m,
                        Priorita = 2,
                        SessioneId = null
                    },
                    new Ordine
                    {
                        ClienteId = cliente.ClienteId,
                        DataCreazione = DateTime.UtcNow.AddMinutes(-30),
                        DataAggiornamento = DateTime.UtcNow.AddMinutes(-15),
                        StatoOrdineId = stato3.StatoOrdineId, // "in coda"
                        StatoPagamentoId = statoPagamento3.StatoPagamentoId,// "completato"
                        Totale = 15.25m,
                        Priorita = 1,
                        SessioneId = sessioneQr?.SessioneId
                    }
                };

                await _context.Ordine.AddRangeAsync(ordini);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Ordini seeded - {ordini.Length} ordini creati");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedOrdiniAsync: {ex.Message}");
            }
        }

        private async Task SeedNotificheOperativeAsync()
        {
            if (await _context.NotificheOperative.AnyAsync()) return;

            var notifiche = new[]
            {
                new NotificheOperative
                {
                    DataCreazione = DateTime.UtcNow.AddHours(-2),
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
                    DataCreazione = DateTime.UtcNow.AddHours(-1),
                    OrdiniCoinvolti = "1",
                    Messaggio = "Ingrediente 'Perle di tapioca' in esaurimento",
                    Stato = "Risolta",
                    DataGestione = DateTime.UtcNow.AddMinutes(-30),
                    UtenteGestione = "gestore",
                    Priorita = 1,
                    TipoNotifica = "ScortaIngrediente"
                },
                new NotificheOperative
                {
                    DataCreazione = DateTime.UtcNow.AddMinutes(-15),
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
                    DataCreazione = DateTime.UtcNow.AddMinutes(-5),
                    OrdiniCoinvolti = "2",
                    Messaggio = "Ordine #2 pronto per la consegna",
                    Stato = "Attiva",
                    DataGestione = null,
                    UtenteGestione = null,
                    Priorita = 2,
                    TipoNotifica = "OrdinePronto"
                }
            };

            await _context.NotificheOperative.AddRangeAsync(notifiche);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ NotificheOperative seeded - {notifiche.Length} notifiche create");
        }

        private async Task SeedStatoStoricoOrdiniAsync()
        {
            if (await _context.StatoStoricoOrdine.AnyAsync()) return;

            try
            {
                var ordini = await _context.Ordine.ToListAsync();
                var statiOrdine = await _context.StatoOrdine.ToListAsync();

                if (ordini.Count < 2 || statiOrdine.Count < 3)
                {
                    Console.WriteLine("⚠️  Ordini o stati insufficienti per StatoStoricoOrdine");
                    return;
                }

                var ordine1 = ordini[0];
                var ordine2 = ordini[1];

                // ✅ CORREZIONE: Prendi i primi 3 stati invece di cercare per nome
                var stato1 = statiOrdine[0]; // Primo stato (es. "bozza")
                var stato2 = statiOrdine[1]; // Secondo stato (es. "in_carrello")  
                var stato3 = statiOrdine[2]; // Terzo stato (es. "In Attesa")

                var storicoOrdini = new[]
                {
                    new StatoStoricoOrdine
                    {
                        OrdineId = ordine1.OrdineId,
                        StatoOrdineId = stato1.StatoOrdineId,
                        Inizio = DateTime.UtcNow.AddHours(-2),
                        Fine = DateTime.UtcNow.AddHours(-1)
                    },
                    new StatoStoricoOrdine
                    {
                        OrdineId = ordine1.OrdineId,
                        StatoOrdineId = stato2.StatoOrdineId,
                        Inizio = DateTime.UtcNow.AddHours(-1),
                        Fine = DateTime.UtcNow.AddMinutes(-30)
                    },
                    new StatoStoricoOrdine
                    {
                        OrdineId = ordine1.OrdineId,
                        StatoOrdineId = stato3.StatoOrdineId,
                        Inizio = DateTime.UtcNow.AddMinutes(-30)
                    },
                    new StatoStoricoOrdine
                    {
                        OrdineId = ordine2.OrdineId,
                        StatoOrdineId = stato1.StatoOrdineId,
                        Inizio = DateTime.UtcNow.AddHours(-1)
                    }
                };

                await _context.StatoStoricoOrdine.AddRangeAsync(storicoOrdini);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ StatoStoricoOrdine seeded - {storicoOrdini.Length} record creati");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedStatoStoricoOrdiniAsync: {ex.Message}");
            }
        }

        private async Task SeedOrderItemsAsync()
        {
            if (await _context.OrderItem.AnyAsync()) return;

            try
            {
                var ordini = await _context.Ordine.ToListAsync();
                var articoli = await _context.Articolo.ToListAsync();
                var taxRates = await _context.TaxRates.ToListAsync();

                // ✅ MODIFICA: BASTA 1 ORDINE
                if (!ordini.Any())
                {
                    Console.WriteLine("⚠️  Nessun ordine trovato per OrderItems");
                    return;
                }

                var ordine1 = ordini[0];

                // ✅ MODIFICA: PRENDI QUALSIASI ARTICOLO
                var articolo1 = articoli.FirstOrDefault();
                var articolo2 = articolo1 != null && articoli.Count > 1 ? articoli[1] : articolo1;

                if (articolo1 == null)
                {
                    Console.WriteLine("⚠️  Nessun articolo trovato per OrderItems");
                    return;
                }

                var taxRate = taxRates.FirstOrDefault();
                if (taxRate == null)
                {
                    Console.WriteLine("⚠️  Nessuna tax rate trovata per OrderItems");
                    return;
                }

                Console.WriteLine($"✅ Creando OrderItems con: OrdineId={ordine1.OrdineId}, ArticoloId={articolo1.ArticoloId}, TaxRateId={taxRate.TaxRateId}");

                var orderItems = new[]
                {
                    new OrderItem
                    {
                        OrdineId = ordine1.OrdineId,
                        ArticoloId = articolo1.ArticoloId,
                        Quantita = 2,
                        PrezzoUnitario = 4.50m,
                        ScontoApplicato = 0.00m,
                        Imponibile = 9.00m,
                        TotaleIvato = 9.00m + 9.00m * taxRate.Aliquota / 100,
                        TaxRateId = taxRate.TaxRateId, // ✅ COLLEGATO!
                        TipoArticolo = articolo1.Tipo,
                        DataCreazione = DateTime.UtcNow.AddHours(-2),
                        DataAggiornamento = DateTime.UtcNow.AddHours(-2)
                    }
                };

                await _context.OrderItem.AddRangeAsync(orderItems);
                Console.WriteLine($"✅ OrderItems seeded successfully ({orderItems.Length} records)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedOrderItemsAsync: {ex.Message}");
            }
        }

        private async Task SeedStatisticheCacheAsync()
        {
            if (await _context.StatisticheCache.AnyAsync()) return;

            var statistiche = new[]
            {
                new StatisticheCache
                {
                    TipoStatistica = "VenditeGiornaliere",
                    Periodo = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    Metriche = "{\"totaleOrdini\": 15, \"fatturato\": 187.50, \"mediaOrdine\": 12.50, \"bevandePiuVendute\": [\"Classic Milk Tea\", \"Fruit Fusion\"]}",
                    DataAggiornamento = DateTime.UtcNow.AddHours(-1)
                },
                new StatisticheCache
                {
                    TipoStatistica = "VenditeMensili",
                    Periodo = DateTime.UtcNow.ToString("yyyy-MM"),
                    Metriche = "{\"totaleOrdini\": 325, \"fatturato\": 4125.75, \"crescitaMesePrecedente\": 12.5, \"clientiAttivi\": 45}",
                    DataAggiornamento = DateTime.UtcNow.AddDays(-1)
                },
                new StatisticheCache
                {
                    TipoStatistica = "PerformanceTempi",
                    Periodo = "UltimaSettimana",
                    Metriche = "{\"tempoMedioPreparazione\": 8.5, \"tempoMedioAttesa\": 3.2, \"efficienza\": 92.5, \"ordiniRitardati\": 2}",
                    DataAggiornamento = DateTime.UtcNow.AddHours(-2)
                },
                new StatisticheCache
                {
                    TipoStatistica = "PreferenzeClienti",
                    Periodo = "UltimoMese",
                    Metriche = "{\"ingredientiPopolari\": [\"Perle di tapioca\", \"Sciroppo di caramello\"], \"categoriePreferite\": [\"Classici\", \"Specialità\"], \"dimensionePreferita\": \"Large\"}",
                    DataAggiornamento = DateTime.UtcNow.AddDays(-2)
                }
            };

            await _context.StatisticheCache.AddRangeAsync(statistiche);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ StatisticheCache seeded - {statistiche.Length} statistiche create");
        }
    }
}