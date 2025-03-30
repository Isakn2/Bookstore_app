// BookGenerator.cs
using Bogus;
using System.Linq;
using System.Collections.Generic;
using BookStoreTester.Models;

namespace BookStoreTester.Services
{
    public class BookGenerator
    {
        public List<Book> GenerateBooks(BookRequest request)
        {
            var combinedSeed = request.Seed + request.Page;
            Randomizer.Seed = new Random(combinedSeed);

            var validLocale = request.Locale switch
            {
                "de" => "de",
                "fr" => "fr",
                "en" => "en",
                _ => "en"
            };

            var faker = new Faker(validLocale);

            var bookFaker = new Faker<Book>(locale: validLocale)
                .UseSeed(combinedSeed)
                .RuleFor(b => b.Index, (f, b) => (request.Page - 1) * request.PageSize + f.IndexGlobal + 1)
                .RuleFor(b => b.ISBN, f => GenerateISBN(f, validLocale))
                .RuleFor(b => b.Title, f => GenerateTitle(f, validLocale))
                .RuleFor(b => b.Author, f => GenerateAuthor(f, validLocale))
                .RuleFor(b => b.Publisher, f => GeneratePublisher(f, validLocale))
                .RuleFor(b => b.AverageLikes, request.AvgLikes)
                .RuleFor(b => b.AverageReviews, request.AvgReviews)
                .RuleFor(b => b.ActualLikes, f => CalculateActualLikes(f, request.AvgLikes))
                .RuleFor(b => b.Reviews, f => GenerateReviews(f, validLocale, request.AvgReviews))
                .RuleFor(b => b.Reviewers, f => GenerateReviewers(f, request.AvgReviews))
                .RuleFor(b => b.CoverImageUrl, f => GenerateCoverImageUrl(f, validLocale));

            return bookFaker.Generate(request.PageSize);
        }

        private string GenerateAuthor(Faker f, string locale)
        {
            return locale switch
            {
                "de" => $"{f.Name.LastName()}, {f.Name.FirstName()}",
                "fr" => $"{f.Name.FirstName()} {f.Name.LastName()}",
                _ => $"{f.Name.FirstName()} {f.Name.LastName()}"
            };
        }

        private int CalculateActualLikes(Faker f, double avgLikes)
        {
            var baseLikes = (int)Math.Floor(avgLikes);
            var probability = avgLikes - baseLikes;
            return baseLikes + (f.Random.Double() < probability ? 1 : 0);
        }

        private string GeneratePublisher(Faker f, string locale)
        {
            var publishers = locale switch
            {
                "de" => new[] {
                    $"{f.Company.CompanyName()} Verlag",
                    "Goldblatt Bücher",
                    "Nordlicht Verlag",
                    "Tintenherz Press",
                    "Silberfeder Medien"
                },
                "fr" => new[] {
                    $"Éditions {f.Company.CompanyName()}",
                    "Plume d'Or",
                    "Livre de Poche",
                    "Éditions du Soleil",
                    "Presse Littéraire"
                },
                _ => new[] {
                    $"{f.Company.CompanyName()} Publishing",
                    "Golden Quill Press",
                    "Midnight Ink",
                    "Royal Pages Publishing",
                    "Inkwell Classics"
                }
            };
            
            return f.PickRandom(publishers);
        }

        private List<string> GenerateReviewers(Faker f, double avgReviews)
        {
            var reviewers = new List<string>();
            var reviewCount = CalculateActualLikes(f, avgReviews);
            
            for (int i = 0; i < reviewCount; i++)
            {
                reviewers.Add(f.Name.FullName());
            }
            return reviewers;
        }

        private string GenerateTitle(Faker f, string locale)
        {
            // First generate a title
            string title = locale switch
            {
                "de" => GenerateGermanTitle(f),
                "fr" => GenerateFrenchTitle(f),
                _ => GenerateEnglishTitle(f)
            };
            
            // Add optional subtitle 30% of the time
            if (f.Random.Bool(0.3f))
            {
                string subtitle = locale switch
                {
                    "de" => $" - {f.PickRandom(new[] {"Eine Geschichte", "Die Chroniken", "Die wahre Geschichte"})}",
                    "fr" => $" - {f.PickRandom(new[] {"Une histoire", "Les chroniques", "La véritable histoire"})}",
                    _ => $" - {f.PickRandom(new[] {"A Story", "The Chronicles", "The True Account"})}"
                };
                title += subtitle;
            }
            
            return title;
        }

        private string GenerateEnglishTitle(Faker f)
        {
            var patterns = new[]
            {
                "{Adjective} {Noun}",
                "The {Noun} of {Place}",
                "{Name}'s {Adjective} {Noun}",
                "When {Noun} {Verb}",
                "{Adjective} {Noun}: A {Noun} Story",
                "The {Adjective} {Noun} Chronicles",
                "{Place}'s {Adjective} Secret",
                "A {Adjective} {Noun}'s Journey",
                "The {Noun} That {Verb} the World",
                "{Name} and the {Adjective} {Noun}"
            };

            string[] adjectives = { 
                "Lost", "Forgotten", "Golden", "Dark", "Mysterious", "Secret", "Ancient",
                "Hidden", "Crimson", "Silent", "Broken", "Eternal", "Whispering", "Cursed",
                "Sacred", "Vanishing", "Lonely", "Burning", "Frozen", "Shattered", "Last",
                "First", "Final", "Undying", "Unseen", "Spectral", "Emerald", "Ivory", "Obsidian"
            };
            
            string[] nouns = { 
                "Dragon", "Castle", "King", "Sword", "Journey", "Prophet", "Kingdom",
                "Throne", "Crown", "Legacy", "Shadow", "Phoenix", "Oracle", "Witch",
                "Chronicle", "Legend", "Empire", "Reckoning", "Sanctuary", "Oath", "Promise",
                "Codex", "Manuscript", "Tome", "Grimoire", "Relic", "Artifact", "Heirloom"
            };
            
            string[] places = { 
                "the Mountains", "the Forest", "the River", "the Castle", "Time", "the Stars",
                "the Ruins", "the Desert", "the Abyss", "the Cosmos", "the Void", "the Tides",
                "the Eclipse", "the Ashes", "the Storm", "the Horizon", "the Labyrinth",
                "the North", "the Sea", "the Underworld", "the Heavens", "the Mists"
            };
            
            string[] verbs = { 
                "Falls", "Rises", "Returns", "Whispers", "Burns", "Awakens", "Fades",
                "Shatters", "Ends", "Begins", "Crumbles", "Ascends", "Descends", "Vanishes",
                "Changes", "Remembers", "Forgets", "Destroys", "Creates", "Abandons"
            };

            var pattern = f.PickRandom(patterns);
            string title = pattern
                .Replace("{Adjective}", f.PickRandom(adjectives))
                .Replace("{Noun}", f.PickRandom(nouns))
                .Replace("{Place}", f.PickRandom(places))
                .Replace("{Verb}", f.PickRandom(verbs))
                .Replace("{Name}", f.Name.FirstName());

            // 40% chance to add a subtitle
            if (f.Random.Bool(0.4f))
            {
                string[] subtitles = {
                    "A Tale of {Adjective} {Noun}",
                    "The {Adjective} Chronicles",
                    "Book {Number} of the {Adjective} {Noun}",
                    "The {Adjective} {Noun} Saga",
                    "A {Noun}'s Journey"
                };
                
                string subtitle = f.PickRandom(subtitles)
                    .Replace("{Adjective}", f.PickRandom(adjectives))
                    .Replace("{Noun}", f.PickRandom(nouns))
                    .Replace("{Number}", f.PickRandom(new[]{"I", "II", "III", "IV", "V"}));
                    
                title += ": " + subtitle;
            }

            return title;
        }

        private string GenerateGermanTitle(Faker f)
        {
            var patterns = new[]
            {
                "{Adjektiv} {Nomen}",
                "Das {Nomen} von {Ort}",
                "{Name}s {Adjektiv} {Nomen}",
                "Als {Nomen} {Verb}",
                "{Adjektiv} {Nomen}: Eine {Nomen} Geschichte",
                "Die {Adjektiv} {Nomen} Chroniken",
                "{Ort}s {Adjektiv} Geheimnis",
                "Die {Nomen} die die Welt {Verb}",
                "{Name} und das {Adjektiv} {Nomen}"
            };

            string[] adjektivs = { 
                "Verloren", "Vergessen", "Goldene", "Dunkle", "Geheimnisvolle", "Alte",
                "Versteckte", "Blutrote", "Stille", "Gebrochene", "Ewige", "Flüsternde",
                "Verfluchte", "Heilige", "Verschwindende", "Einsame", "Brennende", "Letzte",
                "Erste", "Unsterbliche", "Unsichtbare", "Smaragdene", "Elfenbeinerne"
            };

            string[] nomens = { 
                "Drache", "Schloss", "König", "Schwert", "Reise", "Prophet", "Königreich",
                "Thron", "Krone", "Vermächtnis", "Schatten", "Phönix", "Orakel", "Hexe",
                "Chronik", "Legende", "Imperium", "Abgrund", "Versprechen", "Kodex",
                "Manuskript", "Foliant", "Relikt", "Artefakt", "Erbstück"
            };

            string[] orts = { 
                "den Bergen", "dem Wald", "dem Fluss", "der Burg", "der Zeit", "den Sternen",
                "den Ruinen", "der Wüste", "dem Abgrund", "dem Kosmos", "der Leere", "den Gezeiten",
                "der Finsternis", "der Asche", "dem Sturm", "dem Horizont", "dem Labyrinth",
                "dem Norden", "der See", "der Unterwelt", "den Himmeln", "den Nebeln"
            };

            string[] verbs = { 
                "Fällt", "Steigt", "Kehrt zurück", "Flüstert", "Brennt", "Erwacht", "Verblasst",
                "Zerbricht", "Endet", "Beginnt", "Zerfällt", "Steigt auf", "Steigt ab", "Verschwindet",
                "Verändert", "Erinnert", "Vergisst", "Zerstört", "Erschafft", "Verlässt"
            };

            var pattern = f.PickRandom(patterns);
            string title = pattern
                .Replace("{Adjektiv}", f.PickRandom(adjektivs))
                .Replace("{Nomen}", f.PickRandom(nomens))
                .Replace("{Ort}", f.PickRandom(orts))
                .Replace("{Verb}", f.PickRandom(verbs))
                .Replace("{Name}", f.Name.FirstName());

            // 40% chance to add a subtitle
            if (f.Random.Bool(0.4f))
            {
                string[] subtitles = {
                    "Eine Geschichte von {Adjektiv} {Nomen}",
                    "Die {Adjektiv} Chroniken",
                    "Buch {Number} der {Adjektiv} {Nomen}",
                    "Die {Adjektiv} {Nomen} Saga",
                    "Eine {Nomen} Reise"
                };
                
                string subtitle = f.PickRandom(subtitles)
                    .Replace("{Adjektiv}", f.PickRandom(adjektivs))
                    .Replace("{Nomen}", f.PickRandom(nomens))
                    .Replace("{Number}", f.PickRandom(new[]{"I", "II", "III", "IV", "V"}));
                    
                title += ": " + subtitle;
            }

            return title;
        }

        private string GenerateFrenchTitle(Faker f)
        {
            var patterns = new[]
            {
                "{Adjectif} {Nom}",
                "Le {Nom} de {Lieu}",
                "{Nom} {Adjectif} de {Nom}",
                "Quand {Nom} {Verbe}",
                "{Adjectif} {Nom}: Une histoire de {Nom}",
                "Les Chroniques {Adjectif} {Nom}",
                "Le {Adjectif} Secret de {Lieu}",
                "Le {Nom} qui {Verbe} le monde",
                "{Nom} et le {Adjectif} {Nom}"
            };

            string[] adjectifs = { 
                "Perdu", "Oublié", "Doré", "Sombre", "Mystérieux", "Secret", "Ancien",
                "Caché", "Pourpre", "Silencieux", "Brisé", "Éternel", "Maudit", "Sacré",
                "Brûlant", "Gelé", "Solitaire", "Dernier", "Premier", "Immortel", "Invisible",
                "Émeraude", "Ivoire"
            };

            string[] noms = { 
                "Dragon", "Château", "Roi", "Épée", "Voyage", "Prophète", "Royaume",
                "Trône", "Couronne", "Héritage", "Ombre", "Phénix", "Oracle", "Sorcière",
                "Légende", "Empire", "Abîme", "Promesse", "Codex", "Manuscrit", "Tome",
                "Relique", "Artéfact"
            };

            string[] lieux = { 
                "les Montagnes", "la Forêt", "la Rivière", "le Château", "le Temps", "les Étoiles",
                "les Ruines", "le Désert", "l'Abîme", "le Cosmos", "le Vide", "les Marées",
                "l'Éclipse", "les Cendres", "la Tempête", "l'Horizon", "le Labyrinthe",
                "le Nord", "la Mer", "les Enfers", "les Cieux", "les Brumes"
            };

            string[] verbes = { 
                "Tombe", "Monte", "Revient", "Murmure", "Brûle", "S'éveille", "S'efface",
                "Se brise", "Finit", "Commence", "S'effondre", "Monte", "Descend", "Disparaît",
                "Change", "Se souvient", "Oublie", "Détruit", "Crée", "Abandonne"
            };

            var pattern = f.PickRandom(patterns);
            string title = pattern
                .Replace("{Adjectif}", f.PickRandom(adjectifs))
                .Replace("{Nom}", f.PickRandom(noms))
                .Replace("{Lieu}", f.PickRandom(lieux))
                .Replace("{Verbe}", f.PickRandom(verbes))
                .Replace("{Name}", f.Name.FirstName());

            // 40% chance to add a subtitle
            if (f.Random.Bool(0.4f))
            {
                string[] subtitles = {
                    "Un conte de {Adjectif} {Nom}",
                    "Les Chroniques {Adjectif}",
                    "Livre {Number} des {Adjectif} {Nom}",
                    "La Saga {Adjectif} {Nom}",
                    "Un voyage de {Nom}"
                };
                
                string subtitle = f.PickRandom(subtitles)
                    .Replace("{Adjectif}", f.PickRandom(adjectifs))
                    .Replace("{Nom}", f.PickRandom(noms))
                    .Replace("{Number}", f.PickRandom(new[]{"I", "II", "III", "IV", "V"}));
                    
                title += ": " + subtitle;
            }

            return title;
        }

        private string GenerateISBN(Faker f, string locale)
        {
            var prefix = locale switch
            {
                "de" => "978-3",
                "fr" => "978-2",
                _ => "978-0"
            };
            
            var digits = f.Random.Digits(9).Select(d => d.ToString()).Aggregate((a, b) => a + b);
            
            var sum = 0;
            for (int i = 0; i < 12; i++)
            {
                var digit = int.Parse((prefix.Replace("-", "") + digits)[i].ToString());
                sum += (i % 2 == 0) ? digit : digit * 3;
            }
            var checkDigit = (10 - (sum % 10)) % 10;
            
            return $"{prefix}-{digits}-{checkDigit}";
        }

        private List<string> GenerateReviews(Faker f, string locale, double avgReviews)
        {
            var reviews = new List<string>();
            var reviewCount = CalculateActualLikes(f, avgReviews);
            var lorem = new Bogus.DataSets.Lorem(locale);

            var reviewTemplates = locale switch
            {
                "de" => new[]
                {
                    "Das Buch hat mich {Reaktion}, besonders weil {Satz} Die {Aspekt} war {Adjektiv}, und {Satz}",
                    "Nachdem ich {Buchtyp} gelesen habe, muss ich sagen: {Anfang} {Satz} Was mich {Reaktion} hat, war {Aspekt}. {Satz}",
                    "{Bewertung}: {Anfang} {Satz} Die {Aspekt} war {Adjektiv}, aber {Satz}",
                    "Als {Lesertyp} kann ich sagen: {Anfang} {Satz} Besonders {Aspekt} hat mich {Reaktion}. {Satz}",
                    "Absolut {Adjektiv}! {Anfang} {Satz} Die {Aspekt} war {Steigerung}, was {Satz}"
                },
                "fr" => new[]
                {
                    "Ce livre m'a {Réaction}, surtout parce que {Phrase} Le {Aspect} était {Adjectif}, et {Phrase}",
                    "Après avoir lu {TypeLivre}, je dois dire: {Début} {Phrase} Ce qui m'a {Réaction}, c'est {Aspect}. {Phrase}",
                    "{Évaluation}: {Début} {Phrase} Le {Aspect} était {Adjectif}, mais {Phrase}",
                    "En tant que {TypeLecteur}, je peux dire: {Début} {Phrase} Particulièrement {Aspect} m'a {Réaction}. {Phrase}",
                    "Absolument {Adjectif}! {Début} {Phrase} Le {Aspect} était {Superlatif}, ce qui {Phrase}"
                },
                _ => new[]
                {
                    "This book made me {Reaction}, especially because {Sentence} The {Aspect} was {Adjective}, and {Sentence}",
                    "After reading {BookType}, I must say: {Opening} {Sentence} What {Reaction} me was {Aspect}. {Sentence}",
                    "{Rating}: {Opening} {Sentence} The {Aspect} was {Adjective}, but {Sentence}",
                    "As a {ReaderType}, I can say: {Opening} {Sentence} Particularly {Aspect} made me {Reaction}. {Sentence}",
                    "Absolutely {Adjective}! {Opening} {Sentence} The {Aspect} was {Superlative}, which {Sentence}"
                }
            };

            var aspects = locale switch
            {
                "de" => new[] { "Handlung", "Charakterentwicklung", "Schreibstil", "Ende", "Atmosphäre", "Dialoge", "Spannungsbogen" },
                "fr" => new[] { "intrigue", "développement des personnages", "style d'écriture", "fin", "ambiance", "dialogues", "arc dramatique" },
                _ => new[] { "plot", "character development", "writing style", "ending", "atmosphere", "dialogues", "story arc" }
            };

            for (int i = 0; i < reviewCount; i++)
            {
                var pattern = f.PickRandom(reviewTemplates);
                var review = pattern;

                if (locale == "de")
                {
                    review = review
                        .Replace("{Adjektiv}", f.PickRandom(new[] { "fantastisch", "toll", "schrecklich", "mittelmäßig", "fesselnd", "langweilig", "beeindruckend", "enttäuschend" }))
                        .Replace("{Reaktion}", f.PickRandom(new[] { "begeistert", "entsetzt", "überrascht", "gerührt", "verwirrt", "gelangweilt", "fasziniert" }))
                        .Replace("{Bewertung}", f.PickRandom(new[] { "5/5 Sterne", "1/5 Stern", "Empfehlenswert", "Nicht empfehlenswert", "Meisterwerk", "Enttäuschung des Jahres", "Bester Roman des Monats" }))
                        .Replace("{Aspekt}", f.PickRandom(aspects))
                        .Replace("{Satz}", lorem.Sentence())
                        .Replace("{Anfang}", f.PickRandom(new[] { "Dieses Werk hat mich tief bewegt.", "Ich war von der ersten Seite an gefesselt.", "Die Geschichte entwickelte sich unerwartet." }))
                        .Replace("{Buchtyp}", f.PickRandom(new[] { "diesen Roman", "diese Novelle", "diese Erzählung", "diesen Thriller" }))
                        .Replace("{Lesertyp}", f.PickRandom(new[] { "langjähriger Leser", "Literaturkritiker", "Buchliebhaber", "Hobbyautor" }))
                        .Replace("{Steigerung}", f.PickRandom(new[] { "herausragend", "enttäuschend", "überraschend", "atemberaubend" }));
                }
                else if (locale == "fr")
                {
                    review = review
                        .Replace("{Adjectif}", f.PickRandom(new[] { "fantastique", "génial", "terrible", "moyen", "captivant", "ennuyeux", "impressionnant", "décevant" }))
                        .Replace("{Réaction}", f.PickRandom(new[] { "enthousiasmé", "consterné", "surpris", "ému", "confus", "ennuyé", "fasciné" }))
                        .Replace("{Évaluation}", f.PickRandom(new[] { "5/5 étoiles", "1/5 étoile", "Recommandé", "Déconseillé", "Chef-d'œuvre", "Déception de l'année", "Meilleur roman du mois" }))
                        .Replace("{Aspect}", f.PickRandom(aspects))
                        .Replace("{Phrase}", lorem.Sentence())
                        .Replace("{Début}", f.PickRandom(new[] { "Cette œuvre m'a profondément touché.", "J'ai été captivé dès la première page.", "L'histoire a pris un tour inattendu." }))
                        .Replace("{TypeLivre}", f.PickRandom(new[] { "ce roman", "cette nouvelle", "ce récit", "ce thriller" }))
                        .Replace("{TypeLecteur}", f.PickRandom(new[] { "lecteur assidu", "critique littéraire", "amateur de livres", "écrivain amateur" }))
                        .Replace("{Superlatif}", f.PickRandom(new[] { "exceptionnel", "décevant", "surprenant", "à couper le souffle" }));
                }
                else
                {
                    review = review
                        .Replace("{Adjective}", f.PickRandom(new[] { "fantastic", "great", "terrible", "mediocre", "engaging", "boring", "impressive", "disappointing" }))
                        .Replace("{Reaction}", f.PickRandom(new[] { "thrilled", "appalled", "surprised", "moved", "confused", "bored", "fascinated" }))
                        .Replace("{Rating}", f.PickRandom(new[] { "5/5 stars", "1/5 star", "Highly recommended", "Not recommended", "Masterpiece", "Disappointment of the year", "Best novel of the month" }))
                        .Replace("{Aspect}", f.PickRandom(aspects))
                        .Replace("{Sentence}", lorem.Sentence())
                        .Replace("{Opening}", f.PickRandom(new[] { "This work deeply moved me.", "I was hooked from the first page.", "The story took an unexpected turn." }))
                        .Replace("{BookType}", f.PickRandom(new[] { "this novel", "this novella", "this story", "this thriller" }))
                        .Replace("{ReaderType}", f.PickRandom(new[] { "longtime reader", "literary critic", "book lover", "amateur writer" }))
                        .Replace("{Superlative}", f.PickRandom(new[] { "outstanding", "disappointing", "surprising", "breathtaking" }));
                }

                // Capitalize first letter and ensure proper punctuation
                review = char.ToUpper(review[0]) + review.Substring(1);
                if (!review.EndsWith(".") && !review.EndsWith("!") && !review.EndsWith("?"))
                {
                    review += ".";
                }

                reviews.Add(review);
            }

            return reviews;
        }

        private string GenerateCoverImageUrl(Faker f, string locale)
        {
            // Generate a random but consistent image ID based on book title
            string title = GenerateTitle(f, locale);
            int imageId = Math.Abs(title.GetHashCode()) % 1000; // Ensure ID is within picsum's range
            
            return $"https://picsum.photos/seed/book_{imageId}/300/450";
        }

        private string DetermineGenre(string title)
        {
            // Simple genre detection based on title keywords
            if (title.Contains("Dragon") || title.Contains("Magic")) return "Fantasy";
            if (title.Contains("Space") || title.Contains("AI")) return "Sci-Fi";
            if (title.Contains("Love") || title.Contains("Heart")) return "Romance";
            if (title.Contains("Murder") || title.Contains("Secret")) return "Mystery";
            return "General";
        }

        private string GetGenreColor(string genre, int titleHash)
        {
            // Base color by genre with variations based on title hash
            return (genre, (titleHash % 5)) switch
            {
                ("Fantasy", _) => ShiftColor("5F4B8B", titleHash),  // Purple base
                ("Sci-Fi", _)  => ShiftColor("2A5C8A", titleHash),  // Blue base
                ("Romance", _) => ShiftColor("C74375", titleHash),  // Pink base
                ("Mystery", _) => ShiftColor("4A4A4A", titleHash),  // Dark gray
                (_, _)         => ShiftColor("6B8E23", titleHash)   // Olive default
            };
        }

        private string ShiftColor(string baseHex, int variationSeed)
        {
            // Create subtle variations while maintaining genre color family
            var rnd = new Random(variationSeed);
            int r = Convert.ToInt32(baseHex.Substring(0, 2), 16);
            int g = Convert.ToInt32(baseHex.Substring(2, 2), 16);
            int b = Convert.ToInt32(baseHex.Substring(4, 2), 16);
            
            // Adjust components by ±15%
            r = Math.Clamp(r + rnd.Next(-40, 40), 30, 220);
            g = Math.Clamp(g + rnd.Next(-40, 40), 30, 220);
            b = Math.Clamp(b + rnd.Next(-40, 40), 30, 220);
            
            return $"{r:X2}{g:X2}{b:X2}";
        }

        private string GetBackgroundPattern(int titleHash)
        {
            // Add subtle patterns to break monotony
            return (titleHash % 7) switch
            {
                0 => "&pattern=bricks&pattern_size=40&pattern_opacity=10",
                1 => "&pattern=fish-scales&pattern_size=30&pattern_opacity=15",
                2 => "&pattern=topography&pattern_opacity=12",
                3 => "&pattern=circles&pattern_size=20&pattern_opacity=8",
                4 => "&pattern=lines&pattern_angle=45&pattern_opacity=5",
                _ => "" // No pattern for majority of cases
            };
        }

        private string GetOptimalTextColor(string bgColor)
        {
            // Calculate luminance and choose white or black text
            int r = Convert.ToInt32(bgColor.Substring(0, 2), 16);
            int g = Convert.ToInt32(bgColor.Substring(2, 2), 16);
            int b = Convert.ToInt32(bgColor.Substring(4, 2), 16);
            double luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
            return luminance > 0.6 ? "000000" : "FFFFFF"; // Black on light, white on dark
        }
    }
}