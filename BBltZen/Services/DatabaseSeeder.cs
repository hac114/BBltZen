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

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ✅ ORDINE CORRETTO per foreign keys
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

                // ✅ PRIMA gli Articoli (base per molte entità)
                await SeedArticoliAsync();

                // ✅ POI le Personalizzazioni (sia standard che custom)
                await SeedPersonalizzazioniAsync();
                await SeedPersonalizzazioneIngredientiAsync();
                await SeedDimensioneQuantitaIngredientiAsync();
                await SeedPersonalizzazioniCustomAsync();
                await SeedIngredientiPersonalizzazioneAsync();

                // ✅ INFINE le entità che dipendono da Articolo + altre
                await SeedBevandeStandardAsync();     // Dipende da Articolo + Personalizzazione + DimensioneBicchiere
                await SeedBevandeCustomAsync();       // Dipende da Articolo + PersonalizzazioneCustom  
                await SeedDolciAsync();               // Dipende da Articolo

                // ✅ Clienti e Utenti (dipendono da Tavolo)
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
                await transaction.CommitAsync();

                Console.WriteLine("✅ Database seeded successfully!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
                    Tipo = "bevanda",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                },
                new Articolo
                {
                    Tipo = "bevanda",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                },
                new Articolo
                {
                    Tipo = "bevanda",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                },
                new Articolo
                {
                    Tipo = "dolce",
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                }
            };

            await _context.Articolo.AddRangeAsync(articoli);
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

            // Recupera entità correlate
            var personalizzazioneClassic = await _context.Personalizzazione.FirstAsync(p => p.Nome == "Classic Milk Tea");
            var personalizzazioneFruit = await _context.Personalizzazione.FirstAsync(p => p.Nome == "Fruit Fusion");

            var teaNero = await _context.Ingrediente.FirstAsync(i => i.Ingrediente1 == "Tea nero premium");
            var teaVerde = await _context.Ingrediente.FirstAsync(i => i.Ingrediente1 == "Tea verde special");
            var caramello = await _context.Ingrediente.FirstAsync(i => i.Ingrediente1 == "Sciroppo di caramello");
            var latteCocco = await _context.Ingrediente.FirstAsync(i => i.Ingrediente1 == "Latte di cocco");

            var grUnit = await _context.UnitaDiMisura.FirstAsync(u => u.Sigla == "GR");
            var mlUnit = await _context.UnitaDiMisura.FirstAsync(u => u.Sigla == "ML");

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
        }

        private async Task SeedDimensioneQuantitaIngredientiAsync()
        {
            if (await _context.DimensioneQuantitaIngredienti.AnyAsync()) return;

            // Recupera entità correlate
            var personalizzazioneIngredienti = await _context.PersonalizzazioneIngrediente.ToListAsync();
            var dimensioneMedia = await _context.DimensioneBicchiere.FirstAsync(d => d.Sigla == "M");
            var dimensioneLarge = await _context.DimensioneBicchiere.FirstAsync(d => d.Sigla == "L");

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
        }

        private async Task SeedPersonalizzazioniCustomAsync()
        {
            if (await _context.PersonalizzazioneCustom.AnyAsync()) return;

            var dimensioneMedia = await _context.DimensioneBicchiere.FirstAsync(d => d.Sigla == "M");
            var dimensioneLarge = await _context.DimensioneBicchiere.FirstAsync(d => d.Sigla == "L");

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
        }

        private async Task SeedIngredientiPersonalizzazioneAsync()
        {
            if (await _context.IngredientiPersonalizzazione.AnyAsync()) return;

            // Recupera entità correlate
            var customTea = await _context.PersonalizzazioneCustom.FirstAsync(p => p.Nome == "My Custom Tea");
            var extraSweet = await _context.PersonalizzazioneCustom.FirstAsync(p => p.Nome == "Extra Sweet Mix");

            var teaNero = await _context.Ingrediente.FirstAsync(i => i.Ingrediente1 == "Tea nero premium");
            var caramello = await _context.Ingrediente.FirstAsync(i => i.Ingrediente1 == "Sciroppo di caramello");
            var perleTapioca = await _context.Ingrediente.FirstAsync(i => i.Ingrediente1 == "Perle di tapioca");

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
        }

        private async Task SeedBevandeStandardAsync()
        {
            if (await _context.BevandaStandard.AnyAsync()) return;

            // Recupera entità correlate
            var articoloBevanda1 = await _context.Articolo.FirstAsync(a => a.Tipo == "bevanda");
            var articoloBevanda2 = await _context.Articolo.Skip(1).FirstAsync(a => a.Tipo == "bevanda");

            var personalizzazioneClassic = await _context.Personalizzazione.FirstAsync(p => p.Nome == "Classic Milk Tea");
            var personalizzazioneFruit = await _context.Personalizzazione.FirstAsync(p => p.Nome == "Fruit Fusion");

            var dimensioneMedia = await _context.DimensioneBicchiere.FirstAsync(d => d.Sigla == "M");
            var dimensioneLarge = await _context.DimensioneBicchiere.FirstAsync(d => d.Sigla == "L");

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
        }

        private async Task SeedBevandeCustomAsync()
        {
            if (await _context.BevandaCustom.AnyAsync()) return;

            // Recupera entità correlate
            var articoloBevanda3 = await _context.Articolo.Skip(2).FirstAsync(a => a.Tipo == "bevanda");
            var customTea = await _context.PersonalizzazioneCustom.FirstAsync(p => p.Nome == "My Custom Tea");
            var extraSweet = await _context.PersonalizzazioneCustom.FirstAsync(p => p.Nome == "Extra Sweet Mix");

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
        }

        private async Task SeedDolciAsync()
        {
            if (await _context.Dolce.AnyAsync()) return;

            var articoloDolce = await _context.Articolo.FirstAsync(a => a.Tipo == "dolce");

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

            // Recupera entità correlate
            var cliente = await _context.Cliente.FirstAsync();

            // ✅ CORRETTO: Usa 'StatoOrdine1' invece di 'NomeStatoOrdine'
            var statoOrdineInAttesa = await _context.StatoOrdine.FirstAsync(s => s.StatoOrdine1 == "In Attesa");

            // ✅ CORRETTO: Usa 'StatoPagamento1' invece di 'NomeStatoPagamento'  
            var statoPagamentoPending = await _context.StatoPagamento.FirstAsync(s => s.StatoPagamento1 == "Pending");

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
        }

        private async Task SeedOrderItemsAsync()
        {
            if (await _context.OrderItem.AnyAsync()) return;

            // Recupera entità correlate
            var ordine1 = await _context.Ordine.FirstAsync();
            var ordine2 = await _context.Ordine.Skip(1).FirstAsync();

            var articoloBevanda1 = await _context.Articolo.FirstAsync(a => a.Tipo == "bevanda");
            var articoloBevanda2 = await _context.Articolo.Skip(1).FirstAsync(a => a.Tipo == "bevanda");
            var articoloDolce = await _context.Articolo.FirstAsync(a => a.Tipo == "dolce");

            var taxRateStandard = await _context.TaxRates.FirstAsync(t => t.Aliquota == 22.00m);

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
                    ArticoloId = articoloDolce.ArticoloId,
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
        }

        private async Task SeedConfigSoglieTempiAsync()
        {
            if (await _context.ConfigSoglieTempi.AnyAsync()) return;

            // Recupera stati ordine
            var statoInAttesa = await _context.StatoOrdine.FirstAsync(s => s.StatoOrdine1 == "In Attesa");
            var statoInPreparazione = await _context.StatoOrdine.FirstAsync(s => s.StatoOrdine1 == "In Preparazione");
            var statoPronto = await _context.StatoOrdine.FirstAsync(s => s.StatoOrdine1 == "Pronto");

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
        }

        private async Task SeedStatoStoricoOrdiniAsync()
        {
            if (await _context.StatoStoricoOrdine.AnyAsync()) return;

            // Recupera entità correlate
            var ordine1 = await _context.Ordine.FirstAsync();
            var ordine2 = await _context.Ordine.Skip(1).FirstAsync();

            var statoInAttesa = await _context.StatoOrdine.FirstAsync(s => s.StatoOrdine1 == "In Attesa");
            var statoInPreparazione = await _context.StatoOrdine.FirstAsync(s => s.StatoOrdine1 == "In Preparazione");
            var statoPronto = await _context.StatoOrdine.FirstAsync(s => s.StatoOrdine1 == "Pronto");

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
        }

        private async Task SeedPreferitiClienteAsync()
        {
            if (await _context.PreferitiCliente.AnyAsync()) return;

            // Recupera entità correlate
            var cliente = await _context.Cliente.FirstAsync();
            var bevandaClassic = await _context.BevandaStandard.FirstAsync(b => b.Prezzo == 4.50m);
            var bevandaFruit = await _context.BevandaStandard.FirstAsync(b => b.Prezzo == 5.50m);

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
        }

        private async Task SeedSessioniQrAsync()
        {
            if (await _context.SessioniQr.AnyAsync()) return;

            // Recupera entità correlate
            var tavolo1 = await _context.Tavolo.FirstAsync(t => t.Numero == 1);
            var cliente = await _context.Cliente.FirstAsync();

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
        }

        private async Task SeedLogAccessiAsync()
        {
            if (await _context.LogAccessi.AnyAsync()) return;

            // Recupera entità correlate
            var utente = await _context.Utenti.FirstAsync();
            var cliente = await _context.Cliente.FirstAsync();

            var logAccessi = new[]
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
                    UtenteId = null,
                    ClienteId = cliente.ClienteId,
                    TipoAccesso = "Registrazione",
                    Esito = "Successo",
                    IpAddress = "192.168.1.150",
                    UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X)",
                    DataCreazione = DateTime.UtcNow.AddHours(-2),
                    Dettagli = "Nuovo cliente registrato tramite QR code"
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

            await _context.LogAccessi.AddRangeAsync(logAccessi);
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