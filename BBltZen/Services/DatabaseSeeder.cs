using Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBltZen.Services
{
    public class DatabaseSeeder
    {
        private readonly BubbleTeaContext _context;

        public DatabaseSeeder(BubbleTeaContext context)
        {
            _context = context;
        }

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
                new Tavolo { Numero = 1, Zona = "Interno", Disponibile = true },
                new Tavolo { Numero = 2, Zona = "Interno", Disponibile = true },
                new Tavolo { Numero = 3, Zona = "Terrazza", Disponibile = false },
                new Tavolo { Numero = 4, Zona = "Terrazza", Disponibile = true },
                new Tavolo { Numero = 5, Zona = "Bar", Disponibile = true }
            };

            await _context.Tavolo.AddRangeAsync(tavoli);
        }

        private async Task SeedUnitaMisuraAsync()
        {
            if (await _context.UnitaDiMisura.AnyAsync()) return;

            var unita = new[]
            {
                new UnitaDiMisura { Sigla = "ML", Descrizione = "Millilitri" },
                new UnitaDiMisura { Sigla = "GR", Descrizione = "Grammi" },
                new UnitaDiMisura { Sigla = "PZ", Descrizione = "Pezzi" }
            };

            await _context.UnitaDiMisura.AddRangeAsync(unita);
        }

        private async Task SeedCategorieIngredientiAsync()
        {
            if (await _context.CategoriaIngrediente.AnyAsync()) return;

            var categorie = new[]
            {
                new CategoriaIngrediente { Categoria = "tea" },
                new CategoriaIngrediente { Categoria = "latte" },
                new CategoriaIngrediente { Categoria = "dolcificante" },
                new CategoriaIngrediente { Categoria = "topping" },
                new CategoriaIngrediente { Categoria = "aroma" },
                new CategoriaIngrediente { Categoria = "speciale" }
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

            var articoli = new[]
            {
                new Articolo
                {
                    Tipo = "BS",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow,
                    BevandaStandard = new BevandaStandard()
                    {
                        Disponibile = true,
                        SempreDisponibile = true, // ✅ ORDINABILE
                        Prezzo = 4.50m,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                },
                new Articolo
                {
                    Tipo = "BS",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow,
                    BevandaStandard = new BevandaStandard()
                    {
                        Disponibile = true,
                        SempreDisponibile = false, // ✅ NON ORDINABILE
                        Prezzo = 5.00m,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                },
                new Articolo
                {
                    Tipo = "BC",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow,
                    BevandaCustom = new List<BevandaCustom>() 
                    {
                        new BevandaCustom()
                        {
                            Prezzo = 6.00m,
                            DataCreazione = DateTime.UtcNow,
                            DataAggiornamento = DateTime.UtcNow
                        }
                    }
                },
                // ✅ DOLCE - ORDINABILE
                new Articolo
                {
                    Tipo = "D",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow,
                    Dolce = new Dolce() // ✅ AGGIUNGI PARENTESI ()
                    {
                        Nome = "Tiramisù",
                        Prezzo = 4.50m,
                        Disponibile = true, // ✅ ORDINABILE
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                },
                // ✅ DOLCE - NON ORDINABILE
                new Articolo
                {
                    Tipo = "D",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow,
                    Dolce = new Dolce() // ✅ AGGIUNGI PARENTESI ()
                    {
                        Nome = "Cheesecake",
                        Prezzo = 5.00m,
                        Disponibile = false, // ✅ NON ORDINABILE
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                }
            };

            await _context.Articolo.AddRangeAsync(articoli);
            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Articoli seeded successfully with correct types and specializations");
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

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var personalizzazioni = await _context.Personalizzazione.ToListAsync();
                var ingredienti = await _context.Ingrediente.ToListAsync();
                var unitaMisura = await _context.UnitaDiMisura.ToListAsync();

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var personalizzazioneClassic = personalizzazioni.FirstOrDefault(p => p.Nome == "Classic Milk Tea");
                var personalizzazioneFruit = personalizzazioni.FirstOrDefault(p => p.Nome == "Fruit Fusion");

                var teaNero = ingredienti.FirstOrDefault(i => i.Ingrediente1 == "Tea nero premium");
                var teaVerde = ingredienti.FirstOrDefault(i => i.Ingrediente1 == "Tea verde special");
                var caramello = ingredienti.FirstOrDefault(i => i.Ingrediente1 == "Sciroppo di caramello");
                var latteCocco = ingredienti.FirstOrDefault(i => i.Ingrediente1 == "Latte di cocco");

                var grUnit = unitaMisura.FirstOrDefault(u => u.Sigla == "GR");
                var mlUnit = unitaMisura.FirstOrDefault(u => u.Sigla == "ML");

                // ✅ Verifica che tutte le entità esistano
                if (personalizzazioneClassic == null || personalizzazioneFruit == null ||
                    teaNero == null || teaVerde == null || caramello == null || latteCocco == null ||
                    grUnit == null || mlUnit == null)
                {
                    Console.WriteLine("⚠️  Entità mancanti per PersonalizzazioneIngredienti");
                    return;
                }

                var personalizzazioneIngredienti = new[]
                {
                    // Classic Milk Tea ingredients
                    new PersonalizzazioneIngrediente
                    {
                        PersonalizzazioneId = personalizzazioneClassic.PersonalizzazioneId,
                        IngredienteId = teaNero.IngredienteId,
                        Quantita = 10.0m,
                        UnitaMisuraId = grUnit.UnitaMisuraId
                    },
                    new PersonalizzazioneIngrediente
                    {
                        PersonalizzazioneId = personalizzazioneClassic.PersonalizzazioneId,
                        IngredienteId = caramello.IngredienteId,
                        Quantita = 20.0m,
                        UnitaMisuraId = mlUnit.UnitaMisuraId
                    },

                    // Fruit Fusion ingredients
                    new PersonalizzazioneIngrediente
                    {
                        PersonalizzazioneId = personalizzazioneFruit.PersonalizzazioneId,
                        IngredienteId = teaVerde.IngredienteId,
                        Quantita = 8.0m,
                        UnitaMisuraId = grUnit.UnitaMisuraId
                    },
                    new PersonalizzazioneIngrediente
                    {
                        PersonalizzazioneId = personalizzazioneFruit.PersonalizzazioneId,
                        IngredienteId = latteCocco.IngredienteId,
                        Quantita = 150.0m,
                        UnitaMisuraId = mlUnit.UnitaMisuraId
                    }
                };

                await _context.PersonalizzazioneIngrediente.AddRangeAsync(personalizzazioneIngredienti);
                Console.WriteLine("✅ PersonalizzazioneIngredienti seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedPersonalizzazioneIngredientiAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedDimensioneQuantitaIngredientiAsync()
        {
            if (await _context.DimensioneQuantitaIngredienti.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var personalizzazioneIngredienti = await _context.PersonalizzazioneIngrediente.ToListAsync();
                var dimensioniBicchieri = await _context.DimensioneBicchiere.ToListAsync();

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var dimensioneMedia = dimensioniBicchieri.FirstOrDefault(d => d.Sigla == "M");
                var dimensioneLarge = dimensioniBicchieri.FirstOrDefault(d => d.Sigla == "L");

                // ✅ Verifica che le dimensioni esistano
                if (dimensioneMedia == null || dimensioneLarge == null)
                {
                    Console.WriteLine("⚠️  Dimensioni bicchieri non trovate per DimensioneQuantitaIngredienti");
                    return;
                }

                // ✅ Verifica che ci siano personalizzazione ingredienti
                if (!personalizzazioneIngredienti.Any())
                {
                    Console.WriteLine("⚠️  Nessuna PersonalizzazioneIngrediente trovata per DimensioneQuantitaIngredienti");
                    return;
                }

                var dimensioneQuantitaIngredienti = new List<DimensioneQuantitaIngredienti>();

                // Per ogni personalizzazione ingrediente, crea record per entrambe le dimensioni
                foreach (var personalizzazioneIngrediente in personalizzazioneIngredienti)
                {
                    // Per dimensione Media
                    dimensioneQuantitaIngredienti.Add(new DimensioneQuantitaIngredienti
                    {
                        PersonalizzazioneIngredienteId = personalizzazioneIngrediente.PersonalizzazioneIngredienteId,
                        DimensioneBicchiereId = dimensioneMedia.DimensioneBicchiereId,
                        Moltiplicatore = 1.0m // Moltiplicatore base per dimensione media
                    });

                    // Per dimensione Large
                    dimensioneQuantitaIngredienti.Add(new DimensioneQuantitaIngredienti
                    {
                        PersonalizzazioneIngredienteId = personalizzazioneIngrediente.PersonalizzazioneIngredienteId,
                        DimensioneBicchiereId = dimensioneLarge.DimensioneBicchiereId,
                        Moltiplicatore = 1.3m // 30% in più per dimensione large
                    });
                }

                await _context.DimensioneQuantitaIngredienti.AddRangeAsync(dimensioneQuantitaIngredienti);
                Console.WriteLine($"✅ DimensioneQuantitaIngredienti seeded successfully ({dimensioneQuantitaIngredienti.Count} records)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedDimensioneQuantitaIngredientiAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedPersonalizzazioniCustomAsync()
        {
            if (await _context.PersonalizzazioneCustom.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var dimensioniBicchieri = await _context.DimensioneBicchiere.ToListAsync();

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var dimensioneMedia = dimensioniBicchieri.FirstOrDefault(d => d.Sigla == "M");
                var dimensioneLarge = dimensioniBicchieri.FirstOrDefault(d => d.Sigla == "L");

                // ✅ Verifica che le dimensioni esistano
                if (dimensioneMedia == null || dimensioneLarge == null)
                {
                    Console.WriteLine("⚠️  Dimensioni bicchieri non trovate per PersonalizzazioniCustom");
                    return;
                }

                var personalizzazioniCustom = new[]
                {
                    new PersonalizzazioneCustom
                    {
                        Nome = "My Custom Tea",
                        GradoDolcezza = 5,
                        DimensioneBicchiereId = dimensioneMedia.DimensioneBicchiereId,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    },
                    new PersonalizzazioneCustom
                    {
                        Nome = "Extra Sweet Mix",
                        GradoDolcezza = 8,
                        DimensioneBicchiereId = dimensioneLarge.DimensioneBicchiereId,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                };

                await _context.PersonalizzazioneCustom.AddRangeAsync(personalizzazioniCustom);
                Console.WriteLine("✅ PersonalizzazioniCustom seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedPersonalizzazioniCustomAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
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
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var articoli = await _context.Articolo.Where(a => a.Tipo == "bevanda").ToListAsync();
                var personalizzazioni = await _context.Personalizzazione.ToListAsync();
                var dimensioniBicchieri = await _context.DimensioneBicchiere.ToListAsync();

                // ✅ Verifica che ci siano abbastanza articoli bevanda
                if (articoli.Count < 2)
                {
                    Console.WriteLine("⚠️  Articoli bevanda insufficienti per BevandeStandard");
                    return;
                }

                // ✅ Usa accesso per indice invece di Skip().FirstAsync()
                var articoloBevanda1 = articoli[0];
                var articoloBevanda2 = articoli[1];

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var personalizzazioneClassic = personalizzazioni.FirstOrDefault(p => p.Nome == "Classic Milk Tea");
                var personalizzazioneFruit = personalizzazioni.FirstOrDefault(p => p.Nome == "Fruit Fusion");

                var dimensioneMedia = dimensioniBicchieri.FirstOrDefault(d => d.Sigla == "M");
                var dimensioneLarge = dimensioniBicchieri.FirstOrDefault(d => d.Sigla == "L");

                // ✅ Verifica che tutte le entità esistano
                if (personalizzazioneClassic == null || personalizzazioneFruit == null ||
                    dimensioneMedia == null || dimensioneLarge == null)
                {
                    Console.WriteLine("⚠️  Entità mancanti per BevandeStandard");
                    return;
                }

                var bevande = new[]
                {
                    new BevandaStandard
                    {
                        ArticoloId = articoloBevanda1.ArticoloId,
                        PersonalizzazioneId = personalizzazioneClassic.PersonalizzazioneId,
                        DimensioneBicchiereId = dimensioneMedia.DimensioneBicchiereId,
                        Prezzo = 4.50m,
                        ImmagineUrl = "/images/classic-milk-tea.jpg",
                        Disponibile = true,
                        SempreDisponibile = true,
                        Priorita = 1,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    },
                    new BevandaStandard
                    {
                        ArticoloId = articoloBevanda2.ArticoloId,
                        PersonalizzazioneId = personalizzazioneFruit.PersonalizzazioneId,
                        DimensioneBicchiereId = dimensioneLarge.DimensioneBicchiereId,
                        Prezzo = 5.50m,
                        ImmagineUrl = "/images/fruit-fusion.jpg",
                        Disponibile = true,
                        SempreDisponibile = true,
                        Priorita = 2,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                };

                await _context.BevandaStandard.AddRangeAsync(bevande);
                Console.WriteLine("✅ BevandeStandard seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedBevandeStandardAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedBevandeCustomAsync()
        {
            if (await _context.BevandaCustom.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var articoli = await _context.Articolo.Where(a => a.Tipo == "bevanda").ToListAsync();
                var personalizzazioniCustom = await _context.PersonalizzazioneCustom.ToListAsync();

                // ✅ Verifica che ci siano abbastanza articoli bevanda
                if (articoli.Count < 3)
                {
                    Console.WriteLine("⚠️  Articoli bevanda insufficienti per BevandeCustom (servono almeno 3)");
                    return;
                }

                // ✅ Usa accesso per indice invece di Skip(2).FirstAsync()
                var articoloBevanda3 = articoli[2];

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var customTea = personalizzazioniCustom.FirstOrDefault(p => p.Nome == "My Custom Tea");
                var extraSweet = personalizzazioniCustom.FirstOrDefault(p => p.Nome == "Extra Sweet Mix");

                // ✅ Verifica che tutte le entità esistano
                if (customTea == null || extraSweet == null)
                {
                    Console.WriteLine("⚠️  PersonalizzazioniCustom mancanti per BevandeCustom");
                    return;
                }

                var bevandeCustom = new[]
                {
                    new BevandaCustom
                    {
                        ArticoloId = articoloBevanda3.ArticoloId,
                        PersCustomId = customTea.PersCustomId,
                        Prezzo = 6.00m,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                };

                await _context.BevandaCustom.AddRangeAsync(bevandeCustom);
                Console.WriteLine("✅ BevandeCustom seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedBevandeCustomAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedDolciAsync()
        {
            if (await _context.Dolce.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var articoli = await _context.Articolo.Where(a => a.Tipo == "dolce").ToListAsync();

                // ✅ Verifica che ci siano articoli dolce
                if (!articoli.Any())
                {
                    Console.WriteLine("⚠️  Nessun articolo dolce trovato per Dolci");
                    return;
                }

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var articoloDolce = articoli.FirstOrDefault();

                // ✅ Verifica che l'articolo esista
                if (articoloDolce == null)
                {
                    Console.WriteLine("⚠️  Articolo dolce non trovato");
                    return;
                }

                var dolci = new[]
                {
                    new Dolce
                    {
                        ArticoloId = articoloDolce.ArticoloId,
                        Nome = "Tiramisù",
                        Prezzo = 5.50m,
                        Descrizione = "Dolce al cucchiaio classico",
                        ImmagineUrl = "/images/tiramisu.jpg",
                        Disponibile = true,
                        Priorita = 1,
                        DataCreazione = DateTime.UtcNow,
                        DataAggiornamento = DateTime.UtcNow
                    }
                };

                await _context.Dolce.AddRangeAsync(dolci);
                Console.WriteLine("✅ Dolci seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedDolciAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedClientiAsync()
        {
            if (await _context.Cliente.AnyAsync()) return;

            var tavolo1 = await _context.Tavolo.FirstAsync(t => t.Numero == 1);

            var clienti = new[]
            {
                new Cliente
                {
                    TavoloId = tavolo1.TavoloId,
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                }
            };

            await _context.Cliente.AddRangeAsync(clienti);
        }

        private async Task SeedUtentiAsync()
        {
            if (await _context.Utenti.AnyAsync()) return;

            var utenti = new[]
            {
                new Utenti
                {
                    Email = "gestore@bubbleteazen.com",
                    PasswordHash = "$2a$10$N9qo8uLOickgx2ZMRZoMye3s3B9yX7U7Jq.7c6q8q7q6q8q7q6q8q7", // password: "test123"
                    TipoUtente = "gestore",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow,
                    Attivo = true
                }
            };

            await _context.Utenti.AddRangeAsync(utenti);
        }

        private async Task SeedStatiOrdineAsync()
        {
            if (await _context.StatoOrdine.AnyAsync()) return;

            var statiOrdine = new[]
            {
                new StatoOrdine
                {
                    StatoOrdine1 = "In Attesa",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdine1 = "In Preparazione",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdine1 = "Pronto",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdine1 = "Completato",
                    Terminale = true
                },
                new StatoOrdine
                {
                    StatoOrdine1 = "Annullato",
                    Terminale = true
                }
            };

            await _context.StatoOrdine.AddRangeAsync(statiOrdine);
        }

        private async Task SeedStatiPagamentoAsync()
        {
            if (await _context.StatoPagamento.AnyAsync()) return;

            var statiPagamento = new[]
            {
                new StatoPagamento
                {
                    StatoPagamento1 = "Pending"
                },
                new StatoPagamento
                {
                    StatoPagamento1 = "Pagato"
                },
                new StatoPagamento
                {
                    StatoPagamento1 = "Fallito"
                },
                new StatoPagamento
                {
                    StatoPagamento1 = "Rimborsato"
                }
            };

            await _context.StatoPagamento.AddRangeAsync(statiPagamento);
        }

        private async Task SeedOrdiniAsync()
        {
            if (await _context.Ordine.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var clienti = await _context.Cliente.ToListAsync();
                var statiOrdine = await _context.StatoOrdine.ToListAsync();
                var statiPagamento = await _context.StatoPagamento.ToListAsync();

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var cliente = clienti.FirstOrDefault();

                // ✅ CORRETTO: Usa 'StatoOrdine1' invece di 'NomeStatoOrdine'
                var statoOrdineInAttesa = statiOrdine.FirstOrDefault(s => s.StatoOrdine1 == "In Attesa");

                // ✅ CORRETTO: Usa 'StatoPagamento1' invece di 'NomeStatoPagamento'  
                var statoPagamentoPending = statiPagamento.FirstOrDefault(s => s.StatoPagamento1 == "Pending");

                // ✅ Verifica che tutte le entità esistano
                if (cliente == null)
                {
                    Console.WriteLine("⚠️  Cliente non trovato per Ordini");
                    return;
                }

                if (statoOrdineInAttesa == null)
                {
                    Console.WriteLine("⚠️  Stato ordine 'In Attesa' non trovato per Ordini");
                    return;
                }

                if (statoPagamentoPending == null)
                {
                    Console.WriteLine("⚠️  Stato pagamento 'Pending' non trovato per Ordini");
                    return;
                }

                var ordini = new[]
                {
                    new Ordine
                    {
                        ClienteId = cliente.ClienteId,
                        DataCreazione = DateTime.UtcNow.AddHours(-2),
                        DataAggiornamento = DateTime.UtcNow.AddHours(-2),
                        StatoOrdineId = statoOrdineInAttesa.StatoOrdineId,
                        StatoPagamentoId = statoPagamentoPending.StatoPagamentoId,
                        Totale = 12.50m,
                        Priorita = 1
                    },
                    new Ordine
                    {
                        ClienteId = cliente.ClienteId,
                        DataCreazione = DateTime.UtcNow.AddHours(-1),
                        DataAggiornamento = DateTime.UtcNow.AddHours(-1),
                        StatoOrdineId = statoOrdineInAttesa.StatoOrdineId,
                        StatoPagamentoId = statoPagamentoPending.StatoPagamentoId,
                        Totale = 8.75m,
                        Priorita = 2
                    }
                };

                await _context.Ordine.AddRangeAsync(ordini);
                Console.WriteLine("✅ Ordini seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedOrdiniAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedOrderItemsAsync()
        {
            if (await _context.OrderItem.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var ordini = await _context.Ordine.ToListAsync();
                var articoli = await _context.Articolo.ToListAsync();
                var taxRates = await _context.TaxRates.ToListAsync();

                // ✅ Verifica che ci siano abbastanza ordini
                if (ordini.Count < 2)
                {
                    Console.WriteLine("⚠️  Ordini insufficienti per OrderItems (servono almeno 2)");
                    return;
                }

                // ✅ Usa accesso per indice invece di Skip().FirstAsync()
                var ordine1 = ordini[0];
                var ordine2 = ordini[1];

                // ✅ Filtra articoli per tipo in memoria
                var articoliBevanda = articoli.Where(a => a.Tipo == "bevanda").ToList();
                var articoliDolce = articoli.Where(a => a.Tipo == "dolce").ToList();

                // ✅ Verifica che ci siano abbastanza articoli
                if (articoliBevanda.Count < 2)
                {
                    Console.WriteLine("⚠️  Articoli bevanda insufficienti per OrderItems (servono almeno 2)");
                    return;
                }

                if (!articoliDolce.Any())
                {
                    Console.WriteLine("⚠️  Nessun articolo dolce trovato per OrderItems");
                    return;
                }

                var articoloBevanda1 = articoliBevanda[0];
                var articoloBevanda2 = articoliBevanda[1];
                var articoloDolce = articoliDolce.FirstOrDefault();

                // ✅ CORREZIONE WARNING: Verifica esplicita che articoloDolce non sia null
                if (articoloDolce == null)
                {
                    Console.WriteLine("⚠️  Articolo dolce non trovato per OrderItems");
                    return;
                }

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var taxRateStandard = taxRates.FirstOrDefault(t => t.Aliquota == 22.00m);

                // ✅ Verifica che la tax rate esista
                if (taxRateStandard == null)
                {
                    Console.WriteLine("⚠️  Tax rate standard non trovato per OrderItems");
                    return;
                }

                var orderItems = new[]
                {
                    // Ordine 1 - Classic Milk Tea
                    new OrderItem
                    {
                        OrdineId = ordine1.OrdineId,
                        ArticoloId = articoloBevanda1.ArticoloId,
                        Quantita = 2,
                        PrezzoUnitario = 4.50m,
                        ScontoApplicato = 0.00m,
                        Imponibile = 9.00m,
                        TotaleIvato = 10.98m, // 9.00 + 22% IVA
                        TaxRateId = taxRateStandard.TaxRateId,
                        TipoArticolo = "bevanda",
                        DataCreazione = DateTime.UtcNow.AddHours(-2),
                        DataAggiornamento = DateTime.UtcNow.AddHours(-2)
                    },
                    // Ordine 1 - Tiramisù
                    new OrderItem
                    {
                        OrdineId = ordine1.OrdineId,
                        ArticoloId = articoloDolce.ArticoloId, // ✅ ORA SICURO: articoloDolce non è null
                        Quantita = 1,
                        PrezzoUnitario = 5.50m,
                        ScontoApplicato = 0.00m,
                        Imponibile = 5.50m,
                        TotaleIvato = 6.71m, // 5.50 + 22% IVA
                        TaxRateId = taxRateStandard.TaxRateId,
                        TipoArticolo = "dolce",
                        DataCreazione = DateTime.UtcNow.AddHours(-2),
                        DataAggiornamento = DateTime.UtcNow.AddHours(-2)
                    },
                    // Ordine 2 - Fruit Fusion
                    new OrderItem
                    {
                        OrdineId = ordine2.OrdineId,
                        ArticoloId = articoloBevanda2.ArticoloId,
                        Quantita = 1,
                        PrezzoUnitario = 5.50m,
                        ScontoApplicato = 0.50m, // Sconto applicato
                        Imponibile = 5.00m,
                        TotaleIvato = 6.10m, // 5.00 + 22% IVA
                        TaxRateId = taxRateStandard.TaxRateId,
                        TipoArticolo = "bevanda",
                        DataCreazione = DateTime.UtcNow.AddHours(-1),
                        DataAggiornamento = DateTime.UtcNow.AddHours(-1)
                    }
                };

                await _context.OrderItem.AddRangeAsync(orderItems);
                Console.WriteLine($"✅ OrderItems seeded successfully ({orderItems.Length} records)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedOrderItemsAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedConfigSoglieTempiAsync()
        {
            if (await _context.ConfigSoglieTempi.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var statiOrdine = await _context.StatoOrdine.ToListAsync();

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var statoInAttesa = statiOrdine.FirstOrDefault(s => s.StatoOrdine1 == "In Attesa");
                var statoInPreparazione = statiOrdine.FirstOrDefault(s => s.StatoOrdine1 == "In Preparazione");
                var statoPronto = statiOrdine.FirstOrDefault(s => s.StatoOrdine1 == "Pronto");

                // ✅ Verifica che gli stati esistano
                if (statoInAttesa == null || statoInPreparazione == null || statoPronto == null)
                {
                    Console.WriteLine("⚠️  Stati ordine non trovati per ConfigSoglieTempi");
                    return;
                }

                var soglieTempi = new[]
                {
                    new ConfigSoglieTempi
                    {
                        StatoOrdineId = statoInAttesa.StatoOrdineId,
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
                        StatoOrdineId = statoPronto.StatoOrdineId,
                        SogliaAttenzione = 5,    // minuti
                        SogliaCritico = 15,      // minuti
                        DataAggiornamento = DateTime.UtcNow,
                        UtenteAggiornamento = "system"
                    }
                };

                await _context.ConfigSoglieTempi.AddRangeAsync(soglieTempi);
                Console.WriteLine("✅ ConfigSoglieTempi seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedConfigSoglieTempiAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedStatoStoricoOrdiniAsync()
        {
            if (await _context.StatoStoricoOrdine.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var ordini = await _context.Ordine.ToListAsync();
                var statiOrdine = await _context.StatoOrdine.ToListAsync();

                // ✅ Verifica che ci siano abbastanza ordini
                if (ordini.Count < 2)
                {
                    Console.WriteLine("⚠️  Ordini insufficienti per StatoStoricoOrdini (servono almeno 2)");
                    return;
                }

                // ✅ Usa accesso per indice invece di Skip().FirstAsync()
                var ordine1 = ordini[0];
                var ordine2 = ordini[1];

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var statoInAttesa = statiOrdine.FirstOrDefault(s => s.StatoOrdine1 == "In Attesa");
                var statoInPreparazione = statiOrdine.FirstOrDefault(s => s.StatoOrdine1 == "In Preparazione");
                var statoPronto = statiOrdine.FirstOrDefault(s => s.StatoOrdine1 == "Pronto");

                // ✅ Verifica che tutti gli stati esistano
                if (statoInAttesa == null || statoInPreparazione == null || statoPronto == null)
                {
                    Console.WriteLine("⚠️  Stati ordine non trovati per StatoStoricoOrdini");
                    return;
                }

                var storicoOrdini = new[]
                {
                    // Ordine 1 - Cronologia completa
                    new StatoStoricoOrdine
                    {
                        OrdineId = ordine1.OrdineId,
                        StatoOrdineId = statoInAttesa.StatoOrdineId,
                        Inizio = DateTime.UtcNow.AddHours(-2),
                        Fine = DateTime.UtcNow.AddHours(-1) // Passato a In Preparazione dopo 1 ora
                    },
                    new StatoStoricoOrdine
                    {
                        OrdineId = ordine1.OrdineId,
                        StatoOrdineId = statoInPreparazione.StatoOrdineId,
                        Inizio = DateTime.UtcNow.AddHours(-1),
                        Fine = DateTime.UtcNow.AddMinutes(-30) // Passato a Pronto dopo 30 minuti
                    },
                    new StatoStoricoOrdine
                    {
                        OrdineId = ordine1.OrdineId,
                        StatoOrdineId = statoPronto.StatoOrdineId,
                        Inizio = DateTime.UtcNow.AddMinutes(-30)
                        // Fine = null -> Stato corrente
                    },

                    // Ordine 2 - Ancora in attesa
                    new StatoStoricoOrdine
                    {
                        OrdineId = ordine2.OrdineId,
                        StatoOrdineId = statoInAttesa.StatoOrdineId,
                        Inizio = DateTime.UtcNow.AddHours(-1)
                        // Fine = null -> Stato corrente
                    }
                };

                await _context.StatoStoricoOrdine.AddRangeAsync(storicoOrdini);
                Console.WriteLine($"✅ StatoStoricoOrdini seeded successfully ({storicoOrdini.Length} records)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedStatoStoricoOrdiniAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedPreferitiClienteAsync()
        {
            if (await _context.PreferitiCliente.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var clienti = await _context.Cliente.ToListAsync();
                var bevandeStandard = await _context.BevandaStandard.ToListAsync();

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var cliente = clienti.FirstOrDefault();

                // ✅ Cerca le bevande per prezzo usando FirstOrDefault
                var bevandaClassic = bevandeStandard.FirstOrDefault(b => b.Prezzo == 4.50m);
                var bevandaFruit = bevandeStandard.FirstOrDefault(b => b.Prezzo == 5.50m);

                // ✅ Verifica che tutte le entità esistano
                if (cliente == null)
                {
                    Console.WriteLine("⚠️  Cliente non trovato per PreferitiCliente");
                    return;
                }

                if (bevandaClassic == null || bevandaFruit == null)
                {
                    Console.WriteLine("⚠️  Bevande standard non trovate per PreferitiCliente");
                    return;
                }

                var preferiti = new[]
                {
                    new PreferitiCliente
                    {
                        ClienteId = cliente.ClienteId,
                        BevandaId = bevandaClassic.ArticoloId,
                        DataAggiunta = DateTime.UtcNow.AddDays(-7)
                    },
                    new PreferitiCliente
                    {
                        ClienteId = cliente.ClienteId,
                        BevandaId = bevandaFruit.ArticoloId,
                        DataAggiunta = DateTime.UtcNow.AddDays(-3)
                    }
                };

                await _context.PreferitiCliente.AddRangeAsync(preferiti);
                Console.WriteLine("✅ PreferitiCliente seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedPreferitiClienteAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedSessioniQrAsync()
        {
            if (await _context.SessioniQr.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var tavoli = await _context.Tavolo.ToListAsync();
                var clienti = await _context.Cliente.ToListAsync();

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var tavolo1 = tavoli.FirstOrDefault(t => t.Numero == 1);
                var cliente = clienti.FirstOrDefault();

                // ✅ Verifica che le entità esistano
                if (tavolo1 == null)
                {
                    Console.WriteLine("⚠️  Tavolo 1 non trovato per SessioniQr");
                    return;
                }

                if (cliente == null)
                {
                    Console.WriteLine("⚠️  Cliente non trovato per SessioniQr");
                    return;
                }

                var sessioniQr = new[]
                {
                    new SessioniQr
                    {
                        SessioneId = Guid.NewGuid(),
                        ClienteId = cliente.ClienteId,
                        QrCode = $"QR_{Guid.NewGuid()}",
                        DataCreazione = DateTime.UtcNow,
                        DataScadenza = DateTime.UtcNow.AddHours(2),
                        Utilizzato = true,
                        DataUtilizzo = DateTime.UtcNow.AddMinutes(5),
                        TavoloId = tavolo1.TavoloId,
                        CodiceSessione = $"SESS_{DateTime.UtcNow:yyyyMMddHHmmss}",
                        Stato = "Completata"
                    },
                    new SessioniQr
                    {
                        SessioneId = Guid.NewGuid(),
                        ClienteId = null, // Sessione non ancora associata
                        QrCode = $"QR_{Guid.NewGuid()}",
                        DataCreazione = DateTime.UtcNow,
                        DataScadenza = DateTime.UtcNow.AddHours(1),
                        Utilizzato = false,
                        DataUtilizzo = null,
                        TavoloId = tavolo1.TavoloId,
                        CodiceSessione = $"SESS_{DateTime.UtcNow.AddMinutes(1):yyyyMMddHHmmss}",
                        Stato = "Attiva"
                    }
                };

                await _context.SessioniQr.AddRangeAsync(sessioniQr);
                Console.WriteLine("✅ SessioniQr seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedSessioniQrAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedLogAccessiAsync()
        {
            if (await _context.LogAccessi.AnyAsync()) return;

            try
            {
                // ✅ CORREZIONE: Carica tutto in memoria prima di filtrare
                var utenti = await _context.Utenti.ToListAsync();
                var clienti = await _context.Cliente.ToListAsync();

                // ✅ Usa FirstOrDefault invece di FirstAsync per InMemory
                var utente = utenti.FirstOrDefault();
                var cliente = clienti.FirstOrDefault();

                // ✅ Verifica che le entità esistano (cliente può essere null per alcuni log)
                if (utente == null)
                {
                    Console.WriteLine("⚠️  Utente non trovato per LogAccessi");
                    return;
                }

                var logAccessi = new List<LogAccessi>
                {
                    new LogAccessi
                    {
                        UtenteId = utente.UtenteId,
                        ClienteId = null,
                        TipoAccesso = "Login",
                        Esito = "Successo",
                        IpAddress = "192.168.1.100",
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                        DataCreazione = DateTime.UtcNow.AddHours(-3),
                        Dettagli = "Accesso amministratore al sistema"
                    },
                    new LogAccessi
                    {
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

                // ✅ Aggiungi il log con cliente solo se cliente esiste
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
                Console.WriteLine($"✅ LogAccessi seeded successfully ({logAccessi.Count} records)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Errore in SeedLogAccessiAsync: {ex.Message}");
                // Continua senza bloccare tutto il seeding
            }
        }

        private async Task SeedLogAttivitaAsync()
        {
            if (await _context.LogAttivita.AnyAsync()) return;

            var logAttivita = new[]
            {
                new LogAttivita
                {
                    TipoAttivita = "Sistema",
                    Descrizione = "Avvio applicazione",
                    DataEsecuzione = DateTime.UtcNow.AddHours(-4),
                    Dettagli = "Sistema avviato correttamente"
                },
                new LogAttivita
                {
                    TipoAttivita = "Database",
                    Descrizione = "Pulizia cache",
                    DataEsecuzione = DateTime.UtcNow.AddHours(-3),
                    Dettagli = "Cache pulita automaticamente"
                },
                new LogAttivita
                {
                    TipoAttivita = "Ordine",
                    Descrizione = "Nuovo ordine creato",
                    DataEsecuzione = DateTime.UtcNow.AddHours(-2),
                    Dettagli = "Ordine #1 creato dal cliente"
                },
                new LogAttivita
                {
                    TipoAttivita = "Ordine",
                    Descrizione = "Stato ordine aggiornato",
                    DataEsecuzione = DateTime.UtcNow.AddHours(-1),
                    Dettagli = "Ordine #1 passato in preparazione"
                },
                new LogAttivita
                {
                    TipoAttivita = "Sistema",
                    Descrizione = "Backup automatico",
                    DataEsecuzione = DateTime.UtcNow.AddMinutes(-30),
                    Dettagli = "Backup database completato"
                }
            };

            await _context.LogAttivita.AddRangeAsync(logAttivita);
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
                    Priorita = 2
                },
                new NotificheOperative
                {
                    DataCreazione = DateTime.UtcNow.AddHours(-1),
                    OrdiniCoinvolti = "1",
                    Messaggio = "Ingrediente 'Perle di tapioca' in esaurimento",
                    Stato = "Risolta",
                    DataGestione = DateTime.UtcNow.AddMinutes(-30),
                    UtenteGestione = "gestore",
                    Priorita = 1
                },
                new NotificheOperative
                {
                    DataCreazione = DateTime.UtcNow.AddMinutes(-15),
                    OrdiniCoinvolti = "",
                    Messaggio = "Sistema di pagamento temporaneamente non disponibile",
                    Stato = "Attiva",
                    DataGestione = null,
                    UtenteGestione = null,
                    Priorita = 3
                },
                new NotificheOperative
                {
                    DataCreazione = DateTime.UtcNow.AddMinutes(-5),
                    OrdiniCoinvolti = "2",
                    Messaggio = "Ordine #2 pronto per la consegna",
                    Stato = "Attiva",
                    DataGestione = null,
                    UtenteGestione = null,
                    Priorita = 2
                }
            };

            await _context.NotificheOperative.AddRangeAsync(notifiche);
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
        }
    }
}